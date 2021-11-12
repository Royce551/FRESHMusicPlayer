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

        public const string WindowName = "FRESHMusicPlayer [Blueprint 11 b.10.24.2021; Not stable!]";

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
                library = new LiteDatabase($"Filename=\"{Path.Combine(App.DataFolderLocation, "database.fdb2")}\";Connection=shared");
#elif !DEBUG
                library = new LiteDatabase(Path.Combine(App.DataFolderLocation, "database.fdb2"));
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
    }
}
