using Android.App;
using Android.Content.PM;
using Android.Gms.Maps;
using Android.Gms.Maps.Model; // Thư viện để làm Ghim (Marker) và Camera bản đồ
using Android.Gms.Tasks; // Thư viện để lắng nghe dữ liệu trả về
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Firebase.Firestore; // Thư viện đọc dữ liệu Firebase
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Navigation;
using Org.Json; // Thư viện đọc JSON có sẵn của Android
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Content;


namespace poimap
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    // CHÚ Ý: Đã thêm IOnSuccessListener để app biết cách "nghe" dữ liệu từ mạng
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback, IOnSuccessListener, Android.Locations.ILocationListener
    {
        private GoogleMap? _map;
        private BottomNavigationView? _bottomNavigation;
        private HorizontalScrollView? _categoryBar;
        private Android.Widget.LinearLayout? _searchLayout;

        private SupportMapFragment? _mapFragment;
        private ProfileFragment? _profileFragment;
        private ToursFragment? _toursFragment;
        private AndroidX.Fragment.App.Fragment? _activeFragment;
        // --- CÁC BIẾN CHO CHỨC NĂNG TÌM KIẾM ---
        private Android.Widget.EditText? _edtSearch;
        private Android.Widget.ListView? _listSearchResults;
        private Android.Widget.ArrayAdapter<string>? _searchAdapter;
        private List<string> _searchResultsNames = new List<string>();
        // Từ điển lưu trữ các Marker trên bản đồ và Thể loại (category) của nó
        private Dictionary<Marker, string> _markerCategoryDict = new Dictionary<Marker, string>();

        // Thêm dòng này ở dưới cùng của khu vực khai báo biến
        private Dictionary<string, string> _audioUrlsDict = new Dictionary<string, string>();

        private Dictionary<string, string> _shopCategoryByNameDict = new Dictionary<string, string>();

        // Dictionary để lưu cặp <Tên quán, Tọa độ> giúp tìm kiếm nhanh
        private Dictionary<string, LatLng> _allShopsDict = new Dictionary<string, LatLng>();

        // --- CÁC BIẾN CHO GEOFENCING (TỰ PHÁT AUDIO) ---
        private Android.Locations.LocationManager? _locationManager;
        private Android.Media.MediaPlayer? _autoMediaPlayer;
        // Danh sách lưu tên các quán ĐÃ PHÁT, để chống việc đứng yên 1 chỗ mà app cứ nói đi nói lại (Giải quyết Slide 1)
        private List<string> _alreadyPlayedShops = new List<string>();

        private const int RequestLocationId = 1;

        // Xử lý khi người dùng gõ chữ vào thanh tìm kiếm
        private void EdtSearch_TextChanged(object? sender, Android.Text.TextChangedEventArgs e)
        {
            string keyword = e.Text?.ToString().ToLower() ?? "";
            _searchResultsNames.Clear();

            if (string.IsNullOrEmpty(keyword))
            {
                // Nếu không nhập gì, hiển thị tất cả quán (Auto gợi ý)
                ShowAllQ4Shops();
                return;
            }

            // Nếu có nhập chữ, lọc danh sách
            foreach (var shopName in _allShopsDict.Keys)
            {
                if (shopName.ToLower().Contains(keyword))
                {
                    _searchResultsNames.Add(shopName);
                }
            }

            if (_searchResultsNames.Count > 0)
            {
                _listSearchResults!.Visibility = ViewStates.Visible;
                _searchAdapter?.NotifyDataSetChanged();
            }
            else
            {
                _listSearchResults!.Visibility = ViewStates.Gone;
            }
        }

        // Xử lý khi người dùng bấm vào một quán trong danh sách xổ xuống
        private void ListSearchResults_ItemClick(object? sender, Android.Widget.AdapterView.ItemClickEventArgs e)
        {
            string selectedName = _searchResultsNames[e.Position];

            if (_allShopsDict.TryGetValue(selectedName, out LatLng targetLocation))
            {
                // 1. Bay camera tới quán đó và zoom to lên
                _map?.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(targetLocation, 18f));

                // 2. Điền tên quán vào thanh tìm kiếm và ẩn danh sách đi
                _edtSearch!.Text = selectedName;
                _listSearchResults!.Visibility = ViewStates.Gone;
                _edtSearch.ClearFocus();

                // 3. Ẩn bàn phím ảo của điện thoại cho đỡ vướng
                var imm = (Android.Views.InputMethods.InputMethodManager?)GetSystemService(InputMethodService);
                imm?.HideSoftInputFromWindow(_edtSearch.WindowToken, 0);
            }
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _bottomNavigation = FindViewById<BottomNavigationView>(Resource.Id.bottom_navigation);
            _categoryBar = FindViewById<HorizontalScrollView>(Resource.Id.category_bar);

            // --- ÁNH XẠ THANH TÌM KIẾM ---
            _searchLayout = FindViewById<Android.Widget.LinearLayout>(Resource.Id.search_layout);
            _edtSearch = FindViewById<Android.Widget.EditText>(Resource.Id.edtSearch);
            _listSearchResults = FindViewById<Android.Widget.ListView>(Resource.Id.listSearchResults);



            // Cài đặt Adapter cho danh sách kết quả
            _searchAdapter = new Android.Widget.ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, _searchResultsNames);
            if (_listSearchResults != null)
            {
                _listSearchResults.Adapter = _searchAdapter;
                _listSearchResults.ItemClick += ListSearchResults_ItemClick; // Sự kiện bấm vào kết quả
            }

            if (_edtSearch != null)
            {
                _edtSearch.TextChanged += EdtSearch_TextChanged;

                // MỚI: Khi chạm vào ô tìm kiếm, tự động xổ ra danh sách gợi ý
                _edtSearch.FocusChange += (s, e) =>
                {
                    if (e.HasFocus && string.IsNullOrEmpty(_edtSearch.Text))
                    {
                        ShowAllQ4Shops();
                    }
                };
            }
            // -----------------------------

            if (_bottomNavigation != null)
            {
                _bottomNavigation.ItemSelected += BottomNavigation_ItemSelected;
            }

            // BẮT SỰ KIỆN CLICK CHO CÁC NÚT DANH MỤC
            FindViewById<Android.Widget.Button>(Resource.Id.btnAll)?.Click += (s, e) => FilterMarkersByCategory("all");
            FindViewById<Android.Widget.Button>(Resource.Id.btnMuseum)?.Click += (s, e) => FilterMarkersByCategory("museum");
            FindViewById<Android.Widget.Button>(Resource.Id.btnRestaurant)?.Click += (s, e) => FilterMarkersByCategory("restaurant");
            FindViewById<Android.Widget.Button>(Resource.Id.btnCafe)?.Click += (s, e) => FilterMarkersByCategory("cafe");
            FindViewById<Android.Widget.Button>(Resource.Id.btnPark)?.Click += (s, e) => FilterMarkersByCategory("park");

            SetupFragments();
        }

        private void SetupFragments()
        {
            _mapFragment = SupportMapFragment.NewInstance();
            _profileFragment = new ProfileFragment();
            _toursFragment = new ToursFragment();

            SupportFragmentManager.BeginTransaction()
                .Add(Resource.Id.map_container, _toursFragment, "tours").Hide(_toursFragment)
                .Add(Resource.Id.map_container, _profileFragment, "profile").Hide(_profileFragment)
                .Add(Resource.Id.map_container, _mapFragment, "map")
                .Commit();

            _activeFragment = _mapFragment;
            _mapFragment.GetMapAsync(this);
        }

        private void BottomNavigation_ItemSelected(object? sender, NavigationBarView.ItemSelectedEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.navigation_map:
                    SwitchFragment(_mapFragment);
                    if (_categoryBar != null) _categoryBar.Visibility = ViewStates.Visible;
                    // Hiện thanh tìm kiếm ở Map
                    if (_searchLayout != null) _searchLayout.Visibility = ViewStates.Visible;
                    break;

                case Resource.Id.navigation_tours:
                    SwitchFragment(_toursFragment);
                    if (_categoryBar != null) _categoryBar.Visibility = ViewStates.Gone;
                    // Ẩn thanh tìm kiếm ở Tour
                    if (_searchLayout != null) _searchLayout.Visibility = ViewStates.Gone;
                    break;

                case Resource.Id.navigation_profile:
                    SwitchFragment(_profileFragment);
                    if (_categoryBar != null) _categoryBar.Visibility = ViewStates.Gone;
                    // Ẩn thanh tìm kiếm ở Profile
                    if (_searchLayout != null) _searchLayout.Visibility = ViewStates.Gone;
                    break;
            }
            e.Handled = true;
        }

        private void SwitchFragment(AndroidX.Fragment.App.Fragment? targetFragment)
        {
            if (targetFragment == null || targetFragment == _activeFragment) return;

            SupportFragmentManager.BeginTransaction()
                .Hide(_activeFragment!)
                .Show(targetFragment)
                .Commit();

            _activeFragment = targetFragment;
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            _map = googleMap;
            if (_map != null)
            {
                _map.UiSettings.ZoomControlsEnabled = true;

                _map.MarkerClick += Map_MarkerClick;
                _map.InfoWindowClick += Map_InfoWindowClick;
            }
            CheckAndRequestLocationPermission();

            // MỚI: Bản đồ vừa load xong là gọi đệ tử đi lấy dữ liệu quán ăn ngay!
            LoadShopsFromFirebase();
        }

        // Hàm 1: Khi người dùng bấm vào cái ghim đỏ
        private void Map_MarkerClick(object? sender, GoogleMap.MarkerClickEventArgs e)
        {
            var marker = e.Marker;

            // Hiển thị một cái bong bóng (InfoWindow) chứa tên quán phía trên cái ghim
            marker.ShowInfoWindow();

            // Đẩy camera di chuyển mượt mà về giữa màn hình
            _map?.AnimateCamera(CameraUpdateFactory.NewLatLng(marker.Position));

            e.Handled = true; // Báo cho hệ thống biết mình đã tự xử lý click
        }

        private void Map_InfoWindowClick(object? sender, GoogleMap.InfoWindowClickEventArgs e)
        {
            var marker = e.Marker;
            // SỬA THÀNH:
            string category = _shopCategoryByNameDict.ContainsKey(marker.Title) ? _shopCategoryByNameDict[marker.Title] : "restaurant";

            // Tạo một cái Menu popup hỏi người dùng muốn làm gì
            var builder = new Android.App.AlertDialog.Builder(this);
            builder.SetTitle(marker.Title);

            // Tùy theo thể loại quán mà hiện Nút bấm khác nhau
            List<string> options = new List<string> { "Chỉ đường tới đây" };

            if (category == "museum" || category == "park")
                options.Add("Nghe Thuyết Minh (Audio)");
            else
                options.Add("Đánh giá Quán này");

            builder.SetItems(options.ToArray(), async (dialog, args) =>
            {
                int choice = args.Which;
                if (choice == 0) // Chọn Chỉ đường
                {
                    Android.Locations.LocationManager? locationManager = (Android.Locations.LocationManager?)GetSystemService(LocationService);
                    Android.Locations.Location? myLoc = locationManager?.GetLastKnownLocation(Android.Locations.LocationManager.NetworkProvider)
                                                     ?? locationManager?.GetLastKnownLocation(Android.Locations.LocationManager.GpsProvider);
                    if (myLoc != null)
                    {
                        Toast.MakeText(this, "Đang vẽ đường...", ToastLength.Short)?.Show();
                        await DrawRouteAsync(new LatLng(myLoc.Latitude, myLoc.Longitude), marker.Position);
                    }
                    else Toast.MakeText(this, "Chưa lấy được GPS!", ToastLength.Short)?.Show();
                }
                // THAY THẾ KHỐI LỆNH CŨ BẰNG KHỐI NÀY:
                else if (choice == 1 && options[1].Contains("Audio"))
                {
                    Intent intent = new Intent(this, typeof(PoiAudioActivity));
                    intent.PutExtra("POI_NAME", marker.Title);

                    // Tìm xem quán này có link audio trên Firebase không, nếu không có thì dùng link trống
                    string actualAudioUrl = _audioUrlsDict.ContainsKey(marker.Title) ? _audioUrlsDict[marker.Title] : "";
                    intent.PutExtra("AUDIO_URL", actualAudioUrl);

                    StartActivity(intent);
                }
                else if (choice == 1 && options[1].Contains("Đánh giá")) // Chọn Đánh giá
                {
                    Intent intent = new Intent(this, typeof(ShopReviewActivity));
                    intent.PutExtra("SHOP_NAME", marker.Title);
                    StartActivity(intent);
                }
            });

            builder.Show();
        }

        // --- CÁC HÀM XỬ LÝ FIREBASE ---
        private void LoadShopsFromFirebase()
        {
            // Kết nối vào bảng "shops" và yêu cầu lấy toàn bộ dữ liệu
            FirebaseFirestore database = FirebaseFirestore.Instance;
            database.Collection("shops").Get().AddOnSuccessListener(this);
        }

        public void OnSuccess(Java.Lang.Object? result)
        {
            var snapshot = (QuerySnapshot)result;
            if (snapshot != null && !snapshot.IsEmpty)
            {
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    // Trích xuất thông tin
                    string name = document.GetString("name") ?? "Quán ăn";
                    double lat = document.GetDouble("latitude")?.DoubleValue() ?? 0;
                    double lng = document.GetDouble("longitude")?.DoubleValue() ?? 0;

                    // ĐỌC THÊM THỂ LOẠI TỪ FIREBASE (Nếu không có thì mặc định là restaurant)
                    string category = document.GetString("category")?.ToLower() ?? "restaurant";

                    // THÊM ĐOẠN MỚI NÀY VÀO NGAY BÊN DƯỚI:
                    string audioUrl = document.GetString("audio_url") ?? "";
                    if (!string.IsNullOrEmpty(audioUrl))
                    {
                        _audioUrlsDict[name] = audioUrl; // Lưu link lại với chìa khóa là Tên quán
                    }

                    // (Tùy chọn) Kiểm tra điều kiện Quận 4 ở đây nếu bạn muốn

                    LatLng location = new LatLng(lat, lng);
                    MarkerOptions markerOptions = new MarkerOptions();
                    markerOptions.SetPosition(location);
                    markerOptions.SetTitle(name);

                    Marker? m = _map?.AddMarker(markerOptions);

                    if (m != null)
                    {
                        // Lưu Marker và Thể loại của nó vào danh sách (Dùng cho hàm Lọc ẩn/hiện)
                        _markerCategoryDict[m] = category;
                    }

                    // THÊM DÒNG NÀY VÀO ĐỂ TÌM KIẾM KHI CLICK:
                    _shopCategoryByNameDict[name] = category;

                    _allShopsDict[name] = location;
                }
            }
        }


        // -------------------------------


        private void FilterMarkersByCategory(string selectedCategory)
        {
            // Duyệt qua toàn bộ các Marker đang có
            foreach (var item in _markerCategoryDict)
            {
                Marker marker = item.Key;
                string cat = item.Value;

                // Nếu chọn "all" hoặc thể loại của marker khớp với nút bấm -> HIỆN
                if (selectedCategory == "all" || cat.Contains(selectedCategory))
                {
                    marker.Visible = true;
                }
                else
                {
                    // Nếu không khớp -> ẨN
                    marker.Visible = false;
                }
            }
        }
        private void StartTrackingGPS()
        {
            _locationManager = (Android.Locations.LocationManager?)GetSystemService(LocationService);
            if (_locationManager != null && ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessFineLocation) == (int)Permission.Granted)
            {
                // Bật Radar: Cập nhật tọa độ mỗi 5 giây (5000ms) hoặc khi người dùng bước đi 2 mét (2f)
                _locationManager.RequestLocationUpdates(Android.Locations.LocationManager.GpsProvider, 5000, 2f, this);
            }
        }

        // --- CẬP NHẬT 2 HÀM CŨ ĐỂ GỌI RADAR ---
        private void CheckAndRequestLocationPermission()
        {
            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessFineLocation) == (int)Permission.Granted)
            {
                if (_map != null) { _map.MyLocationEnabled = true; _map.UiSettings.MyLocationButtonEnabled = true; }
                StartTrackingGPS(); // <--- THÊM DÒNG NÀY
                MoveMyLocationButton();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new string[] { Android.Manifest.Permission.AccessFineLocation }, RequestLocationId);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            if (requestCode == RequestLocationId && grantResults.Length > 0 && grantResults[0] == Android.Content.PM.Permission.Granted)
            {
                if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessFineLocation) == (int)Android.Content.PM.Permission.Granted)
                {
                    if (_map != null) { _map.MyLocationEnabled = true; _map.UiSettings.MyLocationButtonEnabled = true; }
                    StartTrackingGPS(); // <--- THÊM DÒNG NÀY
                    MoveMyLocationButton();
                }
            }
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void ZoomToUserLocation()
        {
            Android.Locations.LocationManager? locationManager = (Android.Locations.LocationManager?)GetSystemService(LocationService);
            Android.Locations.Location? myLocation = locationManager?.GetLastKnownLocation(Android.Locations.LocationManager.NetworkProvider);

            if (myLocation == null)
            {
                myLocation = locationManager?.GetLastKnownLocation(Android.Locations.LocationManager.GpsProvider);
            }

            if (myLocation != null && _map != null)
            {
                // Lấy tọa độ của bạn và bay tới đó, zoom gần lại (mức 16f)
                LatLng myLatLng = new LatLng(myLocation.Latitude, myLocation.Longitude);
                _map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myLatLng, 16f));
            }
        }
        private void ShowAllQ4Shops()
        {
            _searchResultsNames.Clear();
            foreach (var shopName in _allShopsDict.Keys)
            {
                _searchResultsNames.Add(shopName);
            }

            if (_searchResultsNames.Count > 0 && _listSearchResults != null)
            {
                _listSearchResults.Visibility = ViewStates.Visible;
                _searchAdapter?.NotifyDataSetChanged();
            }
        }

        // Biến lưu trữ đường vẽ hiện tại (để xóa đường cũ khi chọn quán khác)
        private Polyline? _currentPolyline;

        // Thuật toán kinh điển của Google để giải mã chuỗi thành tọa độ (Copy & Paste)
        private List<LatLng> DecodePolyline(string encodedPoints)
        {
            if (string.IsNullOrWhiteSpace(encodedPoints)) return new List<LatLng>();
            int index = 0, len = encodedPoints.Length;
            int lat = 0, lng = 0;
            var poly = new List<LatLng>();
            while (index < len)
            {
                int b, shift = 0, result = 0;
                do
                {
                    b = encodedPoints[index++] - 63;
                    result |= (b & 0x1f) << shift;
                    shift += 5;
                } while (b >= 0x20);
                int dlat = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                lat += dlat;
                shift = 0; result = 0;
                do
                {
                    b = encodedPoints[index++] - 63;
                    result |= (b & 0x1f) << shift;
                    shift += 5;
                } while (b >= 0x20);
                int dlng = ((result & 1) != 0 ? ~(result >> 1) : (result >> 1));
                lng += dlng;
                poly.Add(new LatLng(lat / 1E5, lng / 1E5));
            }
            return poly;
        }


        private async System.Threading.Tasks.Task DrawRouteAsync(LatLng origin, LatLng destination)
        {
            // NHỚ SỬA LẠI API KEY CỦA BẠN VÀO ĐÂY NHÉ
            string apiKey = "AIzaSyBY1bmxcN8icZokOhBkLMo8TKj-WuHf25o";

            string url = $"https://maps.googleapis.com/maps/api/directions/json?origin={origin.Latitude},{origin.Longitude}&destination={destination.Latitude},{destination.Longitude}&key={apiKey}";

            try
            {
                using HttpClient client = new HttpClient();
                string response = await client.GetStringAsync(url);
                JSONObject json = new JSONObject(response);

                // Đọc mã trạng thái mà Google trả về
                string status = json.GetString("status");

                if (status == "OK")
                {
                    JSONArray routes = json.GetJSONArray("routes");
                    JSONObject route = routes.GetJSONObject(0);
                    JSONObject overviewPolyline = route.GetJSONObject("overview_polyline");
                    string points = overviewPolyline.GetString("points");

                    var decodedPath = DecodePolyline(points);

                    RunOnUiThread(() =>
                    {
                        if (_currentPolyline != null)
                        {
                            _currentPolyline.Remove();
                        }

                        PolylineOptions lineOptions = new PolylineOptions();
                        foreach (var point in decodedPath)
                        {
                            lineOptions.Add(point);
                        }
                        lineOptions.InvokeWidth(15f);
                        lineOptions.InvokeColor(Android.Graphics.Color.ParseColor("#2196F3"));
                        lineOptions.Geodesic(true);

                        _currentPolyline = _map?.AddPolyline(lineOptions);
                    });
                }
                else
                {
                    // NẾU LỖI: In chi tiết lý do Google từ chối ra màn hình
                    string errorMessage = json.Has("error_message") ? json.GetString("error_message") : status;
                    RunOnUiThread(() => Android.Widget.Toast.MakeText(this, $"Lỗi từ Google: {status} - {errorMessage}", Android.Widget.ToastLength.Long)?.Show());
                }
            }
            catch (System.Exception ex)
            {
                RunOnUiThread(() => Android.Widget.Toast.MakeText(this, $"Lỗi App: {ex.Message}", Android.Widget.ToastLength.Long)?.Show());
            }
        }
        // --- HÀM MỚI: BẮT TÍN HIỆU TỪ TRANG CHI TIẾT TOUR TRUYỀN VỀ ---
        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            string? shopToZoom = intent?.GetStringExtra("ZOOM_TO_SHOP");

            if (!string.IsNullOrEmpty(shopToZoom))
            {
                // 1. Tự động chuyển Menu dưới đáy về Tab Bản đồ
                if (_bottomNavigation != null)
                {
                    _bottomNavigation.SelectedItemId = Resource.Id.navigation_map;
                }

                // 2. Tìm tọa độ và bay camera tới quán đó
                if (_allShopsDict.TryGetValue(shopToZoom, out LatLng targetLocation))
                {
                    // Bay tới và phóng to bản đồ
                    _map?.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(targetLocation, 18f));

                    // Mở luôn cái bong bóng của quán đó lên cho đẹp
                    foreach (var marker in _markerCategoryDict.Keys)
                    {
                        if (marker.Title == shopToZoom)
                        {
                            marker.ShowInfoWindow();
                            break;
                        }
                    }
                }
            }
        }
        // ====================================================================
        // PHẦN Geofencing - TỰ ĐỘNG PHÁT AUDIO THEO TỌA ĐỘ BƯỚC CHÂN
        // ====================================================================

        // Hàm này tự động chạy mỗi khi bạn bước đi (tọa độ GPS thay đổi)
        public void OnLocationChanged(Android.Locations.Location location)
        {
            // Duyệt qua tất cả các quán ăn/bảo tàng đang có trên bản đồ
            foreach (var shop in _allShopsDict)
            {
                string shopName = shop.Key;
                LatLng shopLatLng = shop.Value;

                // 1. Kiểm tra xem quán này có link Audio không? Nếu không thì bỏ qua
                if (!_audioUrlsDict.ContainsKey(shopName) || string.IsNullOrEmpty(_audioUrlsDict[shopName]))
                    continue;

                // 2. Tính khoảng cách từ chỗ bạn đang đứng đến cái quán đó
                float[] results = new float[1];
                Android.Locations.Location.DistanceBetween(location.Latitude, location.Longitude, shopLatLng.Latitude, shopLatLng.Longitude, results);
                float distanceInMeters = results[0];

                // 3. THUẬT TOÁN ĐIỀU KIỆN: Nếu cách DƯỚI 20 MÉT và CHƯA TỪNG PHÁT BÀI NÀY
                if (distanceInMeters < 20f && !_alreadyPlayedShops.Contains(shopName))
                {
                    // Ghi vào sổ đen để không phát lặp lại (Giải quyết yêu cầu Slide 1)
                    _alreadyPlayedShops.Add(shopName);

                    // Bật nhạc!
                    PlayAutoAudio(_audioUrlsDict[shopName], shopName);

                    // Tự động mở bong bóng của quán đó lên màn hình cho sinh động
                    foreach (var marker in _markerCategoryDict.Keys)
                    {
                        if (marker.Title == shopName)
                        {
                            marker.ShowInfoWindow();
                            _map?.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(marker.Position, 18f));
                            break;
                        }
                    }
                }
            }
        }



        private Android.Media.AudioManager? _audioManager;
        private MyAudioFocusListener? _audioFocusListener;

        private void PlayAutoAudio(string url, string shopName)
        {
            try
            {
                // 1. Dọn dẹp trình phát cũ nếu có
                if (_autoMediaPlayer != null)
                {
                    if (_autoMediaPlayer.IsPlaying) _autoMediaPlayer.Stop();
                    _autoMediaPlayer.Release();
                }

                _audioManager = (Android.Media.AudioManager?)GetSystemService(AudioService);
                _autoMediaPlayer = new Android.Media.MediaPlayer();
                _audioFocusListener = new MyAudioFocusListener(_autoMediaPlayer); // Giao phó cho Trọng tài

                _autoMediaPlayer.SetDataSource(url);
                _autoMediaPlayer.PrepareAsync();

                _autoMediaPlayer.Prepared += (s, e) =>
                {
                    // 2. TRƯỚC KHI PHÁT: Phải xin quyền "Cầm Micro" từ hệ thống Android
                    var focusResult = _audioManager?.RequestAudioFocus(_audioFocusListener, Android.Media.Stream.Music, Android.Media.AudioFocus.Gain);

                    if (focusResult == Android.Media.AudioFocusRequest.Granted)
                    {
                        _autoMediaPlayer.Start();
                        Toast.MakeText(this, $"📍 Bạn đã đến {shopName}. Bắt đầu thuyết minh!", ToastLength.Long)?.Show();
                    }
                };
            }
            catch { }
        }

        // --- THỦ THUẬT DỜI NÚT TỌA ĐỘ CỦA GOOGLE MAPS ---
        private void MoveMyLocationButton()
        {
            try
            {
                // Tìm cái View chứa nút Tọa độ (Google ngầm định ID của nó là số 2)
                if (_mapFragment?.View != null)
                {
                    Android.Views.View? locationButton = ((Android.Views.View)_mapFragment.View.FindViewById(int.Parse("1"))?.Parent)?.FindViewById(int.Parse("2"));

                    if (locationButton != null && locationButton.LayoutParameters is Android.Widget.RelativeLayout.LayoutParams layoutParams)
                    {
                        // 1. Gỡ bỏ lệnh ghim lên trên (Top)
                        layoutParams.AddRule(Android.Widget.LayoutRules.AlignParentTop, 0);

                        // 2. Ép lệnh ghim xuống dưới (Bottom)
                        layoutParams.AddRule(Android.Widget.LayoutRules.AlignParentBottom, (int)Android.Widget.LayoutRules.True);

                        // 3. Chỉnh khoảng cách so với lề (Cách phải 30px, Cách đáy 250px để chừa chỗ cho 2 cái nút Zoom)
                        layoutParams.SetMargins(0, 0, 30, 250);

                        locationButton.LayoutParameters = layoutParams;
                    }
                }
            }
            catch { /* Bỏ qua nếu có lỗi ngầm định */ }
        }

        // 3 Hàm bắt buộc của giao diện ILocationListener (Chỉ cần để trống)
        public void OnProviderDisabled(string provider) { }
        public void OnProviderEnabled(string provider) { }
        public void OnStatusChanged(string provider, Android.Locations.Availability status, Bundle extras) { }
    }
}