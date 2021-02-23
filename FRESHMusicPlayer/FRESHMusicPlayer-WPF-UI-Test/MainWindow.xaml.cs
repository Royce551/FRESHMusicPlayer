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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Media;
using WinForms = System.Windows.Forms;

namespace FRESHMusicPlayer
{
    public enum Menu
    {
        Tracks,
        Artists,
        Albums,
        Playlists,
        Import,
        Other
    }
    public enum AuxiliaryPane
    {
        None,
        Settings,
        QueueManagement,
        Search,
        Notifications, 
        TrackInfo,
        Lyrics
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WinForms.Timer progressTimer;
        public static Menu SelectedMenu = Menu.Tracks;
        public static AuxiliaryPane SelectedAuxiliaryPane = AuxiliaryPane.None;
        public static Player Player = new Player { CurrentVolume = App.Config.Volume};
        public static NotificationHandler NotificationHandler = new NotificationHandler();
        public static bool MiniPlayerMode = false;
        public static EventHandler<string> TabChanged;
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
            progressTimer = new WinForms.Timer
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
                            Libraryv2 = new LiteDatabase(Path.Combine(DatabaseHandler.DatabasePath, "database.fdb2"));
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
                App.Config.CurrentMenu = Menu.Import;
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
            var sb = new Storyboard();
            var doubleAnimation = new DoubleAnimation(0f, 1f, TimeSpan.FromSeconds(1));
            doubleAnimation.EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut };
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity"));
            sb.Children.Add(doubleAnimation);
            sb.Begin(ContentFrame);
            sb.Begin(MainBar);
            await UpdateHandler.UpdateApp();
            if (!Player.Playing) HandlePersistence();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            App.Config.Volume = (int)VolumeBar.Value;
            App.Config.CurrentMenu = SelectedMenu;
            TrackingHandler?.Close();
            ConfigurationHandler.Write(App.Config);
            Libraryv2?.Dispose();
            WritePersistence();
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
                SetIntegrations(MediaPlaybackStatus.Playing);
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
        { 
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
        public void ShowAuxilliaryPane(AuxiliaryPane pane, int width = 235, bool openleft = false)
        {
            if (SelectedAuxiliaryPane == pane)
            {
                HideAuxilliaryPane();
                return;
            }
            if (SelectedAuxiliaryPane != AuxiliaryPane.None) HideAuxilliaryPane(false);
            string uri;
            switch (pane)
            {
                case AuxiliaryPane.Settings:
                    uri = "/Pages/Settings/SettingsPage.xaml";
                    break;
                case AuxiliaryPane.QueueManagement:
                    uri = "/Pages/QueueManagement/QueueManagementPage.xaml";
                    break;
                case AuxiliaryPane.Search:
                    uri = "/Pages/Library/SearchPage.xaml";
                    break;
                case AuxiliaryPane.Notifications:
                    uri = "/Pages/NotificationPage.xaml";
                    break;
                case AuxiliaryPane.TrackInfo:
                    uri = "/Pages/TrackInfoPage.xaml";
                    break;
                case AuxiliaryPane.Lyrics:
                    uri = "/Pages/Lyrics/LyricsPage.xaml";
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
            SelectedAuxiliaryPane = AuxiliaryPane.None;
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
        public void HandlePersistence()
        {
            var persistenceFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer", "Configuration", "FMP-WPF", "persistence");
            if (File.Exists(persistenceFilePath))
            {
                var fields = File.ReadAllText(persistenceFilePath).Split(';');
                if (fields[0] != string.Empty)
                {
                    Player.AddQueue(fields[0]);
                    Player.PlayMusic();
                    Player.RepositionMusic(int.Parse(fields[1]));
                    PlayPauseMethod();
                    ProgressTick();
                }
                
                var top = double.Parse(fields[2]);
                var left = double.Parse(fields[3]);
                var height = double.Parse(fields[4]);
                var width = double.Parse(fields[5]);
                var rect = new System.Drawing.Rectangle((int)left, (int)top, (int)width, (int)height);
                if (WinForms.Screen.AllScreens.Any(y => y.WorkingArea.IntersectsWith(rect)))
                {
                    Top = top;
                    Left = left;
                    Height = height;
                    Width = width;
                }
            }
        }
        public void WritePersistence()
        {
            if (Player.Playing) // TODO: make this less shitty
            {
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer", "Configuration", "FMP-WPF", "persistence"),
                    $"{Player.FilePath};{(int)Player.CurrentBackend.CurrentTime.TotalSeconds};{Top};{Left};{Height};{Width}");
            }
            else
            {
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer", "Configuration", "FMP-WPF", "persistence"),
                    $";;{Top};{Left};{Height};{Width}");
            }
        }
        public MemoryStream GetCoverArtFromDirectory()
        {
            if (File.Exists(Player.FilePath))
            {
                var currentDirectory = Path.GetDirectoryName(Player.FilePath);
                foreach (var file in Directory.EnumerateFiles(currentDirectory))
                {
                    if (Path.GetFileNameWithoutExtension(file).ToUpper() == "COVER" ||
                        Path.GetFileNameWithoutExtension(file).ToUpper() == "ARTWORK" ||
                        Path.GetFileNameWithoutExtension(file).ToUpper() == "FRONT" ||
                        Path.GetFileNameWithoutExtension(file).ToUpper() == "BACK" ||
                        Path.GetFileNameWithoutExtension(file).ToUpper() == Player.FilePath)
                    {
                        if (Path.GetExtension(file) == ".png" || Path.GetExtension(file) == ".jpg" || Path.GetExtension(file) == ".jpeg")
                        {
                            return new MemoryStream(File.ReadAllBytes(file));
                        }
                    }
                }
            }
            return null;
        }
        #region Tabs
        private void ChangeTabs(Menu tab, string search = null)
        {
            SelectedMenu = tab;
            TextBlock tabLabel;
            switch (SelectedMenu)
            {
                case Menu.Tracks:
                    ContentFrame.Source = new Uri("/Pages/Library/LibraryPage.xaml", UriKind.Relative);
                    tabLabel = TracksTab;
                    break;
                case Menu.Artists:
                    ContentFrame.Source = new Uri("/Pages/Library/LibraryPage.xaml", UriKind.Relative);
                    tabLabel = ArtistsTab;
                    break;
                case Menu.Albums:
                    ContentFrame.Source = new Uri("/Pages/Library/LibraryPage.xaml", UriKind.Relative);
                    tabLabel = AlbumsTab;
                    break;
                case Menu.Playlists:
                    ContentFrame.Source = new Uri("/Pages/Library/LibraryPage.xaml", UriKind.Relative);
                    tabLabel = PlaylistsTab;
                    break;
                case Menu.Import:
                    ContentFrame.Source = new Uri("/Pages/ImportPage.xaml", UriKind.Relative);
                    tabLabel = ImportTab;
                    break;
                default:
                    tabLabel = null;
                    break;
            }
            ContentFrame.NavigationService.RemoveBackEntry();
            TabChanged?.Invoke(null, search);
            TracksTab.FontWeight = ArtistsTab.FontWeight = AlbumsTab.FontWeight = PlaylistsTab.FontWeight = ImportTab.FontWeight = FontWeights.Normal;
            tabLabel.FontWeight = FontWeights.Bold;
        }
        #endregion


        #endregion

        #region Events
        #region Player
        private void Player_SongStopped(object sender, EventArgs e)
        {   
            Title = "FRESHMusicPlayer";
            TitleLabel.Text = ArtistLabel.Text = Properties.Resources.MAINWINDOW_NOTHINGPLAYING;
            progressTimer.Stop();
            CoverArtBox.Source = null;
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
            SetIntegrations(MediaPlaybackStatus.Playing);
            UpdatePlayButtonState();
            if (CurrentTrack.EmbeddedPictures.Count == 0)
            {
                var file = GetCoverArtFromDirectory();
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
                CoverArtBox.Source = BitmapFrame.Create(new MemoryStream(CurrentTrack.EmbeddedPictures[0].PictureData), BitmapCreateOptions.None, BitmapCacheOption.None);
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
            MessageBox.Show(Environment.CurrentDirectory);
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
        private void NextTrackButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => NextTrackMethod();
        private bool isDragging = false;
        private void ProgressBar_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDragging = true;
            progressTimer.Stop();
        }
        private void ProgressBar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (Player.Playing)
            {
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
        private void TrackTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => ShowAuxilliaryPane(AuxiliaryPane.TrackInfo, 235, true);
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
        private void TrackContextArtist_Click(object sender, RoutedEventArgs e) => ChangeTabs(Menu.Artists, CurrentTrack?.Artist);

        private void TrackContextAlbum_Click(object sender, RoutedEventArgs e) => ChangeTabs(Menu.Albums, CurrentTrack?.Album);

        private void TrackContextLyrics_Click(object sender, RoutedEventArgs e) => ShowAuxilliaryPane(AuxiliaryPane.Lyrics, openleft: true);

        private void TrackContextOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Forms.FMPTextEntryBox(Properties.Resources.IMPORT_MANUALENTRY);
            dialog.ShowDialog();
            if (dialog.OK)
            {
                Player.AddQueue(dialog.Response);
                Player.PlayMusic();
            }
        }
        #endregion
        #region MenuBar
        private void TracksTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(Menu.Tracks);
        private void ArtistsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(Menu.Artists);
        private void AlbumsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(Menu.Albums);
        private void PlaylistsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(Menu.Playlists);
        private void ImportTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(Menu.Import);
        private void SettingsButton_Click(object sender, MouseButtonEventArgs e) => ShowAuxilliaryPane(AuxiliaryPane.Settings, 335);
        private void SearchButton_Click(object sender, MouseButtonEventArgs e) => ShowAuxilliaryPane(AuxiliaryPane.Search, 335);
        private void QueueManagementButton_Click(object sender, MouseButtonEventArgs e) => ShowAuxilliaryPane(AuxiliaryPane.QueueManagement, 335);
        private void NotificationButton_Click(object sender, MouseButtonEventArgs e) => ShowAuxilliaryPane(AuxiliaryPane.Notifications);
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
                    if (SelectedAuxiliaryPane != AuxiliaryPane.Notifications)
                    {
                        ShowAuxilliaryPane(AuxiliaryPane.Notifications);
                        break;
                    }
                }
            }
        }
        #endregion

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.OriginalSource is TextBox || e.OriginalSource is ListBoxItem) || Keyboard.IsKeyDown(Key.LeftCtrl))
            switch (e.Key)
            {
                case Key.Q:
                    ShowAuxilliaryPane(AuxiliaryPane.Settings, 335);
                    break;
                case Key.A:
                    ChangeTabs(Menu.Tracks);
                    break;
                case Key.S:
                    ChangeTabs(Menu.Artists);
                    break;
                case Key.D:
                    ChangeTabs(Menu.Albums);
                    break;
                case Key.F:
                    ChangeTabs(Menu.Playlists);
                    break;
                case Key.G:
                    ChangeTabs(Menu.Import);
                    break;
                case Key.E:
                    ShowAuxilliaryPane(AuxiliaryPane.Search, 335);
                    break;
                case Key.R:
                    ShowAuxilliaryPane(AuxiliaryPane.TrackInfo, 235, true);
                    break;
                case Key.W:
                    ShowAuxilliaryPane(AuxiliaryPane.QueueManagement, 335);
                    break;
                case Key.Space:
                    PlayPauseMethod();
                    break;
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
                    throw new Exception("Exception for debugging");
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
            Player.ClearQueue();
            InterfaceUtils.DoDragDrop((string[])e.Data.GetData(DataFormats.FileDrop), import: false);
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
        public void SetIntegrations(MediaPlaybackStatus status)
        {
            if (Environment.OSVersion.Version.Major >= 10 && App.Config.IntegrateSMTC)
            {
                try
                {
                    Smtc.PlaybackStatus = status;
                    var updater = Smtc.DisplayUpdater;
                    updater.Type = MediaPlaybackType.Music;
                    updater.MusicProperties.Artist = CurrentTrack.Artist;
                    updater.MusicProperties.AlbumArtist = CurrentTrack.AlbumArtist;
                    updater.MusicProperties.Title = CurrentTrack.Title;
                    updater.Update();
                }
                catch
                {
                    // TODO: HACK - ignored; the way i'm detecting windows 10 currently does not work
                }
            }
            if (App.Config.IntegrateDiscordRPC)
            {
                string activity = string.Empty;
                string state = string.Empty;
                switch (status)
                {
                    case MediaPlaybackStatus.Playing:
                        activity = "play";
                        state = $"by {CurrentTrack.Artist}";
                        break;
                    case MediaPlaybackStatus.Paused:
                        activity = "pause";
                        state = "Paused";
                        break;
                    case MediaPlaybackStatus.Stopped:
                        activity = "idle";
                        state = "Idle";
                        break;
                }
                Player.UpdateRPC(activity, state, CurrentTrack.Title);
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
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
