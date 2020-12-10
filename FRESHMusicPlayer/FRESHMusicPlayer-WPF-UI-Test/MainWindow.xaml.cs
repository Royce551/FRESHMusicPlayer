using ATL;
using FRESHMusicPlayer.Forms.Playlists;
using FRESHMusicPlayer.Forms.TagEditor;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Media;
using Winforms = System.Windows.Forms;

namespace FRESHMusicPlayer
{
    public enum SelectedMenu
    {
        Tracks,
        Artists,
        Albums,
        Playlists,
        Import,
        Other
    }
    public enum SelectedAuxiliaryPane
    {
        None,
        Settings,
        QueueManagement,
        Search,
        Notifications, 
        TrackInfo
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Winforms.Timer progressTimer;
        public static SelectedMenu SelectedMenu = SelectedMenu.Tracks;
        public static SelectedAuxiliaryPane SelectedAuxiliaryPane = SelectedAuxiliaryPane.None;
        public static Player Player = new Player { CurrentVolume = App.Config.Volume};
        public static NotificationHandler NotificationHandler = new NotificationHandler();
        public static bool MiniPlayerMode = false;
        public static EventHandler TabChanged;
        public static LiteDatabase Libraryv2;
        public static Track CurrentTrack;

        public SystemMediaTransportControls Smtc;
        public PlaytimeTrackingHandler TrackingHandler;
        public bool PauseAfterCurrentTrack = false;
        public MainWindow(string[] initialFile = null)
        {
            InitializeComponent();
            Player.SongChanged += Player_SongChanged;
            Player.SongStopped += Player_SongStopped;
            Player.SongException += Player_SongException;
            NotificationHandler.NotificationInvalidate += NotificationHandler_NotificationInvalidate;
            progressTimer = new Winforms.Timer
            {
                Interval = 1000
            };
            progressTimer.Tick += ProgressTimer_Tick;
            try
            {
                Libraryv2 = new LiteDatabase(Path.Combine(DatabaseHandler.DatabasePath, "database.fdb2"));
            }
            catch
            {
                NotificationHandler.Add(new Notification
                {
                    ContentText = Properties.Resources.NOTIFICATION_LIBRARYFAILED,
                    ButtonText = "Retry",
                    OnButtonClicked = () =>
                    {
                        try
                        {
                            Libraryv2 = new LiteDatabase(System.IO.Path.Combine(DatabaseHandler.DatabasePath, "database.fdb2"));
                            TracksTab.Visibility = ArtistsTab.Visibility = AlbumsTab.Visibility = PlaylistsTab.Visibility = Visibility.Visible;
                            SearchButton.Visibility = QueueManagementButton.Visibility = Visibility.Visible;
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    },
                    DisplayAsToast = true,
                    Type = NotificationType.Failure
                });
                App.Config.CurrentMenu = SelectedMenu.Import;
                TracksTab.Visibility = ArtistsTab.Visibility = AlbumsTab.Visibility = PlaylistsTab.Visibility = Visibility.Collapsed;
                SearchButton.Visibility = QueueManagementButton.Visibility = Visibility.Collapsed;
            }    
            if (initialFile != null)
            {
                Player.AddQueue(initialFile);
                Player.PlayMusic();
            }
        }
        private async void Window_SourceInitialized(object sender, EventArgs e)
        {
            UpdateIntegrations();
            ProcessSettings(true);
            await UpdateHandler.UpdateApp();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            App.Config.Volume = (int)VolumeBar.Value;
            App.Config.CurrentMenu = SelectedMenu;
            TrackingHandler?.Close();
            ConfigurationHandler.Write(App.Config);
            Libraryv2.Dispose();
            Application.Current.Shutdown();
        }
        private void Smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Dispatcher.Invoke(() => PlayPauseMethod());
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    Dispatcher.Invoke(() => PlayPauseMethod());
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Dispatcher.Invoke(() => NextTrackMethod());
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Dispatcher.Invoke(() => PreviousTrackMethod());
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    Dispatcher.Invoke(() => StopMethod());
                    break;
                default:
                    break;
            }
        }

