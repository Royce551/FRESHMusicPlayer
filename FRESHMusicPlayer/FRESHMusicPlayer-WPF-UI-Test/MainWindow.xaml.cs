using ATL;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer;
using FRESHMusicPlayer.Utilities;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Winforms = System.Windows.Forms;
using System.Windows.Navigation;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using FRESHMusicPlayer.Handlers.Notifications;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Documents;
using System.Collections.Generic;
using FRESHMusicPlayer_WPF_UI_Test;
using FRESHMusicPlayer_WPF_UI_Test.Properties;

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
        // TODO: i dunno how to pass this to the pages, if there is a way to i can avoid making this static
        public static Player Player = new Player();
        public static NotificationHandler NotificationHandler = new NotificationHandler();
        public static List<string> Library = new List<string>();
        public static bool MiniPlayerMode = false;
        public MainWindow()
        {
            InitializeComponent();
            Player.SongChanged += player_songChanged;
            Player.SongStopped += player_songStopped;
            Player.SongException += player_songException;
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
            }
            else
            {
                Width = 702;
                Height = 512;
                MainBar.Visibility = Visibility.Visible;
                MiniPlayerMode = false;
            }
        }
        public void ShowAuxilliaryPane(string Uri)
        {
            RightFrame.Visibility = Visibility.Visible;
            if (FindResource("SlideIn") is Storyboard sb) BeginStoryboard(sb);
            RightFrame.Source = new Uri(Uri, UriKind.Relative);
            RightFrame.NavigationService.RemoveBackEntry();
        }
        public void HideAuxilliaryPane()
        {
            if (FindResource("SlideOut") is Storyboard sb) BeginStoryboard(sb);
            RightFrame.Visibility = Visibility.Collapsed;
            RightFrame.Source = null;
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
                    RightFrame.NavigationService.RemoveBackEntry();
                    tab = TracksTab;
                    break;
                case SelectedMenus.Artists:
                    ContentFrame.Source = new Uri(@"\Pages\Library\LibraryPage.xaml", UriKind.Relative);
                    RightFrame.NavigationService.RemoveBackEntry();
                    tab = ArtistsTab;
                    break;
                case SelectedMenus.Albums:
                    ContentFrame.Source = new Uri(@"\Pages\Library\LibraryPage.xaml", UriKind.Relative);
                    RightFrame.NavigationService.RemoveBackEntry();
                    tab = AlbumsTab;
                    break;
                case SelectedMenus.Import:
                    ContentFrame.Source = new Uri(@"\Pages\ImportPage.xaml", UriKind.Relative);
                    tab = ImportTab;
                    break;
                default:
                    throw new InvalidOperationException("you are idoit");
            }
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
            TitleLabel.Text = track.Title == "" ? "No Title" : track.Title;
            ArtistLabel.Text = track.Artist == "" ? "No Artist" : track.Artist;
            ProgressBar.Maximum = Player.CurrentBackend.TotalTime.TotalSeconds;
            ProgressIndicator2.Text = Player.CurrentBackend.TotalTime.ToString(@"mm\:ss");

            if (track.EmbeddedPictures.Count == 0)
            {
                CoverArtBox.Source = null;
                CoverArtArea.Width = new GridLength(5);
            }
            else
            {
                CoverArtBox.Source = BitmapFrame.Create(new System.IO.MemoryStream(track.EmbeddedPictures[0].PictureData), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                CoverArtArea.Width = new GridLength(75);
            }

            progressTimer.Start();
        }
        private void player_songException(object sender, PlaybackExceptionEventArgs e)
        {
            NotificationHandler.Add(new NotificationBox(new NotificationInfo("A playback error occured", $"{e.Details}\nWe'll skip to the next track for you", true, true)));
            Player.NextSong();
        }
        #endregion
        #region ControlsBox
        private void MoreButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => MoreMethod();
        private void PlayPauseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => PlayPauseMethod();
        private void StopButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => StopMethod();
        private void NextTrackButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => NextTrackMethod();
        private void ProgressBar_MouseUp(object sender, MouseButtonEventArgs e) => Player.RepositionMusic((int)ProgressBar.Value);
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
        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.CurrentBackend.CurrentTime = TimeSpan.FromSeconds(ProgressBar.Value);
        }
        #endregion
        #region MenuBar
        private void TracksTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Tracks);

        private void ArtistsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Artists);

        private void AlbumsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Albums);

        private void ImportTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Import);
        #endregion
        #endregion




        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            //player.AddQueue(FilePathBox.Text);
            Player.PlayMusic();                         
        }
       

        private void Window_Closed(object sender, EventArgs e)
        {
            Winforms.Application.Exit();
        }


      

        //private void QueueButton_Click(object sender, RoutedEventArgs e) => player.AddQueue(FilePathBox.Text);

        

        

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
                    /*if (App.CurrentSkin == Skin.Light) App.CurrentSkin = Skin.Dark; else App.CurrentSkin = Skin.Light;
                    foreach (ResourceDictionary dict in Resources.MergedDictionaries)
                    {

                        if (dict is SkinResourceDictionary skinDict)
                            skinDict.UpdateSource();
                        else
                            dict.Source = dict.Source;
                    }
                    MainWindow newWindow = new MainWindow();
                    Application.Current.MainWindow = newWindow;
                    newWindow.Show();
                    this.Close();
                    e.Handled = true;*/
                    break;
                case Key.W:
                    if (MiniPlayerMode) SetMiniPlayerMode(false); else SetMiniPlayerMode(true);
                    e.Handled = true;
                    break;
                case Key.Left:
                    if (ContentFrame.CanGoBack) ContentFrame.GoBack();
                    e.Handled = true;
                    break;
                case Key.Right:
                    if (ContentFrame.CanGoForward) ContentFrame.GoForward();
                    e.Handled = true;
                    break;
                case Key.E:
                    if (RightFrame.Visibility == Visibility.Collapsed) ShowAuxilliaryPane("/Pages/NotificationPage.xaml"); else HideAuxilliaryPane();
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
