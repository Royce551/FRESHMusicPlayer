using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;

namespace FRESHMusicPlayer.Forms.TagEditor.TimedLyrics
{
    /// <summary>
    /// Interaction logic for TimedLyricsEditor.xaml
    /// </summary>
    public partial class TimedLyricsEditor : Window
    {
        private Player player;

        private WinForms.Timer progressTimer;

        public TimedLyricsEditor(Player player)
        {
            InitializeComponent();
            this.player = player;
            player.SongChanged += Player_SongChanged;
            player.SongStopped += Player_SongStopped;
            ProgressSlider.Maximum = player.CurrentBackend.TotalTime.TotalSeconds;
            progressTimer = new WinForms.Timer
            {
                Interval = 1
            };
            progressTimer.Tick += ProgressTimer_Tick;
            ProgressIndicator2.Text = player.CurrentBackend.TotalTime.ToString(@"mm\:ss\:fff");
            progressTimer.Enabled = player.Playing;
        }

        private void Player_SongStopped(object sender, EventArgs e)
        {
            progressTimer.Stop();
        }

        private void Player_SongChanged(object sender, EventArgs e)
        {
            ProgressIndicator2.Text = player.CurrentBackend.TotalTime.ToString(@"mm\:ss\:fff");
            ProgressSlider.Maximum = player.CurrentBackend.TotalTime.TotalSeconds;
            progressTimer.Start();
        }

        private void ProgressTimer_Tick(object sender, EventArgs e) => ProgressTick();
        private void ProgressTick()
        {
            var time = player.CurrentBackend.CurrentTime;
            ProgressIndicator1.Text = time.ToString(@"mm\:ss\:fff");
            ProgressSlider.Value = time.TotalSeconds;
        }
        private void PlayPauseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!player.Playing) return;
            if (player.Paused)
            {
                player.ResumeMusic();
                progressTimer.Start();
            }
            else
            {
                player.PauseMusic();
                progressTimer.Stop();
            }
            if (!player.Paused) PlayPauseButtonData.Data = (Geometry)FindResource("PauseIcon");
            else PlayPauseButtonData.Data = (Geometry)FindResource("PlayIcon");
        }
    }
}
