using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;

namespace FRESHMusicPlayer.Pages
{
    /// <summary>
    /// Interaction logic for FullscreenPage.xaml
    /// </summary>
    public partial class FullscreenPage : Page
    {
        private readonly MainWindow window;
        public FullscreenPage(MainWindow window)
        {
            this.window = window;
            InitializeComponent();
            window.HideControlsBox();
            window.Player.SongStopped += Player_SongStopped;
            window.Player.SongChanged += Player_SongChanged;
            window.ProgressTimer.Tick += ProgressTimer_Tick;
            Player_SongChanged(null, EventArgs.Empty);
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            var time = window.Player.CurrentTime;
            ProgressIndicator1.Text = time.ToString(@"mm\:ss");
            if (App.Config.ShowRemainingProgress) ProgressIndicator2.Text
                    = $"-{TimeSpan.FromSeconds(time.TotalSeconds - Math.Floor(window.Player.CurrentBackend.TotalTime.TotalSeconds)):mm\\:ss}";
            if (App.Config.ShowTimeInWindow) Title = $"{time:mm\\:ss}/{window.Player.CurrentBackend.TotalTime:mm\\:ss} | {MainWindow.WindowName}";
            ProgressBar.Value = time.TotalSeconds;
        }

        private void Player_SongChanged(object sender, EventArgs e)
        {
            var CurrentTrack = window.Player.CurrentBackend.Metadata;
            Title = $"{string.Join(", ", CurrentTrack.Artists)} - {CurrentTrack.Title} | {MainWindow.WindowName}";
            TitleLabel.Text = CurrentTrack.Title;
            ArtistLabel.Text = string.Join(", ", CurrentTrack.Artists) == "" ? Properties.Resources.MAINWINDOW_NOARTIST : string.Join(", ", CurrentTrack.Artists);
            ProgressBar.Maximum = window.Player.CurrentBackend.TotalTime.TotalSeconds;
            if (window.Player.CurrentBackend.TotalTime.TotalSeconds != 0) ProgressIndicator2.Text = window.Player.CurrentBackend.TotalTime.ToString(@"mm\:ss");
            else ProgressIndicator2.Text = "∞";
            if (CurrentTrack.CoverArt is null)
            {
                var file = window.GetCoverArtFromDirectory();
                if (file != null)
                {
                    CoverArtBox.Source = BitmapFrame.Create(file, BitmapCreateOptions.None, BitmapCacheOption.None);
                    SetCoverArtVisibility(true);
                }
                else
                {
                    CoverArtBox.Source = null;
                    SetCoverArtVisibility(false);
                }
            }
            else
            {
                CoverArtBox.Source = BackgroundCoverArtBox.Source = BitmapFrame.Create(new MemoryStream(CurrentTrack.CoverArt), BitmapCreateOptions.None, BitmapCacheOption.None);
                SetCoverArtVisibility(true);
            }
        }

        public void SetCoverArtVisibility(bool mode)
        {
            if (!mode) CoverArtArea.Width = new GridLength(5);
            else CoverArtArea.Width = new GridLength(155);
        }

        private void Player_SongStopped(object sender, EventArgs e)
        {
            TitleLabel.Text = ArtistLabel.Text = Properties.Resources.MAINWINDOW_NOTHINGPLAYING;
            CoverArtBox.Source = null;
            SetCoverArtVisibility(false);
        }

        private void Rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            if (window.IsControlsBoxVisible) window.HideControlsBox();
            else window.ShowControlsBox();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            window.Player.SongStopped -= Player_SongStopped;
            window.Player.SongChanged -= Player_SongChanged;
            window.ProgressTimer.Tick -= ProgressTimer_Tick;
        }
    }
}
