using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Threading;

namespace vVid
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            mediaElement.MediaOpened += MediaElement_MediaOpened;
            mediaElement.MediaEnded += MediaElement_MediaEnded;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video files (*.mp4;*.wmv)|*.mp4;*.wmv|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                mediaElement.Source = new Uri(openFileDialog.FileName);
                mediaElement.Play();
                timer.Start();
            }
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e) => mediaElement.Play();
        private void BtnPause_Click(object sender, RoutedEventArgs e) => mediaElement.Pause();
        private void BtnStop_Click(object sender, RoutedEventArgs e) => mediaElement.Stop();

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            seekSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            mediaElement.Stop();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                seekSlider.Value = mediaElement.Position.TotalSeconds;
                txtPosition.Text = $"{mediaElement.Position:mm\\:ss} / {mediaElement.NaturalDuration.TimeSpan:mm\\:ss}";
            }
        }

        private void SeekSlider_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mediaElement.Position = TimeSpan.FromSeconds(seekSlider.Value);
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = volumeSlider.Value;
        }
    }
}
