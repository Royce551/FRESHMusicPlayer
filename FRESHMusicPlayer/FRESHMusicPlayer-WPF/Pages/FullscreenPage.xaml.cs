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
using FRESHMusicPlayer.Controls.Lyrics;
using FRESHMusicPlayer.Controls;
using WinForms = System.Windows.Forms;

namespace FRESHMusicPlayer.Pages
{
    /// <summary>
    /// Interaction logic for FullscreenPage.xaml
    /// </summary>
    public partial class FullscreenPage : UserControl
    {
        private readonly MainWindow window;
        private readonly Tab previousMenu;
        public FullscreenPage(MainWindow window, Tab previousMenu)
        {
            this.window = window;
            this.previousMenu = previousMenu;
            InitializeComponent();
            window.Player.SongStopped += Player_SongStopped;
            window.Player.SongChanged += Player_SongChanged;
            window.ProgressTimer.Tick += ProgressTimer_Tick;
            controlDismissTimer.Tick += ControlDismissTimer_Tick;
            if (window.Player.FileLoaded) Player_SongChanged(null, EventArgs.Empty);

            window.WindowStyle = WindowStyle.None;
            window.WindowState = WindowState.Maximized;
            window.TracksTab.Visibility = window.ArtistsTab.Visibility = window.AlbumsTab.Visibility = window.PlaylistsTab.Visibility = window.ImportTab.Visibility = Visibility.Collapsed;
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            var time = window.Player.CurrentTime;
            ProgressIndicator1.Text = time.ToString(@"mm\:ss");
            if (App.Config.ShowRemainingProgress) ProgressIndicator2.Text
                    = $"-{TimeSpan.FromSeconds(time.TotalSeconds - Math.Floor(window.Player.CurrentBackend.TotalTime.TotalSeconds)):mm\\:ss}";
            ProgressBar.Value = time.TotalSeconds;
        }

        private void Player_SongChanged(object sender, EventArgs e)
        {
            var CurrentTrack = window.Player.CurrentBackend.Metadata;
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

            var lyrics = new FRESHMusicPlayer.Controls.Lyrics.Lyrics();
            lyrics.Initialize(window);
            if (lyrics.HandleLyrics())
            {
                InfoThing.Content = lyrics;
            }
            else
            {
                var trackInfo = new TrackInfo();
                trackInfo.Update(CurrentTrack);
                InfoThing.Content = trackInfo;
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

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            window.Player.SongStopped -= Player_SongStopped;
            window.Player.SongChanged -= Player_SongChanged;
            window.ProgressTimer.Tick -= ProgressTimer_Tick;
            controlDismissTimer.Tick -= ControlDismissTimer_Tick;
            Mouse.OverrideCursor = null;

            window.WindowState = WindowState.Normal;
            window.WindowStyle = WindowStyle.SingleBorderWindow;
            window.TracksTab.Visibility = window.ArtistsTab.Visibility = window.AlbumsTab.Visibility = window.PlaylistsTab.Visibility = window.ImportTab.Visibility = Visibility.Visible;
            if (!window.IsControlsBoxVisible) window.ShowControlsBox();
            if (previousMenu != Tab.Fullscreen) window.ChangeTabs(previousMenu);
            else window.ChangeTabs(Tab.Import);
        }

        private readonly WinForms.Timer controlDismissTimer = new WinForms.Timer { Interval = 2000, Enabled = true };

        private Point lastMouseMovePosition;
        private bool isMouseMoving = false;
        private void Page_MouseMove(object sender, MouseEventArgs e)
        {
            if (lastMouseMovePosition != null)
            {
                var position = Mouse.GetPosition(this);
                if (Math.Abs(position.X - lastMouseMovePosition.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - lastMouseMovePosition.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (!controlDismissTimer.Enabled)
                    {
                        if (!window.IsControlsBoxVisible) window.ShowControlsBox();
                        controlDismissTimer.Start();
                        Mouse.OverrideCursor = null;
                        TopBar.Visibility = TopBarOverlay.Visibility = Visibility.Visible;
                    }
                    isMouseMoving = true;
                }
                else isMouseMoving = false;
            }
            lastMouseMovePosition = Mouse.GetPosition(this);
        }

        private void ControlDismissTimer_Tick(object sender, EventArgs e)
        {
            if (isMouseMoving) return;
            if (!IsMouseOver || FocusModeCheckBox.IsMouseOver || BackButton.IsMouseOver) return; // cursor is probably over controls, don't hide yet
            controlDismissTimer.Stop();
            window.HideControlsBox();
            Mouse.OverrideCursor = Cursors.None;
            TopBar.Visibility = TopBarOverlay.Visibility = Visibility.Hidden;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FocusMode_Changed(object sender, RoutedEventArgs e)
        {
            if ((bool)FocusModeCheckBox.IsChecked)
            {
                CoverArtOverlay.Opacity = 1;
                CoverArtOverlay.Fill = (Brush)FindResource("BackgroundColor");
            }
            else
            {
                CoverArtOverlay.Opacity = 0.55;
                CoverArtOverlay.Fill = (Brush)FindResource("ForegroundColor");
            }
        }
    }
}
