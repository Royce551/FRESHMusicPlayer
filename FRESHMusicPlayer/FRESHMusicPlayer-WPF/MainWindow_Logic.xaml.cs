using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Integrations;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Pages;
using FRESHMusicPlayer.Pages.Library;
using FRESHMusicPlayer.Pages.Lyrics;
using FRESHMusicPlayer.Pages.Playlists;
using FRESHMusicPlayer.Utilities;
using FRESHMusicPlayer.Utilities.ColorQuantization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Drawing = System.Drawing;

namespace FRESHMusicPlayer
{
    public enum Tab
    {
        Tracks,
        Artists,
        Albums,
        Playlists,
        Import,
        Fullscreen,
        Other
    }
    public enum Pane
    {
        None,
        Settings,
        QueueManagement,
        Search,
        Notifications,
        TrackInfo,
        Lyrics,
        SoundSettings,
        PlaylistManagement
    }

    // Code for the "shell" parts of the main window, player, and systemwide logic
    public partial class MainWindow
    {
        // Player
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
                await AnimateSeekBarAsync(0);
                ProgressTimer.Start(); // to resync the progress timer

            }
        }

        public void UpdateControlsBoxColors()
        {
            if (Player.Queue.RepeatMode == RepeatMode.RepeatAll)
            {
                RepeatOneButton.Data = (Geometry)FindResource("RepeatAllIcon");
                RepeatOneButton.Fill = (Brush)FindResource("AccentGradientColor");
            }
            else if (Player.Queue.RepeatMode == RepeatMode.RepeatOne)
            {
                RepeatOneButton.Data = (Geometry)FindResource("RepeatOneIcon");
                RepeatOneButton.Fill = (Brush)FindResource("AccentGradientColor");
            }
            else
            {
                RepeatOneButton.Data = (Geometry)FindResource("RepeatAllIcon");
                RepeatOneButton.Fill = (Brush)FindResource("PrimaryTextColor");
            }

            if (PauseAfterCurrentTrack) ProgressIndicator2.Foreground = new SolidColorBrush(Color.FromRgb(212, 70, 63));
            else ProgressIndicator2.Foreground = (Brush)FindResource("SecondaryTextColor");

            if (!Player.Queue.Shuffle) ShuffleButton.Fill = (Brush)FindResource("PrimaryTextColor");
            else ShuffleButton.Fill = ShuffleButton.Fill = (Brush)FindResource("AccentGradientColor");
        }

        public void ShuffleMethod()
        {
            if (Player.Queue.Shuffle) Player.Queue.Shuffle = false;
            else Player.Queue.Shuffle = true;
            UpdateControlsBoxColors();
        }
        public void RepeatOneMethod()
        {
            if (Player.Queue.RepeatMode == RepeatMode.None) Player.Queue.RepeatMode = RepeatMode.RepeatAll;
            else if (Player.Queue.RepeatMode == RepeatMode.RepeatAll) Player.Queue.RepeatMode = RepeatMode.RepeatOne;
            else Player.Queue.RepeatMode = RepeatMode.None;
            UpdateControlsBoxColors();
        }
        public void UpdatePlayButtonState()
        {
            if (!Player.Paused) PlayPauseButton.Data = (Geometry)FindResource("PauseIcon");
            else PlayPauseButton.Data = (Geometry)FindResource("PlayIcon");

            UpdateControlsBoxColors();
        }
        private void Player_SongStopped(object sender, PlaybackStoppedEventArgs e)
        {
            ProgressTimer.Stop();
            
            if (e.IsEndOfPlayback)
            {
                Title = WindowName;
                TitleLabel.Text = ArtistLabel.Text = Properties.Resources.MAINWINDOW_NOTHINGPLAYING;
                CoverArtBox.Source = null;
                SetIntegrations(PlaybackStatus.Stopped);
                SetCoverArtVisibility(false);
            }
            else
            {
                Title = $"Loading... | {WindowName}";
                TitleLabel.Text = "Loading...";
                ArtistLabel.Text = "Loading...";
                CoverArtBox.Source = null;
                SetIntegrations(PlaybackStatus.Changing);
            }

            LoggingHandler.Log("Stopping!");
        }

        private async Task AnimateSeekBarAsync(double toValue)
        {
            var sb = new Storyboard();
            sb.FillBehavior = FillBehavior.Stop;
            var doubleAnimation = new DoubleAnimation(toValue, TimeSpan.FromMilliseconds(500));
            doubleAnimation.EasingFunction = new ExponentialEase { Exponent = 8, EasingMode = EasingMode.EaseInOut };
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(Slider.ValueProperty));
            sb.Children.Add(doubleAnimation);
            await sb.BeginStoryboardAsync(ProgressBar);
        }

        private bool InFullscreen => WindowStyle != WindowStyle.SingleBorderWindow;

        private void Player_SongLoading(object sender, EventArgs e)
        {
            if (!InFullscreen) Mouse.OverrideCursor = Cursors.AppStarting;
        }

        private async void Player_SongChanged(object sender, EventArgs e)
        {
            if (!InFullscreen) Mouse.OverrideCursor = null;
            CurrentTrack = Player.Metadata;
            Title = $"{CurrentTrack.Title} • {string.Join(", ", CurrentTrack.Artists)} - {WindowName}";
            TitleLabel.Text = CurrentTrack.Title;
            ArtistLabel.Text = string.Join(", ", CurrentTrack.Artists) == "" ? Properties.Resources.MAINWINDOW_NOARTIST : string.Join(", ", CurrentTrack.Artists);
            
            if (Player.CurrentBackend.TotalTime.TotalSeconds != 0) ProgressIndicator2.Text = Player.CurrentBackend.TotalTime.ToString(@"mm\:ss");
            else ProgressIndicator2.Text = "∞";
            SetIntegrations(PlaybackStatus.Playing);
            UpdateEqualizer();
            UpdatePlayButtonState();
            if (CurrentTrack.CoverArt is null)
            {
                CoverArtBox.Source = null;
                SetCoverArtVisibility(false);
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

            HandleAccentCoverArt();

            LoggingHandler.Log("Changing tracks!");

            await AnimateSeekBarAsync(0);
            ProgressBar.Maximum = Player.CurrentBackend.TotalTime.TotalSeconds;
        }
        private async void Player_SongException(object sender, PlaybackExceptionEventArgs e)
        {
            if (!InFullscreen) Mouse.OverrideCursor = null;
            var message = new StringBuilder();
            foreach (var problem in e.Problems)
            {
                message.AppendLine($"{problem.Key}: {problem.Value}");
            }

            NotificationHandler.Add(new Notification
            {
                ContentText = string.Format(Properties.Resources.MAINWINDOW_PLAYBACK_ERROR_DETAILS, message.ToString()),
                IsImportant = true,
                DisplayAsToast = true,
                Type = NotificationType.Failure
            });
            await Player.NextAsync();
        }

        public void UpdateEqualizer()
        {
            if (Player.CurrentBackend is ISupportEqualization equalizableBackend)
            {
                Dispatcher.Invoke(() =>
                {
                    equalizableBackend.EqualizerBands = new List<EqualizerBand>()
                {
                    new EqualizerBand { Bandwidth = 0.8f, Frequency = 60, Gain = App.Config.EQBand1 },
                    new EqualizerBand { Bandwidth = 0.8f, Frequency = 150, Gain = App.Config.EQBand2},
                    new EqualizerBand { Bandwidth = 0.8f, Frequency = 400, Gain = App.Config.EQBand3 },
                    new EqualizerBand { Bandwidth = 0.8f, Frequency = 1000, Gain = App.Config.EQBand4 },
                    new EqualizerBand { Bandwidth = 0.8f, Frequency = 2400, Gain = App.Config.EQBand5 },
                    new EqualizerBand { Bandwidth = 0.8f, Frequency = 15000, Gain = App.Config.EQBand6 },
                };
                    equalizableBackend.UpdateEqualizer();
                }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
        }

        public void HandleAccentCoverArt()
        {
            if (!Player.FileLoaded || Player.Metadata.CoverArt is null)
            {
                (Application.Current as App).ChangeAccentColor(Handlers.Configuration.AccentColor.System);
                return;
            }
            if (App.Config.AccentColor != Handlers.Configuration.AccentColor.CoverArt) return;

            using (var bitmap = new Drawing.Bitmap(new MemoryStream(Player.Metadata.CoverArt)))
            {
                using (var resized = new Drawing.Bitmap(bitmap, 25, 25))
                {
                    var colors = new List<Drawing.Color>(resized.Width * resized.Height);
                    for (int x = 0; x < resized.Width; x++)
                    {
                        for (int y = 0; y < resized.Height; y++)
                        {
                            colors.Add(resized.GetPixel(x, y));
                        }
                    }

                    var clustering = new KMeansClusteringCalculator();
                    var dominantColors = clustering.Calculate(3, colors, 5.0d);

                    (Application.Current as App).ApplyAccentColor(dominantColors[1].R, dominantColors[1].G, dominantColors[1].B, dominantColors[2].R, dominantColors[2].G, dominantColors[2].B);
                }
            }
        }


        // Shell

        public Tab CurrentTab = Tab.Tracks;
        public Pane CurrentPane = Pane.None;

        public bool IsControlsBoxVisible { get; private set; } = false;

        public void SetCoverArtVisibility(bool mode)
        {
            if (!mode) 
                CoverArtArea.Width = new GridLength(5);
            else CoverArtArea.Width = new GridLength(75);
        }
        public async void ShowAuxilliaryPane(Pane pane, int width = 235, bool openleft = false, string args = null)
        {
            LoggingHandler.Log($"Showing pane --> {pane}");

            UserControl GetPageForPane(Pane panex)
            {
                switch (panex)
                {
                    case Pane.Settings:
                        return new SettingsPage(this);
                    case Pane.QueueManagement:
                        return new QueueManagement(this);
                    case Pane.Search:
                        return new SearchPage(this);
                    case Pane.Notifications:
                        return new NotificationPage(this);
                    case Pane.TrackInfo:
                        return new TrackInfoPage(this);
                    case Pane.Lyrics:
                        return new LyricsPage(this);
                    case Pane.SoundSettings:
                        return new SoundSettingsPage(this);
                    case Pane.PlaylistManagement:
                        return new PlaylistManagement(Library, NotificationHandler, CurrentTab, args);
                    default:
                        return null;
                }
            }

            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                new Window 
                { 
                    Content = GetPageForPane(pane), 
                    Width = 600, 
                    Height = 500, 
                    Topmost = true, 
                    ShowInTaskbar = false, 
                    Owner = this, 
                    WindowStartupLocation = WindowStartupLocation.CenterOwner 
                }.Show();
                return;
            }

            if (CurrentPane == pane)
            {
                await HideAuxilliaryPane();
                return;
            }
            if (CurrentPane != Pane.None) await HideAuxilliaryPane(true);
            
            if (!openleft) DockPanel.SetDock(RightFrame, Dock.Right); else DockPanel.SetDock(RightFrame, Dock.Left);
            RightFrame.Visibility = Visibility.Visible;
            RightFrame.Width = width;
            RightFrame.Content = GetPageForPane(pane);

            var sb = InterfaceUtils.GetThicknessAnimation(
                openleft ? new Thickness(width * -1 /*negate*/, 0, 0, 0) : new Thickness(0, 0, width * -1 /*negate*/, 0),
                new Thickness(0),
                TimeSpan.FromMilliseconds(120),
                new PropertyPath(MarginProperty),
                new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3 });

            sb.Begin(RightFrame);

            RightFrame.Focus();
            CurrentPane = pane;
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
            CurrentPane = Pane.None;
        }
        public async void ShowControlsBox()
        {
            var navBarStoryboard = InterfaceUtils.GetThicknessAnimation(
                new Thickness(0, -25, 0, 0),
                new Thickness(0),
                TimeSpan.FromMilliseconds(400),
                new PropertyPath(MarginProperty),
                new ExponentialEase { EasingMode = EasingMode.EaseInOut, Exponent = 1 });
            var controlsBoxStoryboard = InterfaceUtils.GetThicknessAnimation(
                new Thickness(0, 0, 0, -84),
                new Thickness(0),
                TimeSpan.FromMilliseconds(400),
                new PropertyPath(MarginProperty),
                new ExponentialEase { EasingMode = EasingMode.EaseInOut, Exponent = 1 });
            navBarStoryboard.Begin(MainBar);
            await controlsBoxStoryboard.BeginStoryboardAsync(ControlsBoxBorder);

            ControlsBox.Focusable = true;
            MainBar.Focusable = true;

            IsControlsBoxVisible = true;
        }
        public async void HideControlsBox()
        {
            var navBarStoryboard = InterfaceUtils.GetThicknessAnimation(
                new Thickness(0),
                new Thickness(0, -25, 0, 0),
                TimeSpan.FromMilliseconds(400),
                new PropertyPath(MarginProperty),
                new ExponentialEase { EasingMode = EasingMode.EaseInOut, Exponent = 3 });
            var controlsBoxStoryboard = InterfaceUtils.GetThicknessAnimation(
                new Thickness(0),
                new Thickness(0, 0, 0, -84),
                TimeSpan.FromMilliseconds(400),
                new PropertyPath(MarginProperty),
                new ExponentialEase { EasingMode = EasingMode.EaseInOut, Exponent = 3 });
            navBarStoryboard.Begin(MainBar);
            await controlsBoxStoryboard.BeginStoryboardAsync(ControlsBoxBorder);

            ControlsBox.Focusable = false;
            MainBar.Focusable = false;

            IsControlsBoxVisible = false;
        }

        private List<(Tab tab, string search)> backLog = new List<(Tab tab, string search)>();
        private List<(Tab tab, string search)> forwardLog = new List<(Tab tab, string search)>();

        public void ChangeTabs(Tab tab, string search = null, bool isNavigating = false)
        {
            LoggingHandler.Log($"Changing tabs -> {tab}");

            var previousMenu = CurrentTab;
            CurrentTab = tab;
            TextBlock tabLabel;
            switch (CurrentTab)
            {
                case Tab.Tracks:
                    ContentFrame.Content = new LibraryPage(this, search);
                    tabLabel = TracksTab;
                    break;
                case Tab.Artists:
                    ContentFrame.Content = new LibraryPage(this, search);
                    tabLabel = ArtistsTab;
                    break;
                case Tab.Albums:
                    ContentFrame.Content = new LibraryPage(this, search);
                    tabLabel = AlbumsTab;
                    break;
                case Tab.Playlists:
                    ContentFrame.Content = new LibraryPage(this, search);
                    tabLabel = PlaylistsTab;
                    break;
                case Tab.Import:
                    ContentFrame.Content = new ImportPage(this);
                    tabLabel = ImportTab;
                    break;
                case Tab.Fullscreen:
                    ContentFrame.Content = new FullscreenPage(this, previousMenu);
                    tabLabel = ImportTab;
                    break;
                default:
                    tabLabel = null;
                    break;
            }

            if (!isNavigating)
            {
                backLog.Add((tab, search));
                forwardLog.Clear();
            }
            //if (backLog.Count > 5) backLog.Remove(backLog.Last());
           
            TracksTab.FontWeight = ArtistsTab.FontWeight = AlbumsTab.FontWeight = PlaylistsTab.FontWeight = ImportTab.FontWeight = FontWeights.Normal;
            tabLabel.FontWeight = FontWeights.Bold;
            ContentFrame.Focus();
        }

        public void NavigateBack()
        {
            if (backLog.Count <= 1) return;

            var entry = backLog[backLog.Count - 2];
            forwardLog.Add(backLog[backLog.Count - 1]);
            ChangeTabs(entry.tab, entry.search, true);
            backLog.RemoveAt(backLog.Count - 1);

            

        }
        public void NavigateForward()
        {
            if (forwardLog.Count <= 0) return;

            var entry = forwardLog[forwardLog.Count - 1];
            ChangeTabs(entry.tab, entry.search, true);
            forwardLog.RemoveAt(forwardLog.Count - 1);
        }

        // Systemwide logic

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

            var version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            if (version != App.Config.LastRecordedVersion && App.Config.LastRecordedVersion != null)
                NotificationHandler.Add(new Notification
                {
                    ContentText = string.Format(Properties.Resources.NOTIFICATION_UPTODATE, version),
                    ButtonText = Properties.Resources.NOTIFICATION_UPTODATE_CHANGELOG,
                    Type = NotificationType.Success,
                    OnButtonClicked = () => { Process.Start("https://github.com/royce551/freshmusicplayer/releases/latest"); return true; }
                });

           App.Config.LastRecordedVersion = version;
        }

        public void WritePersistence()
        {
            if (Player.FileLoaded) // TODO: make this less shitty
            {
                File.WriteAllText(Path.Combine(App.DataFolderLocation, "Configuration", "FMP-WPF", "persistence"),
                    $"{Player.FilePath};{(int)Player.CurrentBackend.CurrentTime.TotalSeconds};{Top};{Left};{Height};{Width}");
            }
            else
            {
                File.WriteAllText(Path.Combine(App.DataFolderLocation, "Configuration", "FMP-WPF", "persistence"),
                    $";;{Top};{Left};{Height};{Width}");
            }
        }

        public void UpdateIntegrations()
        {
            if (App.Config.IntegrateLastFM) lastFMIntegration = new LastFMIntegration(this);
            else lastFMIntegration = null;
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
            if (App.Config.IntegrateLastFM) lastFMIntegration?.Update(CurrentTrack, status);
            if (Environment.OSVersion.Version.Major >= 10 && App.Config.IntegrateSMTC)
            {
                smtcIntegration?.Update(CurrentTrack, status);
            }
            if (App.Config.IntegrateDiscordRPC)
            {
                discordIntegration?.Update(CurrentTrack, status);
            }
        }
        public async Task PerformAutoImport()
        {
            if (App.Config.AutoImportPaths.Count <= 0) return; // not really needed but prevents going through unneeded
                                                               // effort (and showing the notification)
            var notification = new Notification { ContentText = Properties.Resources.NOTIFICATION_SCANNING };
            NotificationHandler.Add(notification);
            var filesToImport = new List<string>();
            var library = Library.GetAllTracks();
            await Task.Run(async () =>
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
                await Library.ImportAsync(filesToImport);
            });
            NotificationHandler.Remove(notification);
        }
    }
}
