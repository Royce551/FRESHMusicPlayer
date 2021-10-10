using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Forms;
using FRESHMusicPlayer.Forms.Playlists;
using FRESHMusicPlayer.Forms.TagEditor;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.Handlers.Integrations;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Pages;
using FRESHMusicPlayer.Pages.Library;
using FRESHMusicPlayer.Pages.Lyrics;
using FRESHMusicPlayer.Utilities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
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
        Fullscreen,
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
        public Menu SelectedMenu = Menu.Tracks;
        public AuxiliaryPane SelectedAuxiliaryPane = AuxiliaryPane.None;
        public Player Player;
        public NotificationHandler NotificationHandler = new NotificationHandler();
        public GUILibrary Library;
        public IMetadataProvider CurrentTrack;

        public const string WindowName = "FRESHMusicPlayer [Blueprint 11 b.10.02.2021; Not stable!]";

        public PlaytimeTrackingHandler TrackingHandler;
        public bool PauseAfterCurrentTrack = false;

        private FileSystemWatcher watcher = new FileSystemWatcher(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer"));
        public WinForms.Timer ProgressTimer;
        private IPlaybackIntegration smtcIntegration; // might be worth making some kind of manager for these, but i'm lazy so -\_(:/)_/-
        private IPlaybackIntegration discordIntegration;
        public MainWindow(Player player, string[] initialFile = null)
        {
            LoggingHandler.Log("Starting main window...");
            Player = player;
            InitializeComponent();
            Player.SongChanged += Player_SongChanged;
            Player.SongLoading += Player_SongLoading;
            Player.SongStopped += Player_SongStopped;
            Player.SongException += Player_SongException;
            NotificationHandler.NotificationInvalidate += NotificationHandler_NotificationInvalidate;
            ProgressTimer = new WinForms.Timer
            {
                Interval = 100
            };
            ProgressTimer.Tick += ProgressTimer_Tick;

            Initialize(initialFile);
        }

        public async void Initialize(string[] initialFile)
        {
            LoggingHandler.Log("Reading library...");

            LiteDatabase library;
            try
            {
#if DEBUG // allow multiple instances of FMP in debug (at the expense of stability with heavy library use)
                library = new LiteDatabase($"Filename=\"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer", "database.fdb2")}\";Connection=shared");
#elif !DEBUG
                library = new LiteDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer", "database.fdb2"));
#endif
                Library = new GUILibrary(library, NotificationHandler, Dispatcher);
            }
            catch (IOException) // library is *probably* being used by another FMP, write initial files, hopefully existing FMP will pick them up
            {
                File.WriteAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer", "instance"), initialFile ?? Array.Empty<string>());
                Application.Current.Shutdown();
                return; // stop initial files from trying to load
            }

            watcher.Filter = "instance";
            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;
            watcher.Changed += (object sender, FileSystemEventArgs args) =>
            {
                Dispatcher.Invoke(async () =>
                {
                    var files = File.ReadAllLines(args.FullPath);
                    if (files.Length != 0) // user wants to play a file
                    {
                        Player.Queue.Clear();
                        Player.Queue.Add(files);
                        await Player.PlayAsync();
                    }
                    else // user might've forgotten fmp is open, let's flash
                    {
                        Activate();
                        Topmost = true;
                        Topmost = false;
                    }
                    File.Delete(args.FullPath);
                });
            };
            LoggingHandler.Log("Ready to go!");

            if (initialFile != null)
            {
                Player.Queue.Add(initialFile);
                await Player.PlayAsync();
            }
        }

