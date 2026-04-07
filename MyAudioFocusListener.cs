using Android.Media;

namespace poimap
{
    // Class này đóng vai trò lắng nghe các sự kiện âm thanh của hệ thống điện thoại
    public class MyAudioFocusListener : Java.Lang.Object, AudioManager.IOnAudioFocusChangeListener
    {
        private MediaPlayer? _player;

        public MyAudioFocusListener(MediaPlayer? player)
        {
            _player = player;
        }

        public void OnAudioFocusChange(AudioFocus focusChange)
        {
            if (_player == null) return;

            switch (focusChange)
            {
                case AudioFocus.Gain:
                    // ĐƯỢC CẤP MICRO: Có thể là lần đầu phát, hoặc vừa nghe xong cuộc gọi điện thoại
                    if (!_player.IsPlaying) _player.Start();
                    break;

                case AudioFocus.Loss:
                    // BỊ TƯỚC MICRO VĨNH VIỄN: Do người dùng mở app Spotify, YouTube, hoặc mở một màn hình Audio khác trong app
                    if (_player.IsPlaying)
                    {
                        _player.Stop();
                        _player.Release();
                    }
                    break;

                case AudioFocus.LossTransient:
                case AudioFocus.LossTransientCanDuck:
                    // BỊ TƯỚC MICRO TẠM THỜI: Do có cuộc gọi đến, hoặc có chuông báo thức, thông báo tin nhắn
                    if (_player.IsPlaying) _player.Pause();
                    break;
            }
        }


    }
}