using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Firebase.Firestore;
using System.Collections.Generic;

namespace poimap
{
    [Activity(Label = "Đánh Giá Quán")]
    public class ShopReviewActivity : Activity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout_shop_review);

            string shopName = Intent?.GetStringExtra("SHOP_NAME") ?? "Quán ăn";
            // Do trong file xml bạn đang fix cứng chữ Bánh Canh, ta tạm tìm id theo code gốc hoặc bạn có thể đổi id TextView sau.
            // FindViewById<TextView>(Resource.Id.txtShopName).Text = shopName; 

            var ratingBar = FindViewById<RatingBar>(Resource.Id.ratingBar);
            var edtComment = FindViewById<EditText>(Resource.Id.edtComment);
            var btnSubmit = FindViewById<Button>(Resource.Id.btnSubmitReview);

            btnSubmit!.Click += (s, e) =>
            {
                float stars = ratingBar?.Rating ?? 0;
                string comment = edtComment?.Text ?? "";

                if (stars == 0)
                {
                    Toast.MakeText(this, "Vui lòng chọn số sao!", ToastLength.Short)?.Show();
                    return;
                }
                // CẬP NHẬT MỚI: Sử dụng JavaDictionary thay vì Dictionary của C#
                var reviewData = new Android.Runtime.JavaDictionary<string, Java.Lang.Object>
                {
                    { "shopName", shopName },
                    { "stars", stars },
                    { "comment", comment },
                    { "timestamp", FieldValue.ServerTimestamp() } // Đã thêm dấu () vào đây
                };

                FirebaseFirestore.Instance.Collection("reviews").Add(reviewData)
                    .AddOnSuccessListener(new SuccessListener(this))
                    .AddOnFailureListener(new FailureListener(this));
            };
        }

        // --- CÁC CLASS PHỤ ĐỂ LẮNG NGHE FIREBASE ---
        private class SuccessListener : Java.Lang.Object, Android.Gms.Tasks.IOnSuccessListener
        {
            Activity _activity;
            public SuccessListener(Activity activity) { _activity = activity; }
            public void OnSuccess(Java.Lang.Object? result)
            {
                Toast.MakeText(_activity, "Cảm ơn bạn đã đánh giá!", ToastLength.Long)?.Show();
                _activity.Finish(); // Đóng màn hình quay về bản đồ
            }
        }

        private class FailureListener : Java.Lang.Object, Android.Gms.Tasks.IOnFailureListener
        {
            Activity _activity;
            public FailureListener(Activity activity) { _activity = activity; }
            public void OnFailure(Java.Lang.Exception e)
            {
                Toast.MakeText(_activity, "Lỗi kết nối mạng", ToastLength.Short)?.Show();
            }
        }
    }
}