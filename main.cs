using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DirectShowLib;

namespace CVideo
{
    public partial class VideoPlayerForm : Form
    {
        private FilgraphManager mediaPlayer;
        private List<string> playlist = new List<string>();
        private int currentIndex = 0;

        public VideoPlayerForm()
        {
            InitializeComponent();
            mediaPlayer = new FilgraphManager();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Video Files|*.mp4;*.avi;*.mkv;*.wmv;*.mov";
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    playlist.Clear();
                    playlist.AddRange(openFileDialog.FileNames);
                    currentIndex = 0;
                    PlayVideo(playlist[currentIndex]);
                }
            }
        }

        private void PlayVideo(string filePath)
        {
            try
            {
                mediaPlayer.RenderFile(filePath);
                mediaPlayer.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading video: " + ex.Message);
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            mediaPlayer.Run();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            mediaPlayer.Pause();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            mediaPlayer.Stop();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (playlist.Count > 1 && currentIndex < playlist.Count - 1)
            {
                currentIndex++;
                PlayVideo(playlist[currentIndex]);
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (playlist.Count > 1 && currentIndex > 0)
            {
                currentIndex--;
                PlayVideo(playlist[currentIndex]);
            }
        }

        private void btnFullscreen_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            videoPanel.Dock = DockStyle.Fill;
        }

        private void btnExitFullscreen_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            videoPanel.Dock = DockStyle.None;
        }
    }
}