#region Controls
        public void PlayPauseMethod()
        {
            if (!Player.FileLoaded) return;
            if (Player.Paused)
            {
                Player.Resume();
                SetIntegrations(PlaybackStatus.Playing);
                ProgressTimer.Start();
            }
            else
            {
                Player.Pause();
                SetIntegrations(PlaybackStatus.Paused);
                ProgressTimer.Stop();
            }
            UpdatePlayButtonState();
        }
        public void StopMethod()
        {
            Player.Queue.Clear();
            Player.Stop();
        }
        public async void NextTrackMethod() => await Player.NextAsync();
        public async void PreviousTrackMethod()
        {
            if (!Player.FileLoaded) return;
            if (Player.CurrentTime.TotalSeconds <= 5) await Player.PreviousAsync();
            else
            {
                Player.CurrentTime = TimeSpan.FromSeconds(0);
                ProgressTimer.Start(); // to resync the progress timer
            }
        }
        public void ShuffleMethod()
        {
            if (Player.Queue.Shuffle)
            {
                Player.Queue.Shuffle = false;
                ShuffleButton.Fill = (Brush)FindResource("PrimaryTextColor");
            }
            else
            {
                Player.Queue.Shuffle = true;
                ShuffleButton.Fill = new LinearGradientBrush(Color.FromRgb(105, 181, 120), Color.FromRgb(51, 139, 193), 0);
            }
        }
        public void RepeatOneMethod()
        {
            if (Player.Queue.RepeatMode == RepeatMode.None)
            {
                Player.Queue.RepeatMode = RepeatMode.RepeatAll;
                RepeatOneButton.Data = (Geometry)FindResource("RepeatAllIcon");
                RepeatOneButton.Fill = new LinearGradientBrush(Color.FromRgb(105, 181, 120), Color.FromRgb(51, 139, 193), 0);
            }
            else if (Player.Queue.RepeatMode == RepeatMode.RepeatAll)
            {
                Player.Queue.RepeatMode = RepeatMode.RepeatOne;
                RepeatOneButton.Data = (Geometry)FindResource("RepeatOneIcon");
                RepeatOneButton.Fill = new LinearGradientBrush(Color.FromRgb(105, 181, 120), Color.FromRgb(51, 139, 193), 0);
            }
            else
            {
                Player.Queue.RepeatMode = RepeatMode.None;
                RepeatOneButton.Data = (Geometry)FindResource("RepeatAllIcon");
                RepeatOneButton.Fill = (Brush)FindResource("PrimaryTextColor");
            }
        }
        public void UpdatePlayButtonState()
        {
            if (!Player.Paused) PlayPauseButton.Data = (Geometry)FindResource("PauseIcon");
            else PlayPauseButton.Data = (Geometry)FindResource("PlayIcon");
            if (PauseAfterCurrentTrack) ProgressIndicator2.Foreground = new SolidColorBrush(Color.FromRgb(212, 70, 63));
            else ProgressIndicator2.Foreground = (Brush)FindResource("SecondaryTextColor");
        }
#endregion

