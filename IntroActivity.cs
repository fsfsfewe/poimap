using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Views; // Thêm thư viện này để dùng View

namespace poimap
{
    [Activity(Label = "PoiMap Tour", Theme = "@style/AppTheme", MainLauncher = true)]
    public class IntroActivity : Activity
    {
        public static bool IsUnlocked = false;

        // Lưu giữ ngôn ngữ hiện tại (mặc định là Tiếng Việt)
        public static string CurrentLang = "vi";

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (IsUnlocked)
            {
                Intent intentMap = new Intent(this, typeof(MainActivity));
                StartActivity(intentMap);
                Finish();
                return;
            }

            SetContentView(Resource.Layout.activity_intro);

            // 1. Ánh xạ nút bấm chuyển trang
            Button btnContinue = FindViewById<Button>(Resource.Id.btnContinueToQr)!;
            btnContinue.Click += (s, e) =>
            {
                Intent intentQr = new Intent(this, typeof(QrScannerActivity));
                StartActivity(intentQr);
            };

            // 2. Ánh xạ nút ngôn ngữ ở góc trên
            LinearLayout? btnLanguage = FindViewById<LinearLayout>(Resource.Id.btnLanguage);
            if (btnLanguage != null)
            {
                btnLanguage.Click += (s, e) => ShowLanguageMenu(btnLanguage);
            }

            // 3. Cập nhật UI theo ngôn ngữ hiện tại (Giả lập dịch mộc)
            UpdateTextUI();
        }

        // Hàm hiển thị Menu xổ xuống khi bấm vào góc trên phải
        private void ShowLanguageMenu(View anchorView)
        {
            PopupMenu popup = new PopupMenu(this, anchorView);

            // Thêm 2 tùy chọn vào Menu (Dùng Emoji cờ cho sinh động)
            popup.Menu.Add(0, 1, 0, "🇻🇳 Tiếng Việt");
            popup.Menu.Add(0, 2, 0, "🇬🇧 English");

            popup.MenuItemClick += (s, args) =>
            {
                if (args.Item.ItemId == 1)
                {
                    ChangeAppLanguage("vi");
                }
                else if (args.Item.ItemId == 2)
                {
                    ChangeAppLanguage("en");
                }
            };

            popup.Show();
        }

        // Hàm đổi ngôn ngữ và load lại màn hình
        private void ChangeAppLanguage(string langCode)
        {
            if (CurrentLang == langCode) return; // Nếu chọn lại ngôn ngữ cũ thì không làm gì cả

            CurrentLang = langCode;

            // Ép hệ thống tải lại Activity này để vẽ lại chữ ngôn ngữ mới
            Intent restartIntent = new Intent(this, typeof(IntroActivity));
            restartIntent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
            StartActivity(restartIntent);
            Finish();
        }

        // Hàm dịch text thủ công ngay trong code (Do ứng dụng hiện tại bạn chưa dùng file strings.xml)
        private void UpdateTextUI()
        {
            // Ánh xạ các View từ XML
            TextView? txtLangLabel = FindViewById<TextView>(Resource.Id.txtLangLabel);
            TextView? txtAppDesc = FindViewById<TextView>(Resource.Id.txtAppDesc);
            Button? btnContinue = FindViewById<Button>(Resource.Id.btnContinueToQr);
    ImageView? imgLangFlag = FindViewById<ImageView>(Resource.Id.imgLangFlag);

            if (CurrentLang == "en")
            {
                // Cập nhật nội dung tiếng Anh
                if (txtLangLabel != null) txtLangLabel.Text = "Language";
                if (txtAppDesc != null) txtAppDesc.Text = "Discover District 4's cuisine and culture in a completely new way through an automatic Audio system.";
                if (btnContinue != null) btnContinue.Text = "CONTINUE";

                // ĐỔI ẢNH CỜ SANG NƯỚC ANH
                imgLangFlag?.SetImageResource(Resource.Drawable.ic_flag_uk);
            }
            else // Mặc định là Tiếng Việt ("vi")
            {
                // Cập nhật nội dung tiếng Việt
                if (txtLangLabel != null) txtLangLabel.Text = "Ngôn ngữ";
                if (txtAppDesc != null) txtAppDesc.Text = "Khám phá ẩm thực và văn hóa Quận 4 theo cách hoàn toàn mới qua hệ thống Audio tự động.";
                if (btnContinue != null) btnContinue.Text = "TIẾP TỤC";

                // ĐỔI ẢNH CỜ SANG VIỆT NAM
                imgLangFlag?.SetImageResource(Resource.Drawable.ic_flag_vn);
            }
        }
    }
}