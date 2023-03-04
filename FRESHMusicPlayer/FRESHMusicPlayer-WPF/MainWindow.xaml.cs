using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.Handlers.Integrations;
using FRESHMusicPlayer.Handlers.Notifications;
using LiteDB;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using WinForms = System.Windows.Forms;

namespace FRESHMusicPlayer
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    // Initialization stuff
    // _Logic    - Main window, player, systemwide logic
    // _Controls - Event handlers and other doo dads
    public partial class MainWindow : Window
    {
        public Player Player;
        public NotificationHandler NotificationHandler = new NotificationHandler();
        public GUILibrary Library;
        public IMetadataProvider CurrentTrack;

#if !BLUEPRINT && !DEBUG
        public const string WindowName = "FRESHMusicPlayer";
#elif BLUEPRINT
        public const string WindowName = "FRESHMusicPlayer Blueprint (possibly unstable!)";
#elif DEBUG
        public const string WindowName = "FRESHMusicPlayer Debug (definitely unstable!)";
#endif
        public PlaytimeTrackingHandler TrackingHandler;
        public bool PauseAfterCurrentTrack = false;

        private FileSystemWatcher watcher = new FileSystemWatcher(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer"));
        public WinForms.Timer ProgressTimer;
        private IPlaybackIntegration smtcIntegration; // might be worth making some kind of manager for these, but i'm lazy so -\_(:/)_/-
        private IPlaybackIntegration discordIntegration;
        private IPlaybackIntegration lastFMIntegration;
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

        public void Initialize(string[] initialFile)
        {
            LoggingHandler.Log("Reading library...");

            var shouldLibraryBeUpgraded = File.Exists(Path.Combine(App.DataFolderLocation, "database.fdb2")) && !File.Exists(Path.Combine(App.DataFolderLocation, "database.fdb3"));

            LiteDatabase library;
            try
            {
#if DEBUG // allow multiple instances of FMP in debug (at the expense of stability with heavy library use)
                library = new LiteDatabase($"Filename=\"{Path.Combine(App.DataFolderLocation, "database.fdb3")}\";Connection=shared");
#else
                library = new LiteDatabase(Path.Combine(App.DataFolderLocation, "database.fdb3"));
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

            if (shouldLibraryBeUpgraded) MigrateLibrary();
            //if (initialFile != null)
            //{
            //    Player.Queue.Add(initialFile);
            //    await Player.PlayAsync();
            //}
        }

        private async void MigrateLibrary()
        {
            var oldLibraryConnection = new LiteDatabase(Path.Combine(App.DataFolderLocation, "database.fdb2"));
            var oldTracks = oldLibraryConnection.GetCollection<OldDatabaseTrack>("tracks").Query().ToArray();
            var oldTrackPaths = oldTracks.Select(x => x.Path).ToArray();

            await Library.ImportAsync(oldTrackPaths);

            var oldPlaylists = oldLibraryConnection.GetCollection<OldDatabasePlaylist>("playlists").Query().ToArray();
            foreach (var playlist in oldPlaylists)
            {
                await Library.CreatePlaylistAsync(playlist.Name, playlist.Name == "Liked" ? true : false);
                foreach (var track in playlist.Tracks)
                {
                    await Library.AddTrackToPlaylistAsync(playlist.Name, track);
                }
            }

            NotificationHandler.Notifications.Add(new Notification 
            { 
                ContentText = "Your library has been upgraded. If anything looks off, please report it at https://github.com/royce551/freshmusicplayer/issues.",
                Type = NotificationType.Success
            });
        }

        private async void Window_SourceInitialized(object sender, EventArgs e)
        {
            UpdateIntegrations();
            ProcessSettings(true);
            //if (!Player.FileLoaded) HandlePersistence();
            var sb = new Storyboard();
            var doubleAnimation = new DoubleAnimation(0f, 1f, TimeSpan.FromSeconds(1));
            doubleAnimation.EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut };
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity"));
            sb.Children.Add(doubleAnimation);
            sb.Begin(ContentFrame);
            sb.Begin(MainBar);
            sb.Begin(ControlsBoxBorder);
            await new UpdateHandler(NotificationHandler).UpdateApp();
            await PerformAutoImport();
        }
        
        private void Window_Closed(object sender, EventArgs e)
        {
            App.Config.Volume = (int)VolumeBar.Value;
            App.Config.CurrentMenu = CurrentTab;
            TrackingHandler?.Close();
            ConfigurationHandler.Write(App.Config);
            Library.Database?.Dispose();
            ProgressTimer.Dispose();
            watcher.Dispose();
            WritePersistence();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowAuxilliaryPane(Pane.SoundSettings, 335, true);
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case System.Windows.Input.MouseButton.XButton1:
                    NavigateBack();
                    break;
                case System.Windows.Input.MouseButton.XButton2:
                    NavigateForward();
                    break;
            }
        }
    }
}
