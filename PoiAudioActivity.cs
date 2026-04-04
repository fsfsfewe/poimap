using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Widget;

namespace poimap
{
    [Activity(Label = "Nghe Thuyết Minh")]
    public class PoiAudioActivity : Activity
    {
        private MediaPlayer? _mediaPlayer;
        private ImageButton? _btnPlayPause;
        private bool _isPlaying = false;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout_poi_audio);

            // Nhận tên POI và link Audio từ màn hình bản đồ truyền sang
            string poiName = Intent?.GetStringExtra("POI_NAME") ?? "Địa điểm";
            string audioUrl = Intent?.GetStringExtra("AUDIO_URL") ?? "";

            FindViewById<TextView>(Resource.Id.txtPoiName)!.Text = poiName;
            _btnPlayPause = FindViewById<ImageButton>(Resource.Id.btnPlayPause);

            // Thiết lập Trình phát nhạc
            _mediaPlayer = new MediaPlayer();
            try
            {
                _mediaPlayer.SetDataSource(audioUrl);
                _mediaPlayer.PrepareAsync(); // Tải nhạc từ mạng
                _mediaPlayer.Prepared += (s, e) => Toast.MakeText(this, "Đã tải xong Audio, sẵn sàng phát!", ToastLength.Short)?.Show();
            }
            catch { Toast.MakeText(this, "Lỗi tải Audio", ToastLength.Short)?.Show(); }

            // Bắt sự kiện nút Play/Pause
            if (_btnPlayPause != null)
            {
                _btnPlayPause.Click += (s, e) =>
                {
                    if (_isPlaying)
                    {
                        _mediaPlayer.Pause();
                        _btnPlayPause.SetImageResource(Android.Resource.Drawable.IcMediaPlay);
                    }
                    else
                    {
                        _mediaPlayer.Start();
                        _btnPlayPause.SetImageResource(Android.Resource.Drawable.IcMediaPause);
                    }
                    _isPlaying = !_isPlaying;
                };
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // Tắt app thì nhớ giải phóng bộ nhớ của loa
            _mediaPlayer?.Release();
            _mediaPlayer = null;
        }
    }
}