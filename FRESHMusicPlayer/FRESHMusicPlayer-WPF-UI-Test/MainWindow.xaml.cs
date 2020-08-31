using ATL;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using FRESHMusicPlayer_WPF_UI_Test.Forms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Winforms = System.Windows.Forms;
using Windows.Media;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Interop;
using Windows.Storage.Streams;
using FRESHMusicPlayer.Handlers.Configuration;
using LiteDB;

namespace FRESHMusicPlayer
{
    public enum SelectedMenus
    {
        Tracks,
        Artists,
        Albums,
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
        public static Player Player = new Player();
        public static NotificationHandler NotificationHandler = new NotificationHandler();
        public static List<string> Library = new List<string>();
        public static bool MiniPlayerMode = false;
        public static bool PreventAuxilliaryPaneHiding = false;
        public static EventHandler TabChanged;
        public static LiteDatabase Libraryv2 = new LiteDatabase(System.IO.Path.Combine(DatabaseHandler.DatabasePath, "database.fdb2"));

        public SystemMediaTransportControls Smtc;
        public MainWindow()
        {
            InitializeComponent();
            Player.SongChanged += player_songChanged;
            Player.SongStopped += player_songStopped;
            Player.SongException += player_songException;
            NotificationHandler.NotificationInvalidate += NotificationHandler_NotificationInvalidate;
            progressTimer = new Winforms.Timer  // System.Windows.Forms timer because dispatcher timer seems to have some threading issues?
            {
                Interval = 1000
            };
            progressTimer.Tick += ProgressTimer_Tick;
            Library = DatabaseHandler.ReadSongs();
            ProcessSettings();
        }
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            if (Environment.OSVersion.Version.Major >= 10 && App.Config.IntegrateSMTC)
            {
                var smtcInterop = (WindowsInteropUtils.ISystemMediaTransportControlsInterop)WindowsRuntimeMarshal.GetActivationFactory(typeof(SystemMediaTransportControls));
                Window window = Window.GetWindow(this);
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
            if (Player.Paused)
            {
                Player.ResumeMusic();
                PlayPauseButton.Data = (Geometry)FindResource("PauseIcon");
                Smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
                progressTimer.Start();
            }
            else
            {
                Player.PauseMusic();
                PlayPauseButton.Data = (Geometry)FindResource("PlayIcon");
                Smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
                progressTimer.Stop();
            }
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
                ShuffleButton.Fill = new LinearGradientBrush(Color.FromRgb(51, 139, 193), Color.FromRgb(105, 181, 120), 0);
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
                RepeatOneButton.Fill = new LinearGradientBrush(Color.FromRgb(51, 139, 193), Color.FromRgb(105, 181, 120), 0);
            }
        }
        #endregion
        #region Logic
        public void SetMiniPlayerMode(bool mode)
        {
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
            if (!openleft) DockPanel.SetDock(RightFrame, Dock.Right); else DockPanel.SetDock(RightFrame, Dock.Left);
            RightFrame.Visibility = Visibility.Visible;
            Storyboard sb = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation(0, width, new TimeSpan(0, 0, 0, 0, 100));
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Width"));
            sb.Children.Add(doubleAnimation);
            sb.Begin(RightFrame);
            RightFrame.Source = new Uri(Uri, UriKind.Relative);
            RightFrame.NavigationService.RemoveBackEntry();
        }
        public void HideAuxilliaryPane()
        {
            if (!PreventAuxilliaryPaneHiding)
            {
                Storyboard sb = new Storyboard();
                DoubleAnimation doubleAnimation = new DoubleAnimation(RightFrame.Width, 0, new TimeSpan(0, 0, 0, 0, 100));
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Width"));
                sb.Children.Add(doubleAnimation);
                sb.Begin(RightFrame);
                RightFrame.Visibility = Visibility.Collapsed;
                RightFrame.Source = null;
            }
            else
            {
                NotificationHandler.Add(new NotificationBox(new NotificationInfo("Hold up!", "The pane still needs your attention. Finish what you're doing first.", false, true)));
            }
        }
        public void ProcessSettings()
        {
            DockPanel.SetDock(ControlsBoxBorder, App.Config.ControlBoxPosition);
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
                    ContentFrame.Source = new Uri(@"\Pages\Library\LibraryPage.xaml", UriKind.Relative);
                    ContentFrame.NavigationService.RemoveBackEntry();
                    tab = TracksTab;
                    break;
                case SelectedMenus.Artists:
                    ContentFrame.Source = new Uri(@"\Pages\Library\LibraryPage.xaml", UriKind.Relative);
                    ContentFrame.NavigationService.RemoveBackEntry();
                    tab = ArtistsTab;
                    break;
                case SelectedMenus.Albums:
                    ContentFrame.Source = new Uri(@"\Pages\Library\LibraryPage.xaml", UriKind.Relative);
                    ContentFrame.NavigationService.RemoveBackEntry();
                    tab = AlbumsTab;
                    break;
                case SelectedMenus.Import:
                    ContentFrame.Source = new Uri(@"\Pages\ImportPage.xaml", UriKind.Relative);
                    tab = ImportTab;
                    break;
                default:
                    tab = null;
                    break;
            }
            TabChanged?.Invoke(null, EventArgs.Empty);
            TracksTab.FontWeight = ArtistsTab.FontWeight = AlbumsTab.FontWeight = ImportTab.FontWeight = FontWeights.Normal;
            tab.FontWeight = FontWeights.Bold;
        }
        #endregion


        #endregion

        #region Events
        #region player
        private void player_songStopped(object sender, EventArgs e)
        {
            Title = "FRESHMusicPlayer 8 Development";
            TitleLabel.Text = ArtistLabel.Text = "Nothing Playing";
            progressTimer.Stop();
            Smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
        }

        private void player_songChanged(object sender, EventArgs e)
        {
            ATL.Track track = new ATL.Track(Player.FilePath);
            Title = $"{track.Artist} - {track.Title} | FRESHMusicPlayer 8 Development";
            TitleLabel.Text = track.Title;
            ArtistLabel.Text = track.Artist == "" ? FRESHMusicPlayer_WPF_UI_Test.Properties.Resources.MAINWINDOW_NOARTIST : track.Artist;
            ProgressBar.Maximum = Player.CurrentBackend.TotalTime.TotalSeconds;
            if (Player.CurrentBackend.TotalTime.TotalSeconds != 0) ProgressIndicator2.Text = Player.CurrentBackend.TotalTime.ToString(@"mm\:ss");
            else ProgressIndicator2.Text = "∞";
            if (Environment.OSVersion.Version.Major >= 10 && App.Config.IntegrateSMTC)
            {
                Smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
                var updater = Smtc.DisplayUpdater;
                updater.Type = MediaPlaybackType.Music;
                updater.MusicProperties.Artist = track.Artist;
                updater.MusicProperties.AlbumArtist = track.AlbumArtist;
                updater.MusicProperties.Title = track.Title;
                updater.Update();
            }
            if (track.EmbeddedPictures.Count == 0)
            {
                CoverArtBox.Source = null;
                SetCoverArtVisibility(false);
            }
            else
            {
                CoverArtBox.Source = BitmapFrame.Create(new System.IO.MemoryStream(track.EmbeddedPictures[0].PictureData), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                SetCoverArtVisibility(true);
            }

            progressTimer.Start();
        }
        private void player_songException(object sender, PlaybackExceptionEventArgs e)
        {
            NotificationHandler.Add(new NotificationBox(new NotificationInfo("A playback error occured", String.Format(FRESHMusicPlayer_WPF_UI_Test.Properties.Resources.MAINWINDOW_PLAYBACK_ERROR_DETAILS, e.Details), true, true)));
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
        private void ProgressBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Player.Playing) Player.RepositionMusic((int)ProgressBar.Value);
        }
        private void VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.CurrentVolume = (float)(VolumeBar.Value / 100);
            if (Player.Playing)
            {
                Player.UpdateSettings(); 
            }
        }
        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            ProgressIndicator1.Text = Player.CurrentBackend.CurrentTime.ToString(@"mm\:ss");
            if (App.Config.ShowTimeInWindow) Title = $"{Player.CurrentBackend.CurrentTime:mm\\:ss}/{Player.CurrentBackend.TotalTime:mm\\:ss} | FRESHMusicPlayer 8 Development";
            ProgressBar.Value = Player.CurrentBackend.CurrentTime.TotalSeconds;
            Player.AvoidNextQueue = false;
        }
        #endregion
        #region MenuBar
        private void TracksTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Tracks);
        private void ArtistsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Artists);
        private void AlbumsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Albums);
        private void ImportTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Import);
        private void SettingsButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (RightFrame.Visibility == Visibility.Visible) HideAuxilliaryPane();
            else ShowAuxilliaryPane("/Pages/Settings/SettingsPage.xaml", 335);
        }
        private void SearchButton_Click(object sender, MouseButtonEventArgs e)
        {
            ContentFrame.Source = new Uri("/Pages/Library/SearchPage.xaml", UriKind.Relative);
            TabChanged?.Invoke(null, EventArgs.Empty);
        }
        #endregion
        private void NotificationHandler_NotificationInvalidate(object sender, EventArgs e)
        {
            foreach (NotificationBox box in NotificationHandler.Notifications)
            {
                if (box.DisplayAsToast && RightFrame.Visibility != Visibility.Visible) ShowAuxilliaryPane("Pages\\NotificationPage.xaml"); // TODO: replace this with proper toast implementation
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
            Winforms.Application.Exit();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Multiselect = true
            };
            if (fileDialog.ShowDialog() == true)
            {
                foreach (string path in fileDialog.FileNames) Player.AddQueue(path);
                Player.PlayMusic();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1:
                    if (RightFrame.Visibility == Visibility.Collapsed) ShowAuxilliaryPane("/Pages/QueueManagement/QueueManagementPage.xaml", 335); else HideAuxilliaryPane();
                    e.Handled = true;
                    break;
                case Key.F2:
                    if (MiniPlayerMode) SetMiniPlayerMode(false); else SetMiniPlayerMode(true);
                    e.Handled = true;
                    break;
                case Key.F3:
                    if (RightFrame.Visibility == Visibility.Collapsed)
                    {
                        ShowAuxilliaryPane("/Pages/TrackInfoPage.xaml", 235, true);
                    }
                    else
                    {
                        HideAuxilliaryPane();
                    }
                    e.Handled = true;
                    break;
                case Key.F4:
                    TagEditor tagEditor = new TagEditor(Player.Queue);
                    tagEditor.Show();
                    e.Handled = true;
                    break;
                case Key.F6:
                    if (RightFrame.Visibility == Visibility.Collapsed) ShowAuxilliaryPane("/Pages/NotificationPage.xaml"); else HideAuxilliaryPane();
                    e.Handled = true;
                    break;
                case Key.OemTilde:
                    NotificationHandler.Add(new NotificationBox(new NotificationInfo("Debug key", "You just pressed the debug key! You may or may not see cool stuff happening.", false, true)));
                    int a = 5;
                    int b = 0;
                    int c = a / b;
                    ((App)System.Windows.Application.Current).ChangeSkin(Skin.Light);
                    e.Handled = true;
                    break;
                case Key.F5:
                    Library.Clear();
                    Library = DatabaseHandler.ReadSongs();
                    ContentFrame.Refresh();
                    e.Handled = true;
                    break;
                case Key.F7:
                    if (Player.Shuffle == true) Player.Shuffle = false; else Player.Shuffle = true;
                    NotificationHandler.Add(new NotificationBox(new NotificationInfo("Debug key", $"Shuffle: {Player.Shuffle}", false, true)));
                    e.Handled = true;
                    break;
                case Key.F8:
                    NotificationHandler.Add(new NotificationBox(new NotificationInfo("Debug key", $"Put stuff in configuration file", false, true)));
                    App.Config.Language = "vi";
                    App.Config.UpdatesLastChecked = DateTime.Now;
                    App.Config.AccentColorHex = "fdfsfdsgsgs";
                    ConfigurationHandler.Write(App.Config);
                    e.Handled = true;
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

        private void SettingsButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
