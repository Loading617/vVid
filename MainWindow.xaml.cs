using LibVLCSharp.Shared;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace vVid
{
    public partial class MainWindow : Window
    {
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            Core.Initialize();

            _libVLC = new LibVLC("--no-xlib");
            _mediaPlayer = new MediaPlayer(_libVLC);
            videoView.MediaPlayer = _mediaPlayer;

            _mediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
            _mediaPlayer.TimeChanged += MediaPlayer_TimeChanged;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _timer.Tick += (s, e) =>
            {
                if (_mediaPlayer?.IsPlaying == true)
                {
                    seekSlider.Value = _mediaPlayer.Time;
                }
            };
        }

        private void OpenVideo_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Video files|*.mp4;*.mkv;*.avi;*.mov|All files|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                _mediaPlayer.Media = new Media(_libVLC, new Uri(dialog.FileName));
                _mediaPlayer.Play();
                _timer.Start();
            }
        }

        private void OpenSubtitle_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Subtitles|*.srt;*.sub;*.ass"
            };
            if (dialog.ShowDialog() == true && _mediaPlayer.IsPlaying)
            {
                _mediaPlayer.AddSlave(MediaSlaveType.Subtitle, dialog.FileName, true);
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e) => _mediaPlayer.Play();
        private void Pause_Click(object sender, RoutedEventArgs e) => _mediaPlayer.Pause();
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Stop();
            _timer.Stop();
            seekSlider.Value = 0;
        }

        private void MediaPlayer_LengthChanged(object sender, MediaPlayerLengthChangedEventArgs e)
        {
            Dispatcher.Invoke(() => seekSlider.Maximum = e.Length);
        }

        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            Dispatcher.Invoke(() => seekSlider.Value = e.Time);
        }

        private void SeekSlider_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_mediaPlayer?.IsPlaying == true)
            {
                _mediaPlayer.Time = (long)seekSlider.Value;
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _mediaPlayer.Volume = (int)e.NewValue;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
        }
    }
}
