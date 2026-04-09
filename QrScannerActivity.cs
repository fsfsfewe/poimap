using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Widget;
using Xamarin.Google.MLKit.Vision.Barcode.Common;
using Xamarin.Google.MLKit.Vision.CodeScanner;

namespace poimap
{
    [Activity(Label = "Quét Mã Kích Hoạt")]
    public class QrScannerActivity : Activity, IOnSuccessListener, IOnFailureListener, IOnCanceledListener
    {
        private const string SECRET_CODE = "POIMAP_QUAN4_2026";

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            try
            {
                // Khởi tạo máy quét
                IGmsBarcodeScanner scanner = GmsBarcodeScanning.GetClient(this);

                // Mở Camera
                scanner.StartScan()
                       .AddOnSuccessListener(this)
                       .AddOnFailureListener(this)
                       .AddOnCanceledListener(this);
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(this, "Lỗi khởi tạo Camera: " + ex.Message, ToastLength.Long)?.Show();
                Finish();
            }
        }

        // KHI QUÉT THÀNH CÔNG
        // KHI QUÉT THÀNH CÔNG
        public void OnSuccess(Java.Lang.Object? result)
        {


            // Ép kiểu về Barcode
            Barcode? barcode = result as Barcode;
            string rawValue = barcode?.RawValue ?? "";

            if (rawValue == SECRET_CODE)
            {
                IntroActivity.IsUnlocked = true;
                Toast.MakeText(this, "Mở khóa thành công! Chào mừng bạn.", ToastLength.Long)?.Show();

                Intent intent = new Intent(this, typeof(MainActivity));

                // THÊM DÒNG NÀY ĐỂ XÓA SẠCH LỊCH SỬ (Không cho back về Intro/QR nữa)
                intent.AddFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);

                StartActivity(intent);
            }
            else
            {
                Toast.MakeText(this, "Mã QR không hợp lệ! Vui lòng thử lại.", ToastLength.Long)?.Show();
            }

            Finish();
        }

        // KHI MÁY QUÉT BỊ LỖI
        public void OnFailure(Java.Lang.Exception e)
        {
            Toast.MakeText(this, "Lỗi quét mã: " + e.Message, ToastLength.Short)?.Show();
            Finish();
        }

        // KHI HỦY BỎ
        public void OnCanceled()
        {
            Finish();
        }
    }
}