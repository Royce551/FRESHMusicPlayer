using FRESHMusicPlayer.Forms;
using FRESHMusicPlayer.Forms.Playlists;
using FRESHMusicPlayer.Forms.TagEditor;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace FRESHMusicPlayer
{
    // Event handlers and other doo dads
    public partial class MainWindow
    {
        // Controls Box
        private void ShuffleButton_MouseLeftButtonDown(object sender, RoutedEventArgs e) => ShuffleMethod();
        private void RepeatOneButton_MouseLeftButtonDown(object sender, RoutedEventArgs e) => RepeatOneMethod();
        private void PreviousButton_MouseLeftButtonDown(object sender, RoutedEventArgs e) => PreviousTrackMethod();
        private void PlayPauseButton_MouseLeftButtonDown(object sender, RoutedEventArgs e) => PlayPauseMethod();
        private void NextTrackButton_MouseLeftButtonDown(object sender, RoutedEventArgs e) => NextTrackMethod();
        private bool isDragging = false;
        private void ProgressBar_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDragging = true;
            ProgressTimer.Stop();
        }
        private void ProgressBar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (Player.FileLoaded)
            {
                ProgressTimer.Start();
            }
            isDragging = false;
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isDragging && Player.FileLoaded)
            {
                Player.CurrentTime = TimeSpan.FromSeconds(ProgressBar.Value);
                ProgressTick();
            }
        }
        private void ProgressBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Player.FileLoaded && !isDragging)
            {
                Player.CurrentTime = TimeSpan.FromSeconds(ProgressBar.Value);
                ProgressTick();
            }
        }
        private void VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.Volume = (float)(VolumeBar.Value / 100);
        }
        private void ProgressTimer_Tick(object sender, EventArgs e) => ProgressTick();
        private void ProgressTick()
        {
            var time = Player.CurrentTime;
            ProgressIndicator1.Text = time.ToString(@"mm\:ss");
            if (App.Config.ShowRemainingProgress) ProgressIndicator2.Text
                    = $"-{TimeSpan.FromSeconds(time.TotalSeconds - Math.Floor(Player.CurrentBackend.TotalTime.TotalSeconds)):mm\\:ss}";
            if (App.Config.ShowTimeInWindow) Title = $"{time:mm\\:ss}/{Player.CurrentBackend.TotalTime:mm\\:ss} | {WindowName}";
            if (!isDragging) ProgressBar.Value = time.TotalSeconds;
            Player.AvoidNextQueue = false;
            ProgressTimer.Start(); // resync the progress timer
        }
        private void TrackTitle_MouseLeftButtonDown(object sender, RoutedEventArgs e) => ShowAuxilliaryPane(Pane.TrackInfo, 235, true);
        private void TrackContextTagEditor_Click(object sender, RoutedEventArgs e)
        {
            var tracks = new List<string>();
            if (Player.FileLoaded) tracks.Add(Player.FilePath); // if playing, edit the file the user is playing
            else tracks = Player.Queue.Queue;
            var tagEditor = new TagEditor(tracks, Player, Library);
            tagEditor.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            tagEditor.Owner = this;
            tagEditor.Show();
        }
        private void TrackContentPlaylistManagement_Click(object sender, RoutedEventArgs e)
        {
            string track;
            if (Player.FileLoaded) track = Player.FilePath;
            else track = null;
            var playlistManagement = new PlaylistManagement(Library, NotificationHandler, CurrentTab, track);
            playlistManagement.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            playlistManagement.Owner = this;
            playlistManagement.Show();
        }
        private void TrackContextMiniplayer_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            var miniPlayer = new MiniPlayer(this);
            miniPlayer.Owner = this;
            miniPlayer.ShowDialog();
            Top = miniPlayer.Top;
            Left = miniPlayer.Left;
            Show();
        }
        private void TrackContext_PauseAuto_Click(object sender, RoutedEventArgs e)
        {
            PauseAfterCurrentTrack = !PauseAfterCurrentTrack;
            UpdatePlayButtonState();
        }
        private void TrackContextArtist_Click(object sender, RoutedEventArgs e) => ChangeTabs(Tab.Artists, CurrentTrack?.Artists[0]);

        private void TrackContextAlbum_Click(object sender, RoutedEventArgs e) => ChangeTabs(Tab.Albums, CurrentTrack?.Album);

        private void TrackContextLyrics_Click(object sender, RoutedEventArgs e) => ShowAuxilliaryPane(Pane.Lyrics, openleft: true);

        private async void TrackContextOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Forms.FMPTextEntryBox(Properties.Resources.IMPORT_MANUALENTRY);
            dialog.ShowDialog();
            if (dialog.OK)
            {
                await Player.PlayAsync(dialog.Response);
            }
        }

        private void ProgressIndicator2_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (Player.FileLoaded)
            {
                if (App.Config.ShowRemainingProgress)
                {
                    App.Config.ShowRemainingProgress = false;
                    if (Player.CurrentBackend.TotalTime.TotalSeconds != 0) ProgressIndicator2.Text = Player.CurrentBackend.TotalTime.ToString(@"mm\:ss");
                    else ProgressIndicator2.Text = "∞";
                }
                else App.Config.ShowRemainingProgress = true;
            }
        }

        private void CoverArtBox_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (CurrentTrack != null)
                CoverArtBoxToolTip.Source = BitmapFrame.Create(new MemoryStream(CurrentTrack.CoverArt), BitmapCreateOptions.None, BitmapCacheOption.None);
        }

        private void CoverArtBox_ToolTipClosing(object sender, ToolTipEventArgs e)
        {
            CoverArtBoxToolTip.Source = null;
        }

        // NavBar
        private void TracksTab_MouseDown(object sender, RoutedEventArgs e) => ChangeTabs(Tab.Tracks);
        private void ArtistsTab_MouseDown(object sender, RoutedEventArgs e) => ChangeTabs(Tab.Artists);
        private void AlbumsTab_MouseDown(object sender, RoutedEventArgs e) => ChangeTabs(Tab.Albums);
        private void PlaylistsTab_MouseDown(object sender, RoutedEventArgs e) => ChangeTabs(Tab.Playlists);
        private void ImportTab_MouseDown(object sender, RoutedEventArgs e) => ChangeTabs(Tab.Import);
        private void SettingsButton_Click(object sender, RoutedEventArgs e) => ShowAuxilliaryPane(Pane.Settings, 335);
        private void SearchButton_Click(object sender, RoutedEventArgs e) => ShowAuxilliaryPane(Pane.Search, 335);
        private void QueueManagementButton_Click(object sender, RoutedEventArgs e) => ShowAuxilliaryPane(Pane.QueueManagement, 335);
        private void NotificationButton_Click(object sender, RoutedEventArgs e) => ShowAuxilliaryPane(Pane.Notifications);

        private void NotificationHandler_NotificationInvalidate(object sender, EventArgs e)
        {
            NotificationCounterLabel.Text = NotificationHandler.Notifications.Count.ToString();
            if (NotificationHandler.Notifications.Count != 0)
            {
                NotificationButton.Visibility = Visibility.Visible;
                NotificationCounterLabel.Visibility = Visibility.Visible;
            }
            else
            {
                NotificationButton.Visibility = Visibility.Collapsed;
                NotificationCounterLabel.Visibility = Visibility.Collapsed;
            }
            foreach (Notification box in NotificationHandler.Notifications)
            {
                if (box.DisplayAsToast && !box.Read)
                {
                    if (CurrentPane != Pane.Notifications)
                    {
                        ShowAuxilliaryPane(Pane.Notifications);
                        break;
                    }
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.OriginalSource is TextBox || e.OriginalSource is ListBoxItem) || Keyboard.IsKeyDown(Key.LeftCtrl))
                switch (e.Key)
                {
                    case Key.Q:
                        ShowAuxilliaryPane(Pane.Settings, 335);
                        break;
                    case Key.A:
                        ChangeTabs(Tab.Tracks);
                        break;
                    case Key.S:
                        ChangeTabs(Tab.Artists);
                        break;
                    case Key.D:
                        ChangeTabs(Tab.Albums);
                        break;
                    case Key.F:
                        ChangeTabs(Tab.Playlists);
                        break;
                    case Key.G:
                        ChangeTabs(Tab.Import);
                        break;
                    case Key.E:
                        ShowAuxilliaryPane(Pane.Search, 335);
                        break;
                    case Key.R:
                        ShowAuxilliaryPane(Pane.TrackInfo, 235, true);
                        break;
                    case Key.W:
                        ShowAuxilliaryPane(Pane.QueueManagement, 335);
                        break;
                    case Key.Space:
                        PlayPauseMethod();
                        break;
                    case Key.P:
                        var printdlg = new PrintDialog();
                        var result = printdlg.ShowDialog();
                        if (result != null && result != false) 
                            printdlg.PrintVisual(new PrintOutput(this, CurrentTrack.Album), CurrentTrack.Album);
                        break;
                }
            switch (e.Key)
            {
                case Key.F1:
                    Process.Start("https://royce551.github.io/FRESHMusicPlayer/docs/index.html");
                    break;
                case Key.F2:
                    var accent = FindResource("AccentColor") as SolidColorBrush;
                    var accent2 = accent.Clone();
                    var random = new Random();
                    var r1 = (byte)random.Next(1, 256);
                    var g1 = (byte)random.Next(1, 256);
                    var b1 = (byte)random.Next(1, 256);

                    var r2 = (byte)random.Next(1, 256);
                    var g2 = (byte)random.Next(1, 256);
                    var b2 = (byte)random.Next(1, 256);

                    accent2.Color = Color.FromRgb(r1, g1, b1);
                    var gradient = FindResource("AccentGradientColor") as LinearGradientBrush;
                    var gradient2 = gradient.Clone();
                    gradient2.GradientStops[0].Color = Color.FromRgb(r1, g1, b1);
                    gradient2.GradientStops[1].Color = Color.FromRgb(r2, g2, b2);
                    Application.Current.Resources["AccentColor"] = accent2;
                    Application.Current.Resources["AccentGradientColor"] = gradient2;
                    UpdateControlsBoxColors();
                    break;
                case Key.F11:
                    if (CurrentTab != Tab.Fullscreen) ChangeTabs(Tab.Fullscreen);
                    else ChangeTabs(Tab.Playlists);
                    break;
                case Key.F12:
                    NotificationHandler.Add(new Notification { ContentText = "Debug Tools" });
                    NotificationHandler.Add(new Notification
                    {
                        ButtonText = "Garbage Collect",
                        OnButtonClicked = () =>
                        {
                            GC.Collect(2);
                            return false;
                        }
                    });
                    NotificationHandler.Add(new Notification
                    {
                        ButtonText = "Throw exception",
                        OnButtonClicked = () =>
                        {
                            throw new Exception("Exception for debugging");
                        }
                    });
                    NotificationHandler.Add(new Notification
                    {
                        ButtonText = "Make FMP topmost",
                        OnButtonClicked = () =>
                        {
                            Topmost = !Topmost;
                            return false;
                        }
                    });
                    break;
            }
        }
        // Window
        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private async void ControlsBox_Drop(object sender, DragEventArgs e)
        {
            Player.Queue.Clear();
            InterfaceUtils.DoDragDrop((string[])e.Data.GetData(DataFormats.FileDrop), Player, Library, import: false);
            await Player.PlayAsync();
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            VolumeBar.Value += e.Delta / 100 * 3;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            ProgressTimer.Interval = 100;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            ProgressTimer.Interval = 1000;
        }
    }
}
