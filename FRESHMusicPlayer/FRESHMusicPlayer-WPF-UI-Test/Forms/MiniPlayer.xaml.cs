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
using System.Windows.Interop;
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
        public MiniPlayer(MainWindow window) // TODO: there's a lot of code copied from the main window here, maybe more stuff could be shared?
        {
            this.window = window;
            InitializeComponent();
            window.Player.SongChanged += Player_SongChanged;
            window.Player.SongStopped += Player_SongStopped;
            progressTimer.Tick += ProgressTimer_Tick;
            Player_SongChanged(null, EventArgs.Empty); // call our own song changed because a song is probably already playing at this point
            UpdateControlsState();
            VolumeSlider.Value = window.VolumeBar.Value;
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
            progressTimer.Dispose();
            Close();
        }

        private void DockPanel_MouseEnter(object sender, MouseEventArgs e)
        {
           

            var fadeIn = InterfaceUtils.GetDoubleAnimation(0f, 1f, TimeSpan.FromMilliseconds(500), new PropertyPath("Opacity"));
            fadeIn.Begin(TitlebarDockPanel);
            var brightener = InterfaceUtils.GetDoubleAnimation(0.8, 1f, TimeSpan.FromMilliseconds(500), new PropertyPath("Opacity"));
            brightener.Begin(BackgroundDockPanel);

            ArtistTextBlock.Visibility = TitleTextBlock.Visibility = Visibility.Collapsed;
            ProgressIndicator1.Visibility = ProgressSlider.Visibility = ProgressIndicator2.Visibility = Visibility.Visible;
        }

        private void DockPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            var fadeOut = InterfaceUtils.GetDoubleAnimation(1f, 0f, TimeSpan.FromMilliseconds(500), new PropertyPath("Opacity"));
            fadeOut.Begin(TitlebarDockPanel);
            var dimmer = InterfaceUtils.GetDoubleAnimation(1f, 0.8, TimeSpan.FromMilliseconds(500), new PropertyPath("Opacity"));
            dimmer.Begin(BackgroundDockPanel);

            ArtistTextBlock.Visibility = TitleTextBlock.Visibility = Visibility.Visible;
            ProgressIndicator1.Visibility = ProgressSlider.Visibility = ProgressIndicator2.Visibility = Visibility.Collapsed;
        }
        public void UpdateControlsState()
        {
            if (window.Player.Queue.RepeatMode == RepeatMode.RepeatAll)
            {
                RepeatButtonData.Data = (Geometry)FindResource("RepeatAllIcon");
                RepeatButtonData.Fill = new LinearGradientBrush(Color.FromRgb(105, 181, 120), Color.FromRgb(51, 139, 193), 0);
            }
            else if (window.Player.Queue.RepeatMode == RepeatMode.RepeatOne)
            {
                RepeatButtonData.Data = (Geometry)FindResource("RepeatOneIcon");
                RepeatButtonData.Fill = new LinearGradientBrush(Color.FromRgb(105, 181, 120), Color.FromRgb(51, 139, 193), 0);
            }
            else
            {
                RepeatButtonData.Data = (Geometry)FindResource("RepeatAllIcon");
                RepeatButtonData.Fill = (Brush)FindResource("PrimaryTextColor");
            }

            if (window.Player.Queue.Shuffle) ShuffleButtonData.Fill = new LinearGradientBrush(Color.FromRgb(105, 181, 120), Color.FromRgb(51, 139, 193), 0);
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

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            window.Player.Volume = (float)(VolumeSlider.Value / 100);
            window.VolumeBar.Value = VolumeSlider.Value;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            var currentScreen = WinForms.Screen.FromHandle(new WindowInteropHelper(this).Handle).Bounds;
            var window = new System.Drawing.Rectangle((int)Left, (int)Top, (int)Width, (int)Height);
            var topHalf = new System.Drawing.Rectangle(currentScreen.X, currentScreen.Y, currentScreen.Width, currentScreen.Height / 2);
            if (topHalf.Contains(window))
            {
                DockPanel.SetDock(ContentGrid, Dock.Top);
                DockPanel.SetDock(TitlebarDockPanel, Dock.Bottom);
            }
            else
            {
                DockPanel.SetDock(ContentGrid, Dock.Bottom);
                DockPanel.SetDock(TitlebarDockPanel, Dock.Top);
            }
        }
    }
}
