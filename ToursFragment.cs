using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Firebase.Firestore;
using System.Collections.Generic;

namespace poimap
{
    public class ToursFragment : AndroidX.Fragment.App.Fragment, Android.Gms.Tasks.IOnSuccessListener
    {
        private ListView? _listTours;
        private List<TourItem> _tourList = new List<TourItem>();
        private TourAdapter? _adapter;
        private AndroidX.SwipeRefreshLayout.Widget.SwipeRefreshLayout? _swipeRefreshLayout;

        // CÁC BIẾN CHO PHẦN LỘ TRÌNH NỔI BẬT
        private RelativeLayout? _layoutFeatured1;
        private RelativeLayout? _layoutFeatured2;
        private TextView? _txtFeaturedTour1;
        private TextView? _txtFeaturedTour2;

        private ImageView? _imgFeatured1;
        private ImageView? _imgFeatured2;

        public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.layout_tours, container, false);
            _listTours = view.FindViewById<ListView>(Resource.Id.listTours);

            // Cài đặt kéo xuống làm mới
            _swipeRefreshLayout = view.FindViewById<AndroidX.SwipeRefreshLayout.Widget.SwipeRefreshLayout>(Resource.Id.swipeRefreshLayoutTours);
            if (_swipeRefreshLayout != null)
            {
                _swipeRefreshLayout.SetColorSchemeColors(Android.Graphics.Color.ParseColor("#008080"));
                _swipeRefreshLayout.Refresh += (sender, e) => LoadToursFromFirebase();
            }

            // Gắn Header và Ánh xạ các nút nổi bật
            View headerView = inflater.Inflate(Resource.Layout.layout_tours_header, null, false);
            _listTours?.AddHeaderView(headerView);

            headerView.FindViewById<TextView>(Resource.Id.txtHeaderTitle1)!.Text = LanguageHelper.GetText("Lộ trình tham quan");
            headerView.FindViewById<TextView>(Resource.Id.txtHeaderDesc)!.Text = LanguageHelper.GetText("Khám phá Vĩnh Khánh theo lộ trình của bạn");
            headerView.FindViewById<TextView>(Resource.Id.txtHeaderTitle2)!.Text = LanguageHelper.GetText("Lộ trình nổi bật");
            headerView.FindViewById<TextView>(Resource.Id.txtHeaderTitle3)!.Text = LanguageHelper.GetText("Tất cả lộ trình");

            _layoutFeatured1 = headerView.FindViewById<RelativeLayout>(Resource.Id.layoutFeatured1);
            _layoutFeatured2 = headerView.FindViewById<RelativeLayout>(Resource.Id.layoutFeatured2);
            _txtFeaturedTour1 = headerView.FindViewById<TextView>(Resource.Id.txtFeaturedTour1);
            _txtFeaturedTour2 = headerView.FindViewById<TextView>(Resource.Id.txtFeaturedTour2);

            _imgFeatured1 = headerView.FindViewById<ImageView>(Resource.Id.imgFeatured1);
            _imgFeatured2 = headerView.FindViewById<ImageView>(Resource.Id.imgFeatured2);

            // BẮT SỰ KIỆN KHI BẤM VÀO LỘ TRÌNH NỔI BẬT
            if (_layoutFeatured1 != null) _layoutFeatured1.Click += (s, e) => OpenTourDetail(_txtFeaturedTour1?.Text);
            if (_layoutFeatured2 != null) _layoutFeatured2.Click += (s, e) => OpenTourDetail(_txtFeaturedTour2?.Text);

            // Cài đặt danh sách Tất cả lộ trình
            _adapter = new TourAdapter(this.Activity!, _tourList);
            if (_listTours != null)
            {
                _listTours.Adapter = _adapter;
                _listTours.ItemClick += (s, e) =>
                {
                    int actualPosition = e.Position - _listTours.HeaderViewsCount;
                    if (actualPosition >= 0)
                    {
                        OpenTourDetail(_tourList[actualPosition].Name);
                    }
                };
            }

            LoadToursFromFirebase();