        #region Controls
        public void PlayPauseMethod()
        {
            if (!Player.Playing) return;
            if (Player.Paused)
            {
                Player.ResumeMusic();
                SetIntegrations(MediaPlaybackStatus.Playing, CurrentTrack.Artist, CurrentTrack.AlbumArtist, CurrentTrack.Title);
                progressTimer.Start();
            }
            else
            {
                Player.PauseMusic();
                SetIntegrations(MediaPlaybackStatus.Paused);
                progressTimer.Stop();
            }
            UpdatePlayButtonState();
        }
        public void StopMethod()
        {
            Player.ClearQueue();
            Player.StopMusic();
        }
        public void NextTrackMethod() => Player.NextSong();
        public void PreviousTrackMethod() => Player.PreviousSong();
        public void ShuffleMethod()
        {
            if (Player.Shuffle)
            {
                Player.Shuffle = false;
                ShuffleButton.Fill = (Brush)FindResource("PrimaryTextColor");
            }
            else
            {
                Player.Shuffle = true;
                ShuffleButton.Fill = new LinearGradientBrush(Color.FromRgb(105, 181, 120), Color.FromRgb(51, 139, 193), 0);
                Player.ShuffleQueue();
            }
        }
        public void RepeatOneMethod()
        {
            if (Player.RepeatOnce)
            {
                Player.RepeatOnce = false;
                RepeatOneButton.Fill = (Brush)FindResource("PrimaryTextColor");
            }
            else
            {
                Player.RepeatOnce = true;
                RepeatOneButton.Fill = new LinearGradientBrush(Color.FromRgb(105, 181, 120), Color.FromRgb(51, 139, 193), 0);
            }
        }
        public void UpdatePlayButtonState()
        {
            if (!Player.Paused) PlayPauseButton.Data = (Geometry)FindResource("PauseIcon");
            else PlayPauseButton.Data = (Geometry)FindResource("PlayIcon");
        }
        #endregion
        #region Logic
        public async void SetMiniPlayerMode(bool mode)
        { // set is for things that use binding
            if (mode)
            {
                await InterfaceUtils.GetDoubleAnimation(Width, 559, TimeSpan.FromMilliseconds(100), new PropertyPath("Width")).BeginStoryboardAsync(this);
                await InterfaceUtils.GetDoubleAnimation(Height, 123, TimeSpan.FromMilliseconds(100), new PropertyPath("Height")).BeginStoryboardAsync(this);
                MainBar.Visibility = Visibility.Collapsed;
                MiniPlayerMode = true;
                Topmost = true;
            }
            else
            {
                await InterfaceUtils.GetDoubleAnimation(Width, 800, TimeSpan.FromMilliseconds(100), new PropertyPath("Width")).BeginStoryboardAsync(this);
                await InterfaceUtils.GetDoubleAnimation(Height, 540, TimeSpan.FromMilliseconds(100), new PropertyPath("Height")).BeginStoryboardAsync(this);
                MainBar.Visibility = Visibility.Visible;
                MiniPlayerMode = false;
                Topmost = false;
            }
        }
        public void SetCoverArtVisibility(bool mode)
        {
            if (!mode) CoverArtArea.Width = new GridLength(5);       
            else CoverArtArea.Width = new GridLength(75);
        }
        public void ShowAuxilliaryPane(SelectedAuxiliaryPane pane, int width = 235, bool openleft = false)
        {
            if (SelectedAuxiliaryPane == pane)
            {
                HideAuxilliaryPane();
                return;
            }
            if (SelectedAuxiliaryPane != SelectedAuxiliaryPane.None) HideAuxilliaryPane(false);
            string uri;
            switch (pane)
            {
                case SelectedAuxiliaryPane.Settings:
                    uri = "/Pages/Settings/SettingsPage.xaml";
                    break;
                case SelectedAuxiliaryPane.QueueManagement:
                    uri = "/Pages/QueueManagement/QueueManagementPage.xaml";
                    break;
                case SelectedAuxiliaryPane.Search:
                    uri = "/Pages/Library/SearchPage.xaml";
                    break;
                case SelectedAuxiliaryPane.Notifications:
                    uri = "/Pages/NotificationPage.xaml";
                    break;
                case SelectedAuxiliaryPane.TrackInfo:
                    uri = "/Pages/TrackInfoPage.xaml";
                    break;
                default:
                    return;
            }
            if (!openleft) DockPanel.SetDock(RightFrame, Dock.Right); else DockPanel.SetDock(RightFrame, Dock.Left);
            RightFrame.Visibility = Visibility.Visible;
            var sb = InterfaceUtils.GetDoubleAnimation(0, width, TimeSpan.FromMilliseconds(100), new PropertyPath("Width"));
            sb.Begin(RightFrame);
            RightFrame.Source = new Uri(uri, UriKind.Relative);
            SelectedAuxiliaryPane = pane;
            RightFrame.NavigationService.RemoveBackEntry();
        }
        public async void HideAuxilliaryPane(bool animate = true)
        {
            var sb = InterfaceUtils.GetDoubleAnimation(RightFrame.Width, 0, TimeSpan.FromMilliseconds(100), new PropertyPath("Width"));
            if (animate) await sb.BeginStoryboardAsync(RightFrame);
            else sb.Begin(RightFrame);
            RightFrame.Visibility = Visibility.Collapsed;
            RightFrame.Source = null;
            SelectedAuxiliaryPane = SelectedAuxiliaryPane.None;
        }
        public void ProcessSettings(bool initialize = false)
        {
            if (initialize)
            {
                VolumeBar.Value = App.Config.Volume;
                ChangeTabs(App.Config.CurrentMenu);
            }
            if (App.Config.PlaybackTracking) TrackingHandler = new PlaytimeTrackingHandler(Player);
            else if (TrackingHandler != null)
            {
                TrackingHandler?.Close();
                TrackingHandler = null;
            }
        }
        #region Tabs
        private void ChangeTabs(SelectedMenu tab)
        {
            SelectedMenu = tab;
            UpdateLibrary();
        }
        private void UpdateLibrary()
        {
            TextBlock tab;
            switch (SelectedMenu)
            {
                case SelectedMenu.Tracks:
                    ContentFrame.Source = new Uri("/Pages/Library/LibraryPage.xaml", UriKind.Relative);
                    ContentFrame.NavigationService.RemoveBackEntry();
                    tab = TracksTab;
                    break;
                case SelectedMenu.Artists:
                    ContentFrame.Source = new Uri("/Pages/Library/LibraryPage.xaml", UriKind.Relative);
                    ContentFrame.NavigationService.RemoveBackEntry();
                    tab = ArtistsTab;
                    break;
                case SelectedMenu.Albums:
                    ContentFrame.Source = new Uri("/Pages/Library/LibraryPage.xaml", UriKind.Relative);
                    ContentFrame.NavigationService.RemoveBackEntry();
                    tab = AlbumsTab;
                    break;
                case SelectedMenu.Playlists:
                    ContentFrame.Source = new Uri("/Pages/Library/LibraryPage.xaml", UriKind.Relative);
                    ContentFrame.NavigationService.RemoveBackEntry();
                    tab = PlaylistsTab;
                    break;
                case SelectedMenu.Import:
                    ContentFrame.Source = new Uri("/Pages/ImportPage.xaml", UriKind.Relative);
                    tab = ImportTab;
                    break;
                default:
                    tab = null;
                    break;
            }
            TabChanged?.Invoke(null, EventArgs.Empty);
            TracksTab.FontWeight = ArtistsTab.FontWeight = AlbumsTab.FontWeight = PlaylistsTab.FontWeight = ImportTab.FontWeight = FontWeights.Normal;
            tab.FontWeight = FontWeights.Bold;
        }
        #endregion


