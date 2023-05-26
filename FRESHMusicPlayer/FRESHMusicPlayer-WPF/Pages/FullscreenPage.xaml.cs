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
using FRESHMusicPlayer.Utilities;
using System.Windows.Media.Animation;
using System.Windows.Threading;

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
            controlDismissTimer.Tick += ControlDismissTimer_Tick;controlDismissTimer.Start();
            if (window.Player.FileLoaded) Player_SongChanged(null, EventArgs.Empty);
            MoveHandler();
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

        private RenderTargetBitmap previousContentBitmap = null;

        private void Player_SongStopped(object sender, PlaybackStoppedEventArgs e)
        {
            previousContentBitmap = RenderBitmap(ContentGrid);

            if (e.IsEndOfPlayback)
            {
                TitleLabel.Text = ArtistLabel.Text = Properties.Resources.MAINWINDOW_NOTHINGPLAYING;
                CoverArtBox.Source = null;
                SetCoverArtVisibility(false);
            }
        }

        private void Player_SongChanged(object sender, EventArgs e)
        {
            AnimateNewTrack();

            var currentTrack = window.Player.Metadata;
            TitleLabel.Text = currentTrack.Title;
            ArtistLabel.Text = string.Join(", ", currentTrack.Artists) == "" ? Properties.Resources.MAINWINDOW_NOARTIST : string.Join(", ", currentTrack.Artists);
            ProgressBar.Maximum = window.Player.CurrentBackend.TotalTime.TotalSeconds;
            if (window.Player.CurrentBackend.TotalTime.TotalSeconds != 0) ProgressIndicator2.Text = window.Player.CurrentBackend.TotalTime.ToString(@"mm\:ss");
            else ProgressIndicator2.Text = "∞";
            if (currentTrack.CoverArt is null)
            {
                CoverArtBox.Source = null;
                SetCoverArtVisibility(false);
            }
            else
            {
                CoverArtBox.Source = BackgroundCoverArtBox.Source = BitmapFrame.Create(new MemoryStream(currentTrack.CoverArt), BitmapCreateOptions.None, BitmapCacheOption.None);
                SetCoverArtVisibility(true);
            }

            var lyrics = new Controls.Lyrics.Lyrics();
            lyrics.Initialize(window);
            if (lyrics.HandleLyrics())
            {
                InfoThing.Content = lyrics;
            }
            else
            {
                InfoThing.Content = null;
            }

        }

        private async void AnimateNewTrack()
        {
            if (ContentGrid.ActualHeight <= 0 || ContentGrid.ActualWidth <= 0) return;

            TransitionRectangle.Source = previousContentBitmap;
            ContentGrid.RenderTransform = new TranslateTransform(ContentGrid.ActualWidth * 2, 0);
            var x = InterfaceUtils.GetDoubleAnimation(
                ContentGrid.ActualWidth * 2,
                0,
                TimeSpan.FromMilliseconds(1000),
                new PropertyPath("(Grid.RenderTransform).(TranslateTransform.X)"),
                new ExponentialEase { EasingMode = EasingMode.EaseInOut, Exponent = 7 });
            await x.BeginStoryboardAsync(ContentGrid);
            TransitionRectangle.Source = null;

        }

        private RenderTargetBitmap RenderBitmap(FrameworkElement element)
        {
            var topLeft = 0;
            var topRight = 0;
            var width = (int)element.ActualWidth;
            var height = (int)element.ActualHeight;
            var dpiX = 96; // the DPI at 100% scale; things will break in high DPI if this adapts to the DPI
            var dpiY = 96; // TODO: investigate why

            var pixelFormat = PixelFormats.Default;
            var elementBrush = new VisualBrush(element);
            var visual = new DrawingVisual();
            var drawingContext = visual.RenderOpen();

            drawingContext.DrawRectangle(elementBrush, null, new Rect(topLeft, topRight, width, height));
            drawingContext.Close();
            var bitmap = new RenderTargetBitmap(width, height, dpiX, dpiY, pixelFormat);
            bitmap.Render(visual);
            return bitmap;
        }

        public void SetCoverArtVisibility(bool mode)
        {
            if (!mode) CoverArtArea.Width = new GridLength(5);
            else CoverArtArea.Width = new GridLength(155);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            window.Player.SongStopped -= Player_SongStopped;
            window.Player.SongChanged -= Player_SongChanged;
            window.ProgressTimer.Tick -= ProgressTimer_Tick;
            controlDismissTimer.Tick -= ControlDismissTimer_Tick;
            controlDismissTimer.Stop();
            Mouse.OverrideCursor = null;

            window.WindowState = WindowState.Normal;
            window.WindowStyle = WindowStyle.SingleBorderWindow;
            window.TracksTab.Visibility = window.ArtistsTab.Visibility = window.AlbumsTab.Visibility = window.PlaylistsTab.Visibility = window.ImportTab.Visibility = Visibility.Visible;
            if (!window.IsControlsBoxVisible) window.ShowControlsBox();
            if (previousMenu != Tab.Fullscreen) window.ChangeTabs(previousMenu);
            else window.ChangeTabs(Tab.Import);
        }

        private readonly DispatcherTimer controlDismissTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(2000), IsEnabled = true };

        private Point lastMouseMovePosition;
        private bool isMouseMoving = false;
        private void Page_MouseMove(object sender, MouseEventArgs e) => MoveHandler();

        private async void ControlDismissTimer_Tick(object sender, EventArgs e)
        {
            MoveHandler();

            if (isMouseMoving) return;
            if (window.IsMouseOver && !IsMouseOver || FocusModeCheckBox.IsMouseOver || BackButton.IsMouseOver) return; // cursor is probably over controls, don't hide yet
            controlDismissTimer.Stop();
            if (window.IsControlsBoxVisible) window.HideControlsBox();
            
            Mouse.OverrideCursor = Cursors.None;
            TopBar.Visibility = TopBarOverlay.Visibility = Visibility.Hidden;
            await window.HideAuxilliaryPane();
        }

        private void MoveHandler()
        {
            if (lastMouseMovePosition != null)
            {
                var position = Mouse.GetPosition(window);
                if (Math.Abs(position.X - lastMouseMovePosition.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - lastMouseMovePosition.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (!controlDismissTimer.IsEnabled)
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
            lastMouseMovePosition = Mouse.GetPosition(window);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => window.ChangeTabs(Tab.Playlists);

        private void FocusMode_Changed(object sender, RoutedEventArgs e)
        {
            if ((bool)FocusModeCheckBox.IsChecked)
            {
                CoverArtOverlay.Opacity = 1;
                CoverArtOverlay.Fill = (Brush)FindResource("BackgroundColor");
                ProgressBar.Visibility = ProgressIndicator1.Visibility = ProgressIndicator2.Visibility = InfoThing.Visibility = Visibility.Hidden;
                MainViewBox.Stretch = Stretch.None;
            }
            else
            {
                CoverArtOverlay.Opacity = 0.55;
                CoverArtOverlay.Fill = (Brush)FindResource("ForegroundColor");
                ProgressBar.Visibility = ProgressIndicator1.Visibility = ProgressIndicator2.Visibility = InfoThing.Visibility = Visibility.Visible;
                MainViewBox.Stretch = Stretch.Uniform;
            }
        }
    }
}
