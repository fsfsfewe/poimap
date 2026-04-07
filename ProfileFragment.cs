using Android.OS;
using Android.Views;
using Android.Widget;
using Firebase.Firestore;
using System.Collections.Generic;

namespace poimap
{
    public class ProfileFragment : AndroidX.Fragment.App.Fragment, Android.Gms.Tasks.IOnSuccessListener
    {
        private ListView? _listMyReviews;
        private List<ReviewItem> _reviewList = new List<ReviewItem>();
        private ReviewAdapter? _adapter;

        private AndroidX.SwipeRefreshLayout.Widget.SwipeRefreshLayout? _swipeRefreshLayout;

        public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.layout_profile, container, false);
            _listMyReviews = view.FindViewById<ListView>(Resource.Id.listMyReviews);

            // --- THÊM KHỐI LỆNH NÀY ---
            _swipeRefreshLayout = view.FindViewById<AndroidX.SwipeRefreshLayout.Widget.SwipeRefreshLayout>(Resource.Id.swipeRefreshLayoutProfile);
            if (_swipeRefreshLayout != null)
            {
                // Đổi màu vòng tròn xoay cho hợp tone màu của app (Màu tím)
                _swipeRefreshLayout.SetColorSchemeColors(Android.Graphics.Color.ParseColor("#6A0DAD"), Android.Graphics.Color.ParseColor("#8A2BE2"));

                // Bắt sự kiện khi người dùng vuốt màn hình xuống
                _swipeRefreshLayout.Refresh += (sender, e) =>
                {
                    // Vuốt xuống -> Gọi hàm tải lại dữ liệu từ đầu
                    LoadReviewsFromFirebase();
                };
            }
            // --------------------------

            // Khởi tạo Adapter
            _adapter = new ReviewAdapter(this.Activity!, _reviewList);
            if (_listMyReviews != null)
            {
                _listMyReviews.Adapter = _adapter;
            }

            LoadReviewsFromFirebase();

            return view;
        }

        private void LoadReviewsFromFirebase()
        {
            // Tải dữ liệu từ bảng "reviews" trên Firebase
            FirebaseFirestore.Instance.Collection("reviews")
                .Get()
                .AddOnSuccessListener(this);
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
                _reviewList.Clear();
                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    string shopName = doc.GetString("shopName") ?? "Quán ẩn danh";
                    float stars = doc.GetDouble("stars")?.FloatValue() ?? 0f;
                    string comment = doc.GetString("comment") ?? "";

                    _reviewList.Add(new ReviewItem { ShopName = shopName, Stars = stars, Comment = comment });
                }

                // Báo cho danh sách cập nhật giao diện
                _adapter?.NotifyDataSetChanged();
            }
        }
    }

    // --- CẤU TRÚC DỮ LIỆU ĐÁNH GIÁ ---
    public class ReviewItem
    {
        public string ShopName { get; set; } = "";
        public float Stars { get; set; } = 0f;
        public string Comment { get; set; } = "";
    }

    // --- BỘ CHUYỂN ĐỔI (ADAPTER) TỪ DỮ LIỆU SANG GIAO DIỆN ---
    public class ReviewAdapter : BaseAdapter<ReviewItem>
    {
        private Android.App.Activity _context;
        private List<ReviewItem> _items;

        public ReviewAdapter(Android.App.Activity context, List<ReviewItem> items)
        {
            _context = context;
            _items = items;
        }

        public override ReviewItem this[int position] => _items[position];
        public override int Count => _items.Count;
        public override long GetItemId(int position) => position;

        public override View GetView(int position, View? convertView, ViewGroup? parent)
        {
            View view = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.item_review, null)!;

            var item = _items[position];

            view.FindViewById<TextView>(Resource.Id.txtRevShopName)!.Text = item.ShopName;
            view.FindViewById<RatingBar>(Resource.Id.ratingBarRev)!.Rating = item.Stars;
            view.FindViewById<TextView>(Resource.Id.txtRevComment)!.Text = item.Comment;

            return view;
        }
    }
}