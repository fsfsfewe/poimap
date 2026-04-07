using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Firebase.Firestore;
using System.Collections.Generic;

namespace poimap
{
    [Activity(Label = "Chi tiết Tour")]
    public class TourDetailActivity : Activity, Android.Gms.Tasks.IOnSuccessListener
    {
        private ListView? _listShopsInTour;
        private List<TourShopItem> _shopDetailsList = new List<TourShopItem>();
        private TourShopAdapter? _adapter;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_tour_detail);

            string tourName = Intent?.GetStringExtra("TOUR_NAME") ?? "Chi tiết Tour";
            FindViewById<TextView>(Resource.Id.txtDetailTourName)!.Text = tourName;
            FindViewById<ImageButton>(Resource.Id.btnBackToTours)!.Click += (s, e) => Finish();

            _listShopsInTour = FindViewById<ListView>(Resource.Id.listShopsInTour);
            _adapter = new TourShopAdapter(this, _shopDetailsList);

            if (_listShopsInTour != null)
            {
                _listShopsInTour.Adapter = _adapter;

                // BẮT SỰ KIỆN CLICK VÀO QUÁN ĐỂ BAY VỀ MÀN HÌNH BẢN ĐỒ
                _listShopsInTour.ItemClick += (s, e) =>
                {
                    string shopName = _shopDetailsList[e.Position].Name;

                    // Gọi MainActivity dậy và truyền tên quán về
                    Intent intent = new Intent(this, typeof(MainActivity));
                    intent.PutExtra("ZOOM_TO_SHOP", shopName);

                    // Lệnh này giúp đóng trang hiện tại và quay về cái Map đang mở sẵn
                    intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
                    StartActivity(intent);
                };
            }

            LoadShopsForTour(tourName);
        }

        private void LoadShopsForTour(string tourName)
        {
            FirebaseFirestore.Instance.Collection("shops")
                .WhereEqualTo("tour_name", tourName)
                .Get()
                .AddOnSuccessListener(this);
        }

        public void OnSuccess(Java.Lang.Object? result)
        {
            var snapshot = (QuerySnapshot)result;
            if (snapshot != null && !snapshot.IsEmpty)
            {
                _shopDetailsList.Clear();
                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    string name = doc.GetString("name") ?? "Quán chưa tên";
                    string category = doc.GetString("category") ?? "Điểm đến";
                    _shopDetailsList.Add(new TourShopItem { Name = name, Category = category.ToUpper() });
                }
                _adapter?.NotifyDataSetChanged();
            }
            else
            {
                Toast.MakeText(this, "Không có dữ liệu. Vui lòng kiểm tra lại tên Tour trên Firebase!", ToastLength.Long)?.Show();
            }
        }
    }

    // --- CẤU TRÚC DỮ LIỆU CHUẨN ---
    public class TourShopItem
    {
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
    }

    // --- ADAPTER TÙY CHỈNH ĐỂ CHỐNG LỖI CHỮ TRẮNG ---
    public class TourShopAdapter : BaseAdapter<TourShopItem>
    {
        private Activity _context;
        private List<TourShopItem> _items;

        public TourShopAdapter(Activity context, List<TourShopItem> items)
        {
            _context = context;
            _items = items;
        }

        public override TourShopItem this[int position] => _items[position];
        public override int Count => _items.Count;
        public override long GetItemId(int position) => position;

        public override View GetView(int position, View? convertView, ViewGroup? parent)
        {
            // Dùng lại layout mặc định của Android nhưng can thiệp vào màu sắc
            View view = convertView ?? _context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null)!;
            TextView txt = view.FindViewById<TextView>(Android.Resource.Id.Text1)!;

            var item = _items[position];
            txt.Text = $"{item.Name} ({item.Category})";

            // ÉP CHỮ MÀU ĐEN VÀ CHỈNH FONT TO LÊN
            txt.SetTextColor(Android.Graphics.Color.Black);
            txt.SetTextSize(Android.Util.ComplexUnitType.Sp, 16f);

            return view;
        }
    }
}