#region Logic

        public void SetCoverArtVisibility(bool mode)
        {
            if (!mode) CoverArtArea.Width = new GridLength(5);       
            else CoverArtArea.Width = new GridLength(75);
        }
        public async void ShowAuxilliaryPane(AuxiliaryPane pane, int width = 235, bool openleft = false)
        {
            LoggingHandler.Log($"Showing pane --> {pane}");
            if (SelectedAuxiliaryPane == pane)
            {
                await HideAuxilliaryPane();
                return;
            }
            if (SelectedAuxiliaryPane != AuxiliaryPane.None) await HideAuxilliaryPane(true);
            switch (pane)
            {
                case AuxiliaryPane.Settings:
                    RightFrame.Content = new SettingsPage(this);
                    break;
                case AuxiliaryPane.QueueManagement:
                    RightFrame.Content = new QueueManagement(this);
                    break;
                case AuxiliaryPane.Search:
                    RightFrame.Content = new SearchPage(this);
                    break;
                case AuxiliaryPane.Notifications:
                    RightFrame.Content = new NotificationPage(this);
                    break;
                case AuxiliaryPane.TrackInfo:
                    RightFrame.Content = new TrackInfoPage(this);
                    break;
                case AuxiliaryPane.Lyrics:
                    RightFrame.Content = new LyricsPage(this);
                    break;
                default:
                    return;
            }
            if (!openleft) DockPanel.SetDock(RightFrame, Dock.Right); else DockPanel.SetDock(RightFrame, Dock.Left);
            RightFrame.Visibility = Visibility.Visible;
            RightFrame.Width = width;

            var sb = InterfaceUtils.GetThicknessAnimation(
                openleft ? new Thickness(width * -1 /*negate*/, 0, 0, 0) : new Thickness(0, 0, width * -1 /*negate*/, 0),
                new Thickness(0),
                TimeSpan.FromMilliseconds(120),
                new PropertyPath(MarginProperty),
                new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3 });

            sb.Begin(RightFrame);

            SelectedAuxiliaryPane = pane;
        }
        public async Task HideAuxilliaryPane(bool animate = true)
        {
            var sb = InterfaceUtils.GetThicknessAnimation(
                new Thickness(0),
                DockPanel.GetDock(RightFrame) == Dock.Left ? new Thickness(RightFrame.Width * -1, 0, 0, 0) : new Thickness(0, 0, RightFrame.Width * -1, 0),
                TimeSpan.FromMilliseconds(120),
                new PropertyPath(MarginProperty),
                new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 3 });

            if (animate) await sb.BeginStoryboardAsync(RightFrame);
            RightFrame.Visibility = Visibility.Collapsed;
            RightFrame.Content = null;
            SelectedAuxiliaryPane = AuxiliaryPane.None;
        }
        public bool IsControlsBoxVisible { get; private set; } = false;
        public async void ShowControlsBox()
        {
            
            var navBarStoryboard = InterfaceUtils.GetThicknessAnimation(
                new Thickness(0, -25, 0, 0),
                new Thickness(0),
                TimeSpan.FromMilliseconds(500),
                new PropertyPath(MarginProperty),
                new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 3 });
            var controlsBoxStoryboard = InterfaceUtils.GetThicknessAnimation(
                new Thickness(0, 0, 0, -84),
                new Thickness(0),
                TimeSpan.FromMilliseconds(500),
                new PropertyPath(MarginProperty),
                new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 3 });
            navBarStoryboard.Begin(MainBar);
            await controlsBoxStoryboard.BeginStoryboardAsync(ControlsBoxBorder);
            IsControlsBoxVisible = true;
        }
        public async void HideControlsBox()
        {
            
            var navBarStoryboard = InterfaceUtils.GetThicknessAnimation(
                new Thickness(0),
                new Thickness(0, -25, 0, 0),
                TimeSpan.FromMilliseconds(500),
                new PropertyPath(MarginProperty),
                new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 3 });
            var controlsBoxStoryboard = InterfaceUtils.GetThicknessAnimation(
                new Thickness(0),
                new Thickness(0, 0, 0, -84),
                TimeSpan.FromMilliseconds(500),
                new PropertyPath(MarginProperty),
                new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 3 });
            navBarStoryboard.Begin(MainBar);
            await controlsBoxStoryboard.BeginStoryboardAsync(ControlsBoxBorder);
            IsControlsBoxVisible = false;
        }

        public void ProcessSettings(bool initialize = false)
        {
            if (initialize)
            {
                VolumeBar.Value = App.Config.Volume;
                ChangeTabs(App.Config.CurrentMenu);
            }
            if (App.Config.PlaybackTracking) TrackingHandler = new PlaytimeTrackingHandler(this);
            else if (TrackingHandler != null)
            {
                TrackingHandler?.Close();
                TrackingHandler = null;
            }
        }
        public async void HandlePersistence()
        {
            var persistenceFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer", "Configuration", "FMP-WPF", "persistence");
            if (File.Exists(persistenceFilePath))
            {
                var fields = File.ReadAllText(persistenceFilePath).Split(';');
                
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
                if (fields[0] != string.Empty)
                {
                    await Player.PlayAsync(fields[0]);
                    Player.RepositionMusic(int.Parse(fields[1]));
                    PlayPauseMethod();
                    ProgressTick();
                }
            }
        }
        public void WritePersistence()
        {
            if (Player.FileLoaded) // TODO: make this less shitty
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
        public void UpdateIntegrations()
        {
            if (Environment.OSVersion.Version.Major >= 10 && App.Config.IntegrateSMTC)
            {
                smtcIntegration = new SMTCIntegration(this);
            }
            else smtcIntegration = null;
            if (App.Config.IntegrateDiscordRPC) discordIntegration = new DiscordIntegration();
            else
            {
                discordIntegration?.Close();
                discordIntegration = null;
            }
        }
        public void SetIntegrations(PlaybackStatus status)
        {
            if (Environment.OSVersion.Version.Major >= 10 && App.Config.IntegrateSMTC)
            {
                smtcIntegration?.Update(CurrentTrack, status);
            }
            if (App.Config.IntegrateDiscordRPC)
            {
                discordIntegration?.Update(CurrentTrack, status);
            }
        }
        public void ChangeTabs(Menu tab, string search = null)
        {
            LoggingHandler.Log($"Changing tabs -> {tab}");

            var previousMenu = SelectedMenu;
            SelectedMenu = tab;
            TextBlock tabLabel;
            switch (SelectedMenu)
            {
                case Menu.Tracks:
                    ContentFrame.Content = new LibraryPage(this, search);
                    tabLabel = TracksTab;
                    break;
                case Menu.Artists:
                    ContentFrame.Content = new LibraryPage(this, search);
                    tabLabel = ArtistsTab;
                    break;
                case Menu.Albums:
                    ContentFrame.Content = new LibraryPage(this, search);
                    tabLabel = AlbumsTab;
                    break;
                case Menu.Playlists:
                    ContentFrame.Content = new LibraryPage(this, search);
                    tabLabel = PlaylistsTab;
                    break;
                case Menu.Import:
                    ContentFrame.Content = new ImportPage(this);
                    tabLabel = ImportTab;
                    break;
                case Menu.Fullscreen:
                    ContentFrame.Content = new FullscreenPage(this, previousMenu);
                    tabLabel = ImportTab;
                    break;
                default:
                    tabLabel = null;
                    break;
            }
            //TabChanged?.Invoke(null, search);
            TracksTab.FontWeight = ArtistsTab.FontWeight = AlbumsTab.FontWeight = PlaylistsTab.FontWeight = ImportTab.FontWeight = FontWeights.Normal;
            tabLabel.FontWeight = FontWeights.Bold;
        }
#endregion

#region Events
#region Player
        private void Player_SongStopped(object sender, EventArgs e)
        {
            Title = WindowName;
            TitleLabel.Text = ArtistLabel.Text = Properties.Resources.MAINWINDOW_NOTHINGPLAYING;
            ProgressTimer.Stop();
            CoverArtBox.Source = null;
            SetIntegrations(PlaybackStatus.Stopped);
            SetCoverArtVisibility(false);

            LoggingHandler.Log("Stopping!");
        }

        private void Player_SongLoading(object sender, EventArgs e) => Mouse.OverrideCursor = Cursors.AppStarting;

        private void Player_SongChanged(object sender, EventArgs e)
        {
            Mouse.OverrideCursor = null;
            CurrentTrack = Player.CurrentBackend.Metadata;
            Title = $"{string.Join(", ", CurrentTrack.Artists)} - {CurrentTrack.Title} | {WindowName}";
            TitleLabel.Text = CurrentTrack.Title;
            ArtistLabel.Text = string.Join(", ", CurrentTrack.Artists) == "" ? Properties.Resources.MAINWINDOW_NOARTIST : string.Join(", ", CurrentTrack.Artists);
            ProgressBar.Maximum = Player.CurrentBackend.TotalTime.TotalSeconds;
            if (Player.CurrentBackend.TotalTime.TotalSeconds != 0) ProgressIndicator2.Text = Player.CurrentBackend.TotalTime.ToString(@"mm\:ss");
            else ProgressIndicator2.Text = "∞";
            SetIntegrations(PlaybackStatus.Playing);
            UpdatePlayButtonState();
            if (CurrentTrack.CoverArt is null)
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
                CoverArtBox.Source = BitmapFrame.Create(new MemoryStream(CurrentTrack.CoverArt), BitmapCreateOptions.None, BitmapCacheOption.None);
                SetCoverArtVisibility(true);
            }
            ProgressTimer.Start();
            if (PauseAfterCurrentTrack && !Player.Paused)
            {
                PlayPauseMethod();
                PauseAfterCurrentTrack = false;
            }

            LoggingHandler.Log("Changing tracks");
        }
        private async void Player_SongException(object sender, PlaybackExceptionEventArgs e)
        {
            NotificationHandler.Add(new Notification
            {
                ContentText = string.Format(Properties.Resources.MAINWINDOW_PLAYBACK_ERROR_DETAILS, e.Details),
                IsImportant = true,
                DisplayAsToast = true,
                Type = NotificationType.Failure
            });
            await Player.NextAsync();
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
                Player.RepositionMusic((int)ProgressBar.Value);
                ProgressTick();
            }
        }
        private void ProgressBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Player.FileLoaded && !isDragging)
            {
                Player.RepositionMusic((int)ProgressBar.Value);
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
            var playlistManagement = new PlaylistManagement(Library, NotificationHandler, SelectedMenu, track);
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
        private void TrackContextArtist_Click(object sender, RoutedEventArgs e) => ChangeTabs(Menu.Artists, CurrentTrack?.Artists[0]);

        private void TrackContextAlbum_Click(object sender, RoutedEventArgs e) => ChangeTabs(Menu.Albums, CurrentTrack?.Album);

        private void TrackContextLyrics_Click(object sender, RoutedEventArgs e) => ShowAuxilliaryPane(AuxiliaryPane.Lyrics, openleft: true);

        private async void TrackContextOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Forms.FMPTextEntryBox(Properties.Resources.IMPORT_MANUALENTRY);
            dialog.ShowDialog();
            if (dialog.OK)
            {
                await Player.PlayAsync(dialog.Response);
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
                case Key.F1:
                    Process.Start("https://royce551.github.io/FRESHMusicPlayer/docs/index.html");
                    break;
                case Key.F11:
                    if (SelectedMenu != Menu.Fullscreen) ChangeTabs(Menu.Fullscreen);
                    else ChangeTabs(Menu.Playlists);
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

        private void ProgressIndicator2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
        private async void Window_SourceInitialized(object sender, EventArgs e)
        {
            UpdateIntegrations();
            ProcessSettings(true);
            if (!Player.FileLoaded) HandlePersistence();
            var sb = new Storyboard();
            var doubleAnimation = new DoubleAnimation(0f, 1f, TimeSpan.FromSeconds(1));
            doubleAnimation.EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut };
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity"));
            sb.Children.Add(doubleAnimation);
            sb.Begin(ContentFrame);
            sb.Begin(MainBar);
            await new UpdateHandler(NotificationHandler).UpdateApp();
            await PerformAutoImport();
        }
        public async Task PerformAutoImport()
        {
            if (App.Config.AutoImportPaths.Count <= 0) return; // not really needed but prevents going through unneeded
                                                               // effort (and showing the notification)
            var notification = new Notification { ContentText = Properties.Resources.NOTIFICATION_SCANNING };
            NotificationHandler.Add(notification);
            var filesToImport = new List<string>();
            var library = Library.Read();
            await Task.Run(() =>
            {
                foreach (var folder in App.Config.AutoImportPaths)
                {
                    var files = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
                        .Where(name => name.EndsWith(".mp3")
                            || name.EndsWith(".wav") || name.EndsWith(".m4a") || name.EndsWith(".ogg")
                            || name.EndsWith(".flac") || name.EndsWith(".aiff")
                            || name.EndsWith(".wma")
                            || name.EndsWith(".aac")).ToArray();
                    foreach (var file in files)
                    {
                        if (!library.Select(x => x.Path).Contains(file))
                            filesToImport.Add(file);
                    }
                }
                Library.Import(filesToImport);
            });
            NotificationHandler.Remove(notification);
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            App.Config.Volume = (int)VolumeBar.Value;
            App.Config.CurrentMenu = SelectedMenu;
            TrackingHandler?.Close();
            ConfigurationHandler.Write(App.Config);
            Library.Database?.Dispose();
            ProgressTimer.Dispose();
            watcher.Dispose();
            WritePersistence();
        }
#endregion

        private void CoverArtBox_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (CurrentTrack != null)
                CoverArtBoxToolTip.Source = BitmapFrame.Create(new MemoryStream(CurrentTrack.CoverArt), BitmapCreateOptions.None, BitmapCacheOption.None);
        }

        private void CoverArtBox_ToolTipClosing(object sender, ToolTipEventArgs e)
        {
            CoverArtBoxToolTip.Source = null;
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
