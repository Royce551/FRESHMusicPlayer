using FRESHMusicPlayer;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace FRESHMusicPlayer_WPF_UI_Test.Pages.Library
{
    /// <summary>
    /// Interaction logic for SongEntry.xaml
    /// </summary>
    public partial class SongEntry : UserControl
    {
        public string FilePath;
        public string Title;
        public SongEntry(string filePath, string artist, string album, string title)
        {
            InitializeComponent();
            FilePath = filePath;
            ArtistAlbumLabel.Text = $"{artist} ・ {album}";
            TitleLabel.Text = title;
            Title = title;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            /*Storyboard sb = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation(0f, 1f, new TimeSpan(0, 0, 0, 0, 250));
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity"));
            sb.Children.Add(doubleAnimation);
            sb.Begin(PlayButton);
            sb.Begin(QueueButton);
            sb.Begin(DeleteButton);*/ // TODO: replace this with column width animation
            PlayButton.Visibility = QueueButton.Visibility = DeleteButton.Visibility = PlayHitbox.Visibility = QueueHitbox.Visibility = DeleteHitbox.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            /*Storyboard sb = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation(1f, 0f, new TimeSpan(0, 0, 0, 0, 100));
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity"));
            sb.Children.Add(doubleAnimation);
            sb.Begin(PlayButton);
            sb.Begin(QueueButton);
            sb.Begin(DeleteButton);*/
            PlayButton.Visibility = QueueButton.Visibility = DeleteButton.Visibility = PlayHitbox.Visibility = QueueHitbox.Visibility = DeleteHitbox.Visibility = Visibility.Collapsed;
        }

        private void PlayButtonClick(object sender, MouseButtonEventArgs e)
        {
            if (MainWindow.Player.Playing)
            {
                MainWindow.Player.ClearQueue();
                MainWindow.Player.QueuePosition = 0; // temporary fix until FMP Core update
            }
            MainWindow.Player.AddQueue(FilePath);
            MainWindow.Player.PlayMusic();
        }

        private void QueueButtonClick(object sender, MouseButtonEventArgs e) => MainWindow.Player.AddQueue(FilePath);

        private void DeleteButtonClick(object sender, MouseButtonEventArgs e)
        {
            DatabaseHandler.DeleteSong(FilePath);
            ((ListBox)Parent).Items.Remove(this);
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MainWindow.Player.AddQueue(FilePath);
                MainWindow.Player.PlayMusic();
            }
        }
    }
}
