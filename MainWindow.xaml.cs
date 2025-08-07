using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Windows.Storage;

namespace vVid
{

    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private async void OpenVideoFile_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);

            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".avi");
            picker.FileTypeFilter.Add(".mkv");
            picker.FileTypeFilter.Add(".mov");
            picker.FileTypeFilter.Add(".m3u");
            picker.FileTypeFilter.Add(".m3u8");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var stream = await fileOpenAsync(FileAccessMode.Read);
                vVid.Source = MediaSource.CreateFromStream(stream, file.ContentType);
                vVid.MediaPlayer.Play();
            }
        }
    }
}
