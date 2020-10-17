using ATL;
using FRESHMusicPlayer.Forms;
using FRESHMusicPlayer.Forms.Playlists;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using LiteDB;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Windows.Media;
using Winforms = System.Windows.Forms;

namespace FRESHMusicPlayer
{
    public enum SelectedMenus
    {
        Tracks,
        Artists,
        Albums,
        Playlists,
        Import,
        Other
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Winforms.Timer progressTimer;
        public static SelectedMenus SelectedMenu = SelectedMenus.Tracks;
        public static Player Player = new Player { CurrentVolume = App.Config.Volume};
        public static NotificationHandler NotificationHandler = new NotificationHandler();
        public static bool MiniPlayerMode = false;
        public static bool AuxilliaryPaneIsOpen = false;
        public static string AuxilliaryPaneUri = "";
        public static EventHandler TabChanged;
        public static LiteDatabase Libraryv2;
        public static Track CurrentTrack;

        public SystemMediaTransportControls Smtc;
        public bool PauseAfterCurrentTrack = false;
        public MainWindow()
        {
            InitializeComponent();
            Player.SongChanged += player_songChanged;
            Player.SongStopped += player_songStopped;
            Player.SongException += player_songException;
            NotificationHandler.NotificationInvalidate += NotificationHandler_NotificationInvalidate;
            progressTimer = new Winforms.Timer
            {
                Interval = 1000
            };
            progressTimer.Tick += ProgressTimer_Tick;
            try
            {
                Libraryv2 = new LiteDatabase(System.IO.Path.Combine(DatabaseHandler.DatabasePath, "database.fdb2"));
            }
            catch
            {
                NotificationHandler.Add(new Notification
                {
                    HeaderText = "Library failed to load",
                    ContentText = "Make sure that you don't have another instance of FMP running!",
                    ButtonText = "Retry",
                    OnButtonClicked = () =>
                    {
                        try
                        {
                            Libraryv2 = new LiteDatabase(System.IO.Path.Combine(DatabaseHandler.DatabasePath, "database.fdb2"));
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    },
                    Type = NotificationType.Failure
                });
            }       
        }
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            UpdateIntegrations();
            ProcessSettings();
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
        public void SetMiniPlayerMode(bool mode)
        { // set is for things that use binding
            if (mode)
            {
                Width = 559;
                Height = 123;
                MainBar.Visibility = Visibility.Collapsed;
                MiniPlayerMode = true;
                Topmost = true;
            }
            else
            {
                Width = 702;
                Height = 512;
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
        public void ShowAuxilliaryPane(string Uri, int width = 235, bool openleft = false)
        {
            if (AuxilliaryPaneUri == Uri)
            {
                HideAuxilliaryPane();
                return;
            }
            if (AuxilliaryPaneIsOpen) HideAuxilliaryPane();

            if (!openleft) DockPanel.SetDock(RightFrame, Dock.Right); else DockPanel.SetDock(RightFrame, Dock.Left);
            RightFrame.Visibility = Visibility.Visible;
            Storyboard sb = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation(0, width, new TimeSpan(0, 0, 0, 0, 100));
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Width"));
            sb.Children.Add(doubleAnimation);
            sb.Begin(RightFrame);
            RightFrame.Source = new Uri(Uri, UriKind.Relative);
            AuxilliaryPaneUri = Uri;
            RightFrame.NavigationService.RemoveBackEntry();
            AuxilliaryPaneIsOpen = true;
        }
        public void HideAuxilliaryPane()
        {
            Storyboard sb = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation(RightFrame.Width, 0, new TimeSpan(0, 0, 0, 0, 100));
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Width"));
            sb.Children.Add(doubleAnimation);
            sb.Begin(RightFrame);
            RightFrame.Visibility = Visibility.Collapsed;
            RightFrame.Source = null;
            AuxilliaryPaneUri = "";
            AuxilliaryPaneIsOpen = false;
        }
        public void ProcessSettings()
        {
            VolumeBar.Value = App.Config.Volume;
            ChangeTabs(App.Config.CurrentMenu);
        }
        #region Tabs
        private void ChangeTabs(SelectedMenus tab)
        {
            SelectedMenu = tab;
            UpdateLibrary();
        }
        private void UpdateLibrary()
        {
            TextBlock tab;
            switch (SelectedMenu)
            {
                case SelectedMenus.Tracks:
                    ContentFrame.Source = new Uri("/Pages/Library/LibraryPage.xaml", UriKind.Relative);
                    ContentFrame.NavigationService.RemoveBackEntry();
                    tab = TracksTab;
                    break;
                case SelectedMenus.Artists:
                    ContentFrame.Source = new Uri("/Pages/Library/LibraryPage.xaml", UriKind.Relative);
                    ContentFrame.NavigationService.RemoveBackEntry();
                    tab = ArtistsTab;
                    break;
                case SelectedMenus.Albums:
                    ContentFrame.Source = new Uri("/Pages/Library/LibraryPage.xaml", UriKind.Relative);
                    ContentFrame.NavigationService.RemoveBackEntry();
                    tab = AlbumsTab;
                    break;
                case SelectedMenus.Playlists:
                    ContentFrame.Source = new Uri("/Pages/Library/LibraryPage.xaml", UriKind.Relative);
                    ContentFrame.NavigationService.RemoveBackEntry();
                    tab = PlaylistsTab;
                    break;
                case SelectedMenus.Import:
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
        private void player_songStopped(object sender, EventArgs e)
        {   
            Title = "FRESHMusicPlayer";
            TitleLabel.Text = ArtistLabel.Text = Properties.Resources.MAINWINDOW_NOTHINGPLAYING;
            progressTimer.Stop();
            SetIntegrations(MediaPlaybackStatus.Stopped);
        }

        private void player_songChanged(object sender, EventArgs e)
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
                CoverArtBox.Source = BitmapFrame.Create(new System.IO.MemoryStream(CurrentTrack.EmbeddedPictures[0].PictureData), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                SetCoverArtVisibility(true);
            }
            progressTimer.Start();
            if (PauseAfterCurrentTrack && !Player.Paused)
            {
                PlayPauseMethod();
                PauseAfterCurrentTrack = false;
            }
        }
        private void player_songException(object sender, PlaybackExceptionEventArgs e)
        {
            NotificationHandler.Add(new Notification
            {
                HeaderText = "A playback error occured",
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
        private void ProgressBar_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) => isDragging = true;
        private void ProgressBar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (Player.Playing) Player.RepositionMusic((int)ProgressBar.Value);
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
        private void VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.CurrentVolume = (float)(VolumeBar.Value / 100);
            if (Player.Playing) Player.UpdateSettings();
        }
        private void ProgressTimer_Tick(object sender, EventArgs e) => ProgressTick();
        private void ProgressTick()
        {
            ProgressIndicator1.Text = Player.CurrentBackend.CurrentTime.ToString(@"mm\:ss");
            if (App.Config.ShowTimeInWindow) Title = $"{Player.CurrentBackend.CurrentTime:mm\\:ss}/{Player.CurrentBackend.TotalTime:mm\\:ss} | FRESHMusicPlayer";
            if (!isDragging) ProgressBar.Value = Player.CurrentBackend.CurrentTime.TotalSeconds;
            Player.AvoidNextQueue = false;
        }
        private void TrackTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowAuxilliaryPane("/Pages/TrackInfoPage.xaml", 235, true);
        }
        private void TrackTitle_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cm = FindResource("MiscContext") as ContextMenu;
            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
        }
        private void TrackContextTagEditor_Click(object sender, RoutedEventArgs e)
        {
            List<string> tracks = new List<string>();
            if (Player.Playing) tracks.Add(Player.FilePath); // if playing, edit the file the user is playing
            else tracks = Player.Queue;
            TagEditor tagEditor = new TagEditor(tracks);
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
                NotificationHandler.Add(new Notification { HeaderText = "Pausing after current track..." });
            }
        }
        #endregion
        #region MenuBar
        private void TracksTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Tracks);
        private void ArtistsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Artists);
        private void AlbumsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Albums);
        private void PlaylistsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Playlists);
        private void ImportTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Import);
        private void SettingsButton_Click(object sender, MouseButtonEventArgs e)
        {
            ShowAuxilliaryPane("/Pages/Settings/SettingsPage.xaml", 335);
        }
        private void SearchButton_Click(object sender, MouseButtonEventArgs e) => ShowAuxilliaryPane("/Pages/Library/SearchPage.xaml", 335);
        private void QueueManagementButton_Click(object sender, MouseButtonEventArgs e) => ShowAuxilliaryPane("/Pages/QueueManagement/QueueManagementPage.xaml", 335);
        private void NotificationButton_Click(object sender, MouseButtonEventArgs e) => ShowAuxilliaryPane("/Pages/NotificationPage.xaml");
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
                    if (AuxilliaryPaneUri != "/Pages/NotificationPage.xaml")
                    {
                        ShowAuxilliaryPane("/Pages/NotificationPage.xaml");
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

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Config.Volume = (int)VolumeBar.Value;
            App.Config.CurrentMenu = SelectedMenu;
            ConfigurationHandler.Write(App.Config);
            Winforms.Application.Exit();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.Q:
                        ShowAuxilliaryPane("/Pages/Settings/SettingsPage.xaml", 335);
                        break;
                    case Key.A:
                        ChangeTabs(SelectedMenus.Tracks);
                        break;
                    case Key.S:
                        ChangeTabs(SelectedMenus.Artists);
                        break;
                    case Key.D:
                        ChangeTabs(SelectedMenus.Albums);
                        break;
                    case Key.F:
                        ChangeTabs(SelectedMenus.Playlists);
                        break;
                    case Key.G:
                        ChangeTabs(SelectedMenus.Import);
                        break;
                    case Key.E:
                        ShowAuxilliaryPane("/Pages/Library/SearchPage.xaml", 335);
                        break;
                    case Key.R:
                        ShowAuxilliaryPane("/Pages/TrackInfoPage.xaml", 235, true);
                        break;
                    case Key.W:
                        ShowAuxilliaryPane("/Pages/QueueManagement/QueueManagementPage.xaml", 335);
                        break;
                    case Key.Space:
                        PlayPauseMethod();
                        break;
                }
            }
            switch (e.Key)
            {
                case Key.OemTilde:
                    NotificationHandler.Add(new Notification
                    {
                        HeaderText = "Debug Key",
                        ContentText = "Deadlock",
                        ButtonText = "Click me!",
                        IsImportant = true,
                        DisplayAsToast = true,
                        Type = NotificationType.Success,
                        OnButtonClicked = () =>
                        {
                            NotificationHandler.Add(new Notification
                            {
                                HeaderText = "Hello world!Loremloremloremlorem"
                            });
                            Player.NextSong();
                            return false;
                        }
                    });
                    break;
                case Key.F5:
                    ContentFrame.Refresh();
                    break;
                case Key.F7:
                    NotificationHandler.Add(new Notification
                    {
                        HeaderText = "No toast test",
                        ContentText = "ok",
                        IsImportant = true,
                        DisplayAsToast = false,
                        Type = NotificationType.Generic
                    });
                    break;
                case Key.F8:
                    DatabaseUtils.Convertv1Tov2();
                    break;
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            await Task.Run(() =>
            {
                string[] tracks = (string[])e.Data.GetData(DataFormats.FileDrop);
                Player.AddQueue(tracks);
            });
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
                Smtc.PlaybackStatus = status;
                var updater = Smtc.DisplayUpdater;
                updater.Type = MediaPlaybackType.Music;
                updater.MusicProperties.Artist = Artist;
                updater.MusicProperties.AlbumArtist = AlbumArtist;
                updater.MusicProperties.Title = Title;
                updater.Update();
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
    }
}
