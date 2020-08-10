using ATL;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer_WPF_UI_Test.Forms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Winforms = System.Windows.Forms;

namespace FRESHMusicPlayer
{
    public enum SelectedMenus
    {
        Tracks,
        Artists,
        Albums,
        Import
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
        }

        


        #region Controls
        public void PlayPauseMethod()
        {
            if (Player.Paused)
            {
                Player.ResumeMusic();
                PlayPauseButton.Data = (Geometry)FindResource("PauseIcon");
            }
            else
            {
                Player.PauseMusic();
                PlayPauseButton.Data = (Geometry)FindResource("PlayIcon");
            }
        }
        public void StopMethod()
        {
            Player.ClearQueue();
            Player.StopMusic();
        }
        public void NextTrackMethod() => Player.NextSong();
        public void MoreMethod()
        {

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
                    throw new InvalidOperationException("you are idoit");
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
        }

        private void player_songChanged(object sender, EventArgs e)
        {
            Track track = new Track(Player.FilePath);
            Title = $"{track.Artist} - {track.Title} | FRESHMusicPlayer 8 Development";
            TitleLabel.Text = track.Title;
            ArtistLabel.Text = track.Artist == "" ? FRESHMusicPlayer_WPF_UI_Test.Properties.Resources.MAINWINDOW_NOARTIST : track.Artist;
            ProgressBar.Maximum = Player.CurrentBackend.TotalTime.TotalSeconds;
            ProgressIndicator2.Text = Player.CurrentBackend.TotalTime.ToString(@"mm\:ss");

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
        private void MoreButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => MoreMethod();
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
            ProgressBar.Value = Player.CurrentBackend.CurrentTime.TotalSeconds;
            Player.AvoidNextQueue = false;
        }
        #endregion
        #region MenuBar
        private void TracksTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Tracks);

        private void ArtistsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Artists);

        private void AlbumsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Albums);

        private void ImportTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Import);
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
                case Key.Q:
                    if (RightFrame.Visibility == Visibility.Collapsed) ShowAuxilliaryPane("/Pages/QueueManagement/QueueManagementPage.xaml", 335); else HideAuxilliaryPane();
                    e.Handled = true;
                    break;
                case Key.W:
                    if (MiniPlayerMode) SetMiniPlayerMode(false); else SetMiniPlayerMode(true);
                    e.Handled = true;
                    break;
                case Key.Left:
                    if (RightFrame.Visibility == Visibility.Collapsed)
                    {
                        ShowAuxilliaryPane("/Pages/TrackInfoPage.xaml", 235, true);
                        SetCoverArtVisibility(false);
                    }
                    else
                    {
                        HideAuxilliaryPane();
                        SetCoverArtVisibility(true);
                    }
                    e.Handled = true;
                    break;
                case Key.Right:
                    TagEditor tagEditor = new TagEditor(Player.Queue);
                    tagEditor.Show();
                    e.Handled = true;
                    break;
                case Key.E:
                    if (RightFrame.Visibility == Visibility.Collapsed) ShowAuxilliaryPane("/Pages/NotificationPage.xaml"); else HideAuxilliaryPane();
                    e.Handled = true;
                    break;
                case Key.F5:
                    Library.Clear();
                    Library = DatabaseHandler.ReadSongs();
                    ContentFrame.Refresh();
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

    }
}
