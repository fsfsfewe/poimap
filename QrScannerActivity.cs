using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Widget;
using Xamarin.Google.MLKit.Vision.Barcode;
using Xamarin.Google.MLKit.Vision.Barcode.Common;
using Xamarin.Google.MLKit.Vision.BarCode;
using Xamarin.Google.MLKit.Vision.CodeScanner;
// --- 2 THƯ VIỆN MỚI CẦN THÊM CHO VIỆC ĐỌC ẢNH ---
using Xamarin.Google.MLKit.Vision.Common;

namespace poimap
{
    [Activity(Label = "Quét Mã Kích Hoạt")]
    public class QrScannerActivity : Activity, IOnSuccessListener, IOnFailureListener, IOnCanceledListener
    {
        private const string SECRET_CODE = "POIMAP_QUAN4_2026";
        private const int PICK_IMAGE_REQUEST = 1001; // Mã định danh cho hành động mở Thư viện ảnh

        
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // 1. Set giao diện mới thay vì dùng Dialog
            SetContentView(Resource.Layout.layout_qr_scanner);

            // 2. Ánh xạ nút bấm
            Button? btnOpenScanner = FindViewById<Button>(Resource.Id.btnOpenScanner);
            Button? btnPickGallery = FindViewById<Button>(Resource.Id.btnPickGallery);

            // Trong hàm OnCreate, ngay sau khi ánh xạ btnPickGallery:
            if (btnOpenScanner != null) btnOpenScanner.Text = LanguageHelper.GetText("BẬT CAMERA QUÉT");
            if (btnPickGallery != null) btnPickGallery.Text = LanguageHelper.GetText("DÙNG ẢNH CÓ SẴN");

            // 3. Gắn sự kiện (Giữ nguyên logic cũ của bạn)
            if (btnOpenScanner != null)
            {
                btnOpenScanner.Click += (s, e) => StartCameraScan();
            }

            if (btnPickGallery != null)
            {
                btnPickGallery.Click += (s, e) => StartGalleryPick();
            }
        }

        // ==========================================
        // CÁCH 1: QUÉT BẰNG CAMERA (CODE CŨ CỦA BẠN)
        // ==========================================
        private void StartCameraScan()
        {
            try
            {
                IGmsBarcodeScanner scanner = GmsBarcodeScanning.GetClient(this);
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

        // ==========================================
        // CÁCH 2: CHỌN ẢNH TỪ THƯ VIỆN (CHỨC NĂNG MỚI)
        // ==========================================
        private void StartGalleryPick()
        {
            Intent intent = new Intent(Intent.ActionPick, Android.Provider.MediaStore.Images.Media.ExternalContentUri);
            intent.SetType("image/*");
            StartActivityForResult(intent, PICK_IMAGE_REQUEST);
        }

        // Hàm này tự động chạy khi người dùng chọn ảnh xong và quay lại App
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Nếu lấy được ảnh thành công
            if (requestCode == PICK_IMAGE_REQUEST && resultCode == Result.Ok && data != null && data.Data != null)
            {
                Android.Net.Uri imageUri = data.Data;
                ProcessImageFromGallery(imageUri);
            }
            else
            {
                Finish(); // Nếu người dùng hủy ngang lúc chọn ảnh thì đóng màn hình
            }
        }

        private void ProcessImageFromGallery(Android.Net.Uri uri)
        {
            try
            {
                // 1. Biến file ảnh thành đối tượng InputImage để AI có thể đọc
                InputImage image = InputImage.FromFilePath(this, uri);

                // ---> SỬA LỖI Ở ĐÂY <---
                // Tạo cấu hình: Chỉ đạo cho máy quét tập trung tìm Mã QR (FormatQrCode)
                BarcodeScannerOptions options = new BarcodeScannerOptions.Builder()
                    .SetBarcodeFormats(Barcode.FormatQrCode)
                    .Build();

                // 2. Truyền biến options vào hàm GetClient()
                IBarcodeScanner scanner = BarcodeScanning.GetClient(options);

                scanner.Process(image)
                       .AddOnSuccessListener(new ImageScanSuccessListener(this))
                       .AddOnFailureListener(this);
            }
            catch (System.Exception ex)
            {
                Toast.MakeText(this, "Lỗi phân tích ảnh: " + ex.Message, ToastLength.Short)?.Show();
                Finish();
            }
        }

        // ==========================================
        // LOGIC KIỂM TRA MÃ CHUNG CHO CẢ 2 CÁCH
        // ==========================================

        // Xử lý khi Camera đọc thành công
        public void OnSuccess(Java.Lang.Object? result)
        {
            Barcode? barcode = result as Barcode;
            CheckBarcodeSecret(barcode?.RawValue ?? "");
        }

        // HÀM KIỂM TRA CHUỖI BÍ MẬT
        public void CheckBarcodeSecret(string rawValue)
        {
            if (rawValue == SECRET_CODE)
            {
                IntroActivity.IsUnlocked = true;
                Toast.MakeText(this, LanguageHelper.GetText("Mở khóa thành công! Chào mừng bạn."), ToastLength.Long)?.Show();

                Intent intent = new Intent(this, typeof(MainActivity));
                intent.AddFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);
                StartActivity(intent);
            }
            else
            {
                Toast.MakeText(this, LanguageHelper.GetText("Mã QR không hợp lệ! Vui lòng thử lại."), ToastLength.Long)?.Show();
            }
            Finish();
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            Toast.MakeText(this, "Lỗi quét mã: " + e.Message, ToastLength.Short)?.Show();
            Finish();
        }

        public void OnCanceled()
        {
            Finish();
        }

        // --- CÁC CLASS PHỤ TRỢ (LẮNG NGHE SỰ KIỆN) ---

        // Class phụ xử lý kết quả khi Đọc ẢNH TĨNH thành công
        private class ImageScanSuccessListener : Java.Lang.Object, IOnSuccessListener
        {
            private QrScannerActivity _activity;
            public ImageScanSuccessListener(QrScannerActivity activity) { _activity = activity; }

            public void OnSuccess(Java.Lang.Object? result)
            {
                // Khác với Camera, quét ảnh tĩnh có thể ra nhiều mã QR cùng lúc -> Trả về 1 Danh sách (List)
                var barcodes = result as Android.Runtime.JavaList;
                if (barcodes != null && barcodes.Count > 0)
                {
                    Barcode? barcode = barcodes[0] as Barcode;
                    _activity.CheckBarcodeSecret(barcode?.RawValue ?? "");
                }
                else
                {
                    Toast.MakeText(_activity, LanguageHelper.GetText("Không tìm thấy mã QR nào trong ảnh!"), ToastLength.Long)?.Show();
                    _activity.Finish();
                }
            }
        }

        // Bắt sự kiện khi người dùng bấm ra ngoài Dialog (hủy chọn)
        private class DialogCancelListener : Java.Lang.Object, IDialogInterfaceOnCancelListener
        {
            private Activity _activity;
            public DialogCancelListener(Activity activity) { _activity = activity; }
            public void OnCancel(IDialogInterface? dialog) { _activity.Finish(); }
        }
    }
}