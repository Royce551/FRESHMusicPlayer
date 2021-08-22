using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FRESHMusicPlayer.Pages
{
    /// <summary>
    /// Interaction logic for QueueEntry.xaml
    /// </summary>
    public partial class QueueEntry : UserControl
    {
        public int Index;

        private readonly Player player;
        public QueueEntry(string artist, string album, string title, string position, int index, Player player)
        {
            this.player = player;
            InitializeComponent();
            ArtistAlbumLabel.Text = $"{artist} ・ {album}";
            TitleLabel.Text = title;
            PositionLabel.Text = position;
            Index = index;
            if (player.Queue.Position == index + 1) // actual position is index + 1, but i didn't want to convert to int
            {
                TitleLabel.FontWeight = FontWeights.Bold;
                ArtistAlbumLabel.FontWeight = FontWeights.Bold;
                PositionLabel.Text = ">";
            }
        }
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            PlayButton.Visibility = DeleteButton.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            PlayButton.Visibility = DeleteButton.Visibility = Visibility.Collapsed;
        }

        private async void PlayButtonClick(object sender, MouseButtonEventArgs e)
        {
            player.Queue.Position = Index;
            await player.PlayAsync();
        }

        private void DeleteButtonClick(object sender, MouseButtonEventArgs e) => player.Queue.Remove(Index);

        private async void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                player.Queue.Position = Index;
                await player.PlayAsync();
            }
        }
    }
}
