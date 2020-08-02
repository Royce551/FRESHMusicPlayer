using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ATL;
using FRESHMusicPlayer;
using FRESHMusicPlayer.Handlers;

namespace FRESHMusicPlayer_WPF_UI_Test.Pages.Library
{
    /// <summary>
    /// Interaction logic for SongEntry.xaml
    /// </summary>
    public partial class SongEntry : UserControl
    {
        public string FilePath;
        public SongEntry(string filePath, string artist, string album, string title)
        {
            InitializeComponent();
            FilePath = filePath;
            ArtistAlbumLabel.Text = $"{artist} ・ {album}";
            TitleLabel.Text = title;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Storyboard sb = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation(0f, 1f, new TimeSpan(0, 0, 0, 0, 250));
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity"));
            sb.Children.Add(doubleAnimation);
            sb.Begin(PlayButton);
            sb.Begin(QueueButton);
            sb.Begin(DeleteButton);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Storyboard sb = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation(1f, 0f, new TimeSpan(0, 0, 0, 0, 250));
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity"));
            sb.Children.Add(doubleAnimation);
            sb.Begin(PlayButton);
            sb.Begin(QueueButton);
            sb.Begin(DeleteButton);
        }

        private void PlayButtonClick(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Player.AddQueue(FilePath);
            MainWindow.Player.PlayMusic();
        }

        private void QueueButtonClick(object sender, MouseButtonEventArgs e) => MainWindow.Player.AddQueue(FilePath);

        private void DeleteButtonClick(object sender, MouseButtonEventArgs e) => DatabaseHandler.DeleteSong(FilePath);

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
