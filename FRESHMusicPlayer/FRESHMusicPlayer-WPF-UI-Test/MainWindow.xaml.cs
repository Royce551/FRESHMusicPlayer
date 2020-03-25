using ATL;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Utilities;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Winforms = System.Windows.Forms;
namespace FRESHMusicPlayer.Forms.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WPFUserInterface : Window
    {
        Winforms.Timer progressTimer;
        public WPFUserInterface()
        {
            InitializeComponent();
            Player.songChanged += Player_songChanged;
            Player.songStopped += Player_songStopped;
            Player.songException += Player_songException;
            progressTimer = new Winforms.Timer  // System.Windows.Forms timer because dispatcher timer seems to have some threading issues?
            {
                Interval = 1000
            };
            progressTimer.Tick += ProgressTimer_Tick;
        }

        
        #region Controls
        public void PlayPauseMethod()
        {
            if (Player.paused)
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
        public void StopMethod() => Player.StopMusic();
        public void NextTrackMethod() => Player.NextSong();
        public void MoreMethod()
        {

        }
        #endregion
        #region Logic
        #endregion
        #region Library
        #endregion
        #region Settings
        #endregion
        #region Events
        #endregion
        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            ProgressIndicator1.Text = NumberUtils.Format((int)Player.audioFile.CurrentTime.TotalSeconds);
            ProgressBar.Value = Player.audioFile.CurrentTime.TotalSeconds;
            Player.avoidnextqueue = false;
        }

        private void Player_songStopped(object sender, EventArgs e)
        {
            Title = "FRESHMusicPlayer WPF Test";
            TitleLabel.Text = ArtistLabel.Text = "Nothing Playing";
            progressTimer.Stop();
        }

        private void Player_songChanged(object sender, EventArgs e)
        {
            Track track = new Track(Player.filePath);
            Title = $"{track.Artist} - {track.Title} | FRESHMusicPlayer WPF Test";
            TitleLabel.Text = track.Title;
            ArtistLabel.Text = track.Artist;
            ProgressBar.Maximum = Player.audioFile.TotalTime.TotalSeconds;
            ProgressIndicator2.Text = NumberUtils.Format((int)Player.audioFile.TotalTime.TotalSeconds);

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

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Player.AddQueue(FilePathBox.Text);
            Player.PlayMusic();                         
            Player.currentvolume = .3f;
            Player.UpdateSettings();
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Player.StopMusic();
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.audioFile.CurrentTime = TimeSpan.FromSeconds(ProgressBar.Value);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Winforms.Application.Exit();
        }


        private void MoreButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => MoreMethod();

        private void PlayPauseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => PlayPauseMethod();

        private void StopButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => StopMethod();

        private void NextTrackButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => NextTrackMethod();

        private void QueueButton_Click(object sender, RoutedEventArgs e) => Player.AddQueue(FilePathBox.Text);

        private void ProgressBar_MouseUp(object sender, MouseButtonEventArgs e) => Player.RepositionMusic((int)ProgressBar.Value);

        private void Player_songException(object sender, PlaybackExceptionEventArgs e) => MessageBox.Show($"A playback error has occured. \"{e.Details}\"");

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                Player.AddQueue(fileDialog.FileName);
                Player.PlayMusic();
            }
        }
    }
}