            return view;
        }

        // HÀM CHUYỂN TRANG DÙNG CHUNG CHO CẢ DANH SÁCH VÀ THẺ NỔI BẬT
        private void OpenTourDetail(string? tourName)
        {
            if (string.IsNullOrEmpty(tourName) || tourName == "Đang tải...") return;
            Intent intent = new Intent(this.Activity, typeof(TourDetailActivity));
            intent.PutExtra("TOUR_NAME", tourName);
            StartActivity(intent);
        }
        private async void LoadImageAsyncForHeader(string url, ImageView imageView)
        {
            try
            {
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    byte[] imageBytes = await client.GetByteArrayAsync(url);
                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        Android.Graphics.Bitmap? bitmap = Android.Graphics.BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                        Activity?.RunOnUiThread(() =>
                        {
                            if (bitmap != null) imageView.SetImageBitmap(bitmap);
                        });
                    }
                }
            }
            catch { }
        }
        private void LoadToursFromFirebase()
        {
            FirebaseFirestore.Instance.Collection("tours").Get().AddOnSuccessListener(this);
        }

        public void OnSuccess(Java.Lang.Object? result)
        {
            if (_swipeRefreshLayout != null && _swipeRefreshLayout.Refreshing)
            {
                _swipeRefreshLayout.Refreshing = false;
            }

            var snapshot = (QuerySnapshot)result;
            if (snapshot != null && !snapshot.IsEmpty)
            {
                _tourList.Clear();
                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    string name = doc.GetString("name") ?? "Tour chưa đặt tên";
                    string desc = doc.GetString("description") ?? "Đang cập nhật mô tả...";

                    // KHAI BÁO BIẾN imgUrl TỪ FIREBASE
                    string imgUrl = doc.GetString("image_url") ?? "";

                    // CHỈ GIỮ LẠI 1 CÂU LỆNH ADD NÀY THÔI
                    _tourList.Add(new TourItem { Name = name, Description = desc, ImageUrl = imgUrl });
                }

                // TỰ ĐỘNG LẤY 2 TOUR ĐẦU TIÊN GÁN VÀO THẺ NỔI BẬT
                if (_tourList.Count > 0)
                {
                    if (_txtFeaturedTour1 != null) _txtFeaturedTour1.Text = _tourList[0].Name;
                    if (_imgFeatured1 != null && !string.IsNullOrEmpty(_tourList[0].ImageUrl))
                        LoadImageAsyncForHeader(_tourList[0].ImageUrl, _imgFeatured1);
                }

                if (_tourList.Count > 1)
                {
                    if (_txtFeaturedTour2 != null) _txtFeaturedTour2.Text = _tourList[1].Name;
                    if (_imgFeatured2 != null && !string.IsNullOrEmpty(_tourList[1].ImageUrl))
                        LoadImageAsyncForHeader(_tourList[1].ImageUrl, _imgFeatured2);
                }

                _adapter?.NotifyDataSetChanged();
            }
        }
    }

    public class TourItem
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
    }

    // --- BỘ CHUYỂN ĐỔI (ADAPTER) CHO ITEM TOUR ---
    public class TourAdapter : BaseAdapter<TourItem>
    {
        private Android.App.Activity _context;
        private List<TourItem> _items;

        public TourAdapter(Android.App.Activity context, List<TourItem> items)
        {
            _context = context;
            _items = items;
        }

        public override TourItem this[int position] => _items[position];
        public override int Count => _items.Count;
        public override long GetItemId(int position) => position;

        public override View GetView(int position, View? convertView, ViewGroup? parent)
        {
            View view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.item_tour, null)!;
            var item = _items[position];

            view.FindViewById<TextView>(Resource.Id.txtTourName)!.Text = item.Name;
            view.FindViewById<TextView>(Resource.Id.txtTourDesc)!.Text = item.Description;

            var imgTour = view.FindViewById<ImageView>(Resource.Id.imgTourItem);

            if (!string.IsNullOrEmpty(item.ImageUrl) && imgTour != null)
            {
                // Đặt một bức ảnh xám mặc định trong lúc chờ tải
                imgTour.SetImageResource(Android.Resource.Drawable.IcMenuGallery);

                // Gọi hàm C# thuần để tải ảnh
                LoadImageAsync(item.ImageUrl, imgTour);
            }

            return view;
        }

        // --- HÀM TỰ VIẾT ĐỂ TẢI ẢNH TỪ MẠNG (KHÔNG CẦN DÙNG THƯ VIỆN) ---
        private async void LoadImageAsync(string url, ImageView imageView)
        {
            try
            {
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    // Tải dữ liệu ảnh về dưới dạng byte
                    byte[] imageBytes = await client.GetByteArrayAsync(url);

                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        // Dịch byte thành Hình ảnh (Bitmap)
                        Android.Graphics.Bitmap? bitmap = Android.Graphics.BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);

                        // Đẩy hình ảnh ra màn hình chính
                        _context.RunOnUiThread(() =>
                        {
                            if (bitmap != null) imageView.SetImageBitmap(bitmap);
                        });
                    }
                }
            }
            catch
            {
                // Bỏ qua nếu link ảnh bị hỏng hoặc lỗi mạng
            }
        }
    }
}