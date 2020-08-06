using FRESHMusicPlayer;
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

namespace FRESHMusicPlayer_WPF_UI_Test.Pages
{
    /// <summary>
    /// Interaction logic for QueueEntry.xaml
    /// </summary>
    public partial class QueueEntry : UserControl
    {
        public int Index;
        public QueueEntry(string artist, string album, string title, string position, int index)
        {
            InitializeComponent();
            ArtistAlbumLabel.Text = $"{artist} ・ {album}";
            TitleLabel.Text = title;
            PositionLabel.Text = position;
            Index = index;
            if (MainWindow.Player.QueuePosition == index + 1) // actual position is index + 1, but i didn't want to convert to int
            {
                TitleLabel.FontWeight = FontWeights.Bold;
                ArtistAlbumLabel.FontWeight = FontWeights.Bold;
            }
        }
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Storyboard sb = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation(0f, 1f, new TimeSpan(0, 0, 0, 0, 250));
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity"));
            sb.Children.Add(doubleAnimation);
            sb.Begin(PlayButton);
            sb.Begin(DeleteButton);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Storyboard sb = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation(1f, 0f, new TimeSpan(0, 0, 0, 0, 250));
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity"));
            sb.Children.Add(doubleAnimation);
            sb.Begin(PlayButton);
            sb.Begin(DeleteButton);
        }

        private void PlayButtonClick(object sender, MouseButtonEventArgs e)
        {
            MainWindow.Player.QueuePosition = Index;
            MainWindow.Player.PlayMusic();
        }

        private void DeleteButtonClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("sorry, specific track removing hasn't been implemented yet");
        }
    }
}
