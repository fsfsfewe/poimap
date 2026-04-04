using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Navigation;
using Android.Gms.Maps;
using Android.Gms.Maps.Model; // Thư viện để làm Ghim (Marker) và Camera bản đồ
using Firebase.Firestore; // Thư viện đọc dữ liệu Firebase
using Android.Gms.Tasks; // Thư viện để lắng nghe dữ liệu trả về
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Org.Json; // Thư viện đọc JSON có sẵn của Android

namespace poimap
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    // CHÚ Ý: Đã thêm IOnSuccessListener để app biết cách "nghe" dữ liệu từ mạng
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback, IOnSuccessListener
    {
        private GoogleMap? _map;
        private BottomNavigationView? _bottomNavigation;
        private HorizontalScrollView? _categoryBar;

        private SupportMapFragment? _mapFragment;
        private ProfileFragment? _profileFragment;
        private ToursFragment? _toursFragment;
        private AndroidX.Fragment.App.Fragment? _activeFragment;

        private const int RequestLocationId = 1;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _bottomNavigation = FindViewById<BottomNavigationView>(Resource.Id.bottom_navigation);
            _categoryBar = FindViewById<HorizontalScrollView>(Resource.Id.category_bar);

            if (_bottomNavigation != null)
            {
                _bottomNavigation.ItemSelected += BottomNavigation_ItemSelected;
            }

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
                    break;

                case Resource.Id.navigation_tours:
                    SwitchFragment(_toursFragment);
                    if (_categoryBar != null) _categoryBar.Visibility = ViewStates.Gone;
                    break;

                case Resource.Id.navigation_profile:
                    SwitchFragment(_profileFragment);
                    if (_categoryBar != null) _categoryBar.Visibility = ViewStates.Gone;
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

        // Hàm 2: Khi người dùng bấm vào cái bong bóng tên quán (Để bắt đầu chỉ đường)
        private async void Map_InfoWindowClick(object? sender, GoogleMap.InfoWindowClickEventArgs e)
        {
            var marker = e.Marker;
            var destination = marker.Position;

            Android.Widget.Toast.MakeText(this, $"Đang tìm đường đến: {marker.Title}", Android.Widget.ToastLength.Short)?.Show();

            // 1. Lấy dịch vụ định vị của điện thoại
            Android.Locations.LocationManager? locationManager = (Android.Locations.LocationManager?)GetSystemService(LocationService);

            // 2. Thử lấy vị trí hiện tại thông qua Mạng/Wifi (nhanh hơn)
            Android.Locations.Location? myLocation = locationManager?.GetLastKnownLocation(Android.Locations.LocationManager.NetworkProvider);

            // 3. Nếu không có Wifi, thử lấy bằng GPS
            if (myLocation == null)
            {
                myLocation = locationManager?.GetLastKnownLocation(Android.Locations.LocationManager.GpsProvider);
            }

            // 4. Nếu lấy được vị trí thành công thì bắt đầu vẽ đường
            if (myLocation != null)
            {
                LatLng realLocation = new LatLng(myLocation.Latitude, myLocation.Longitude);
                await DrawRouteAsync(realLocation, destination);
            }
            else
            {
                // Trường hợp máy ảo chưa kịp cập nhật vị trí hoặc user tắt GPS
                Android.Widget.Toast.MakeText(this, "Chưa xác định được vị trí của bạn, hãy bấm nút La Bàn trên map trước!", Android.Widget.ToastLength.Long)?.Show();
            }
        }

        // --- CÁC HÀM XỬ LÝ FIREBASE ---
        private void LoadShopsFromFirebase()
        {
            // Kết nối vào bảng "shops" và yêu cầu lấy toàn bộ dữ liệu
            FirebaseFirestore database = FirebaseFirestore.Instance;
            database.Collection("shops").Get().AddOnSuccessListener(this);
        }

        // Hàm này sẽ tự động chạy khi Firebase trả dữ liệu về thành công
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

                    // Tạo điểm ghim (Marker)
                    LatLng location = new LatLng(lat, lng);
                    MarkerOptions marker = new MarkerOptions();
                    marker.SetPosition(location);
                    marker.SetTitle(name);

                    // Đóng đinh lên bản đồ
                    _map?.AddMarker(marker);
                }

                // Cuối cùng: Cho máy bay (Camera) bay thẳng tới Phố Vĩnh Khánh - Quận 4
                LatLng vinhKhanhCenter = new LatLng(10.7588, 106.7011);
                _map?.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(vinhKhanhCenter, 16f));
            }
        }
        // -------------------------------

        private void CheckAndRequestLocationPermission()
        {
            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessFineLocation) == (int)Permission.Granted)
            {
                if (_map != null) { _map.MyLocationEnabled = true; _map.UiSettings.MyLocationButtonEnabled = true; }
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new string[] { Android.Manifest.Permission.AccessFineLocation }, RequestLocationId);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == RequestLocationId && grantResults.Length > 0 && grantResults[0] == Permission.Granted)
            {
                if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessFineLocation) == (int)Permission.Granted)
                {
                    if (_map != null) { _map.MyLocationEnabled = true; _map.UiSettings.MyLocationButtonEnabled = true; }
                }
            }
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
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
    }
}