        #endregion

        #region Events
        #region player
        private void Player_SongStopped(object sender, EventArgs e)
        {   
            Title = "FRESHMusicPlayer";
            TitleLabel.Text = ArtistLabel.Text = Properties.Resources.MAINWINDOW_NOTHINGPLAYING;
            progressTimer.Stop();
            SetIntegrations(MediaPlaybackStatus.Stopped);
            SetCoverArtVisibility(false);
        }

        private void Player_SongChanged(object sender, EventArgs e)
        {
            CurrentTrack = new Track(Player.FilePath);
            Title = $"{CurrentTrack.Artist} - {CurrentTrack.Title} | FRESHMusicPlayer";
            TitleLabel.Text = CurrentTrack.Title;
            ArtistLabel.Text = CurrentTrack.Artist == "" ? Properties.Resources.MAINWINDOW_NOARTIST : CurrentTrack.Artist;
            ProgressBar.Maximum = Player.CurrentBackend.TotalTime.TotalSeconds;
            if (Player.CurrentBackend.TotalTime.TotalSeconds != 0) ProgressIndicator2.Text = Player.CurrentBackend.TotalTime.ToString(@"mm\:ss");
            else ProgressIndicator2.Text = "∞";
            SetIntegrations(MediaPlaybackStatus.Playing, CurrentTrack.Artist, CurrentTrack.AlbumArtist, CurrentTrack.Title);
            UpdatePlayButtonState();
            if (CurrentTrack.EmbeddedPictures.Count == 0)
            {
                CoverArtBox.Source = null;
                SetCoverArtVisibility(false);
            }
            else
            {
                CoverArtBox.Source = BitmapFrame.Create(new System.IO.MemoryStream(CurrentTrack.EmbeddedPictures[0].PictureData), BitmapCreateOptions.None, BitmapCacheOption.None);
                SetCoverArtVisibility(true);
            }
            progressTimer.Start();
            if (PauseAfterCurrentTrack && !Player.Paused)
            {
                PlayPauseMethod();
                PauseAfterCurrentTrack = false;
            }
        }
        private void Player_SongException(object sender, PlaybackExceptionEventArgs e)
        {
            NotificationHandler.Add(new Notification
            {
                ContentText = string.Format(Properties.Resources.MAINWINDOW_PLAYBACK_ERROR_DETAILS, e.Details),
                IsImportant = true,
                DisplayAsToast = true,
                Type = NotificationType.Failure
            });
            Player.NextSong();
        }
        #endregion
        #region ControlsBox
        private void ShuffleButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => ShuffleMethod();
        private void RepeatOneButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => RepeatOneMethod();
        private void PreviousButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => PreviousTrackMethod();
        private void PlayPauseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => PlayPauseMethod();
        private void StopButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => StopMethod();
        private void NextTrackButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => NextTrackMethod();
        private bool isDragging = false;
        private void ProgressBar_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDragging = true;
            progressTimer.Interval = 1;
            progressTimer.Stop();
        }
        private void ProgressBar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (Player.Playing)
            {
                progressTimer.Interval = 1000;
                progressTimer.Start();
            }
            isDragging = false;
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isDragging && Player.Playing)
            {
                Player.RepositionMusic((int)ProgressBar.Value);
                ProgressTick();
            }
        }
        private void ProgressBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Player.Playing && !isDragging) Player.RepositionMusic((int)ProgressBar.Value);
            ProgressTick();
        }
        private void VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.CurrentVolume = (float)(VolumeBar.Value / 100);
            if (Player.Playing) Player.UpdateSettings();
        }
        private void ProgressTimer_Tick(object sender, EventArgs e) => ProgressTick();
        private void ProgressTick()
        {
            var time = TimeSpan.FromSeconds(Math.Floor(Player.CurrentBackend.CurrentTime.TotalSeconds));
            ProgressIndicator1.Text = time.ToString(@"mm\:ss");
            if (App.Config.ShowRemainingProgress) ProgressIndicator2.Text 
                    = $"-{TimeSpan.FromSeconds(time.TotalSeconds - Math.Floor(Player.CurrentBackend.TotalTime.TotalSeconds)):mm\\:ss}";
            if (App.Config.ShowTimeInWindow) Title = $"{time:mm\\:ss}/{Player.CurrentBackend.TotalTime:mm\\:ss} | FRESHMusicPlayer";
            if (!isDragging) ProgressBar.Value = time.TotalSeconds;
            Player.AvoidNextQueue = false;
        }
        private void TrackTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => ShowAuxilliaryPane(SelectedAuxiliaryPane.TrackInfo, 235, true);
        private void TrackTitle_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cm = FindResource("MiscContext") as ContextMenu;
            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
        }
        private void TrackContextTagEditor_Click(object sender, RoutedEventArgs e)
        {
            var tracks = new List<string>();
            if (Player.Playing) tracks.Add(Player.FilePath); // if playing, edit the file the user is playing
            else tracks = Player.Queue;
            var tagEditor = new TagEditor(tracks);
            tagEditor.Show();
        }
        private void TrackContentPlaylistManagement_Click(object sender, RoutedEventArgs e)
        {
            string track;
            if (Player.Playing) track = Player.FilePath;
            else track = null;
            var playlistManagement = new PlaylistManagement(track);
            playlistManagement.Show();
        }
        private void TrackContextMiniplayer_Click(object sender, RoutedEventArgs e)
        {
            if (MiniPlayerMode) SetMiniPlayerMode(false); else SetMiniPlayerMode(true);
        }
        private void TrackContext_PauseAuto_Click(object sender, RoutedEventArgs e)
        {
            if (PauseAfterCurrentTrack) PauseAfterCurrentTrack = false;
            else
            {
                PauseAfterCurrentTrack = true;
                NotificationHandler.Add(new Notification { ContentText = Properties.Resources.NOTIFICATION_PAUSING });
            }
        }
        #endregion
        #region MenuBar
        private void TracksTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenu.Tracks);
        private void ArtistsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenu.Artists);
        private void AlbumsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenu.Albums);
        private void PlaylistsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenu.Playlists);
        private void ImportTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenu.Import);
        private void SettingsButton_Click(object sender, MouseButtonEventArgs e) => ShowAuxilliaryPane(SelectedAuxiliaryPane.Settings, 335);
        private void SearchButton_Click(object sender, MouseButtonEventArgs e) => ShowAuxilliaryPane(SelectedAuxiliaryPane.Search, 335);
        private void QueueManagementButton_Click(object sender, MouseButtonEventArgs e) => ShowAuxilliaryPane(SelectedAuxiliaryPane.QueueManagement, 335);
        private void NotificationButton_Click(object sender, MouseButtonEventArgs e) => ShowAuxilliaryPane(SelectedAuxiliaryPane.Notifications);
        #endregion
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
                    if (SelectedAuxiliaryPane != SelectedAuxiliaryPane.Notifications)
                    {
                        ShowAuxilliaryPane(SelectedAuxiliaryPane.Notifications);
                        break;
                    }
                }
            }
        }
        #endregion

        private void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            //player.AddQueue(FilePathBox.Text);
            Player.PlayMusic();                         
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.Q:
                        ShowAuxilliaryPane(SelectedAuxiliaryPane.Settings, 335);
                        break;
                    case Key.A:
                        ChangeTabs(SelectedMenu.Tracks);
                        break;
                    case Key.S:
                        ChangeTabs(SelectedMenu.Artists);
                        break;
                    case Key.D:
                        ChangeTabs(SelectedMenu.Albums);
                        break;
                    case Key.F:
                        ChangeTabs(SelectedMenu.Playlists);
                        break;
                    case Key.G:
                        ChangeTabs(SelectedMenu.Import);
                        break;
                    case Key.E:
                        ShowAuxilliaryPane(SelectedAuxiliaryPane.Search, 335);
                        break;
                    case Key.R:
                        ShowAuxilliaryPane(SelectedAuxiliaryPane.TrackInfo, 235, true);
                        break;
                    case Key.W:
                        ShowAuxilliaryPane(SelectedAuxiliaryPane.QueueManagement, 335);
                        break;
                    case Key.Space:
                        PlayPauseMethod();
                        break;
                }
            }
            switch (e.Key)
            {
                case Key.OemTilde:
                    var box = new Forms.FMPTextEntryBox(string.Empty);
                    box.ShowDialog();
                    if (box.OK) ContentFrame.Source = new Uri(box.Response, UriKind.RelativeOrAbsolute);
                    break;
                case Key.F1:
                    GC.Collect(2);
                    break;
                case Key.F2:
                    NotificationHandler.Add(new Notification { ContentText = Properties.Resources.APPLICATION_CRITICALERROR });
                    break;
                case Key.F5:
                    ContentFrame.Refresh();
                    break;
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void ControlsBox_Drop(object sender, DragEventArgs e)
        {
            string[] tracks = (string[])e.Data.GetData(DataFormats.FileDrop);
            Player.ClearQueue();
            if (tracks.Any(x => Directory.Exists(x)))
            {
                foreach (var track in tracks)
                {
                    if (Directory.Exists(track))
                    {
                        string[] paths = Directory.EnumerateFiles(tracks[0], "*", SearchOption.AllDirectories)
                        .Where(name => name.EndsWith(".mp3")
                        || name.EndsWith(".wav") || name.EndsWith(".m4a") || name.EndsWith(".ogg")
                        || name.EndsWith(".flac") || name.EndsWith(".aiff")
                        || name.EndsWith(".wma")
                        || name.EndsWith(".aac")).ToArray();
                        Player.AddQueue(paths);
                    }
                    else Player.AddQueue(track);
                }

            }
            else Player.AddQueue(tracks);
            Player.PlayMusic();
        }

        public void UpdateIntegrations()
        {
            if (Environment.OSVersion.Version.Major >= 10 && App.Config.IntegrateSMTC)
            {
                var smtcInterop = (WindowsInteropUtils.ISystemMediaTransportControlsInterop)WindowsRuntimeMarshal.GetActivationFactory(typeof(SystemMediaTransportControls));
                Window window = GetWindow(this);
                var wih = new WindowInteropHelper(window);
                IntPtr hWnd = wih.Handle;
                Smtc = smtcInterop.GetForWindow(hWnd, new Guid("99FA3FF4-1742-42A6-902E-087D41F965EC"));
                Smtc.IsPlayEnabled = true;
                Smtc.IsPauseEnabled = true;
                Smtc.IsNextEnabled = true;
                Smtc.IsStopEnabled = true;
                Smtc.IsPreviousEnabled = true;
                Smtc.ButtonPressed += Smtc_ButtonPressed;
            }
            else Smtc = null;
            if (App.Config.IntegrateDiscordRPC) Player.InitDiscordRPC("656678380283887626");
            else Player.DisposeRPC();
        }
        public void SetIntegrations(MediaPlaybackStatus status, string Artist = "Nothing Playing", string AlbumArtist = "Nothing Playing", string Title = "Nothing Playing")
        {
            if (Environment.OSVersion.Version.Major >= 10 && App.Config.IntegrateSMTC)
            {
                try
                {
                    Smtc.PlaybackStatus = status;
                    var updater = Smtc.DisplayUpdater;
                    updater.Type = MediaPlaybackType.Music;
                    updater.MusicProperties.Artist = Artist;
                    updater.MusicProperties.AlbumArtist = AlbumArtist;
                    updater.MusicProperties.Title = Title;
                    updater.Update();
                }
                catch
                {
                    // TODO: HACK - ignored; the way i'm detecting windows 10 currently does not work
                }
            }
            if (App.Config.IntegrateDiscordRPC)
            {
                string activity = "";
                switch (status)
                {
                    case MediaPlaybackStatus.Playing:
                        activity = "play";
                        break;
                    case MediaPlaybackStatus.Paused:
                        activity = "pause";
                        break;
                    case MediaPlaybackStatus.Stopped:
                        activity = "idle";
                        break;
                }
                Player.UpdateRPC(activity, Artist, Title);
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Player.CurrentVolume += e.Delta / 100 * 3;
            VolumeBar.Value += e.Delta / 100 * 3;
            if (Player.Playing && Player.CurrentVolume >= 0 && Player.CurrentVolume <= 1) Player.UpdateSettings();       
        }

        private void ProgressIndicator2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Player.Playing)
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
    }
}
