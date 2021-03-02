using FRESHMusicPlayer.Utilities;
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

namespace FRESHMusicPlayer.Forms
{
    /// <summary>
    /// Interaction logic for MiniPlayer.xaml
    /// </summary>
    public partial class MiniPlayer : Window
    {
        private WinForms.Timer progressTimer = new WinForms.Timer { Interval = 1000 };
        private readonly MainWindow window;
        public MiniPlayer(MainWindow window)
        {
            this.window = window;
            InitializeComponent();
            window.Player.SongChanged += Player_SongChanged;
            window.Player.SongStopped += Player_SongStopped;
            window.Player.SongException += Player_SongException;
            progressTimer.Tick += ProgressTimer_Tick;
            Player_SongChanged(null, EventArgs.Empty); // call our own song changed because a song is probably already playing at this point
            UpdateControlsState();
        }

        private void ProgressTimer_Tick(object sender, EventArgs e) => ProgressTick();

        private bool isDragging = false;
        private void ProgressTick()
        {
            var time = TimeSpan.FromSeconds(Math.Floor(window.Player.CurrentBackend.CurrentTime.TotalSeconds));
            ProgressIndicator1.Text = time.ToString(@"mm\:ss");
            if (App.Config.ShowRemainingProgress) ProgressIndicator2.Text
                    = $"-{TimeSpan.FromSeconds(time.TotalSeconds - Math.Floor(window.Player.CurrentBackend.TotalTime.TotalSeconds)):mm\\:ss}";
            if (App.Config.ShowTimeInWindow) Title = $"{time:mm\\:ss}/{window.Player.CurrentBackend.TotalTime:mm\\:ss} | FRESHMusicPlayer";
            if (!isDragging) ProgressSlider.Value = time.TotalSeconds;
            window.Player.AvoidNextQueue = false;
        }
        private void Player_SongException(object sender, Handlers.PlaybackExceptionEventArgs e)
        {
            
        }

        private void Player_SongStopped(object sender, EventArgs e)
        {
            ArtistTextBlock.Text = Properties.Resources.MAINWINDOW_NOTHINGPLAYING;
            TitleTextBlock.Text = Properties.Resources.MAINWINDOW_NOTHINGPLAYING;
            progressTimer.Stop();
        }

        private void Player_SongChanged(object sender, EventArgs e)
        {
            ArtistTextBlock.Text = window.CurrentTrack.Artist;
            TitleTextBlock.Text = window.CurrentTrack.Title;

            ProgressSlider.Maximum = window.Player.CurrentBackend.TotalTime.TotalSeconds;
            progressTimer.Start();
        }

        private void TitlebarDockPanel_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void ContentGrid_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            window.Player.SongChanged -= Player_SongChanged;
            window.Player.SongStopped -= Player_SongStopped;
            window.Player.SongException -= Player_SongException;
            progressTimer.Dispose();
            Close();
        }

        private void DockPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            var fadeIn = InterfaceUtils.GetDoubleAnimation(0f, 1f, TimeSpan.FromMilliseconds(500), new PropertyPath("Opacity"));
            fadeIn.Begin(TitlebarDockPanel);
            var brightener = InterfaceUtils.GetDoubleAnimation(0.7, 1f, TimeSpan.FromMilliseconds(500), new PropertyPath("Opacity"));
            brightener.Begin(BackgroundDockPanel);

            ArtistTextBlock.Visibility = TitleTextBlock.Visibility = Visibility.Collapsed;
            ProgressIndicator1.Visibility = ProgressSlider.Visibility = ProgressIndicator2.Visibility = Visibility.Visible;
        }

        private void DockPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            var fadeOut = InterfaceUtils.GetDoubleAnimation(1f, 0f, TimeSpan.FromMilliseconds(500), new PropertyPath("Opacity"));
            fadeOut.Begin(TitlebarDockPanel);
            var dimmer = InterfaceUtils.GetDoubleAnimation(1f, 0.7, TimeSpan.FromMilliseconds(500), new PropertyPath("Opacity"));
            dimmer.Begin(BackgroundDockPanel);

            ArtistTextBlock.Visibility = TitleTextBlock.Visibility = Visibility.Visible;
            ProgressIndicator1.Visibility = ProgressSlider.Visibility = ProgressIndicator2.Visibility = Visibility.Collapsed;
        }
        public void UpdateControlsState()
        {
            if (window.Player.RepeatOnce) RepeatButtonData.Fill = new LinearGradientBrush(Color.FromRgb(105, 181, 120), Color.FromRgb(51, 139, 193), 0);
            else RepeatButtonData.Fill = (Brush)FindResource("PrimaryTextColor");
            if (window.Player.Shuffle) ShuffleButtonData.Fill = new LinearGradientBrush(Color.FromRgb(105, 181, 120), Color.FromRgb(51, 139, 193), 0);
            else ShuffleButtonData.Fill = (Brush)FindResource("PrimaryTextColor");

            if (!window.Player.Paused) PlayPauseButtonData.Data = (Geometry)FindResource("PauseIcon");
            else PlayPauseButtonData.Data = (Geometry)FindResource("PlayIcon");
        }
        private void NextButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => window.NextTrackMethod();

        private void RepeatButtonData_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            window.RepeatOneMethod();
            UpdateControlsState();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            window.PlayPauseMethod();
            UpdateControlsState();
        }

        private void ShuffleButtonData_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            window.ShuffleMethod();
            UpdateControlsState();
        }

        private void PreviousButtonData_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => window.PreviousTrackMethod();
    }
}
