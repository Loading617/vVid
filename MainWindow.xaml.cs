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
        private List<string> playlist = new();
        private int currentIndex = -1;

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
                Multiselect = true
            };
            if (dialog.ShowDialog() == true)
            {
                _mediaPlayer.Media = new Media(_libVLC, new Uri(dialog.FileName));
                _mediaPlayer.Play();
                _timer.Start();
                playlist = dialog.FileNames.ToList();
                currentIndex = 0;
                PlayVideoAtIndex(currentIndex);
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

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex + 1 < playlist.Count)
            {
                currentIndex++;
                PlayVideoAtIndex(currentIndex);
            }
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex - 1 >= 0)
            {
                currentIndex--;
                PlayVideoAtIndex(currentIndex);
            }
        }

        private void PlayVideoAtIndex(int index)
        {
            if (index >= 0 && index < playlist.Count)
            {
                _mediaPlayer.Media = new Media(_libVLC, new Uri(playlist[index]));
                _mediaPlayer.Play();
                _timer.Start();
            }
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

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var videoFiles = files.Where(f => f.EndsWith(".mp4") || f.EndsWith(".mkv") || f.EndsWith(".avi") || f.EndsWith(".mov")).ToList();

                if (videoFiles.Any())
                {
                    playlist = videoFiles;
                    currentIndex = 0;
                    PlayVideoAtIndex(currentIndex);
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
        }
    }
}
