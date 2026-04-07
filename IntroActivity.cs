using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace poimap
{
    // Đặt MainLauncher = true để trang này chạy đầu tiên
    [Activity(Label = "PoiMap Tour", Theme = "@style/AppTheme", MainLauncher = true)]
    public class IntroActivity : Activity
    {
        // 1. THÊM BIẾN NÀY: Nó sẽ nằm trong RAM. App tắt hẳn thì nó mới bị reset về false.
        public static bool IsUnlocked = false;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // 2. KIỂM TRA TRÍ NHỚ: Nếu đã quét đúng QR trước đó (RAM vẫn còn giữ)
            if (IsUnlocked)
            {
                // Cho bay thẳng vào Bản đồ, không cần hiện trang Giới Thiệu nữa
                Intent intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
                Finish();
                return; // Dừng ngay hàm này lại
            }

            // Nếu là lần đầu mở app (hoặc vừa vuốt tắt app) thì hiện giao diện Giới thiệu
            SetContentView(Resource.Layout.activity_intro);

            // Bắt sự kiện nút Tiếp tục
            Button btnContinue = FindViewById<Button>(Resource.Id.btnContinueToQr)!;
            btnContinue.Click += (s, e) =>
            {
                // Chuyển sang trang Quét QR
                Intent intent = new Intent(this, typeof(QrScannerActivity));
                StartActivity(intent);
            };
        }
    }
}