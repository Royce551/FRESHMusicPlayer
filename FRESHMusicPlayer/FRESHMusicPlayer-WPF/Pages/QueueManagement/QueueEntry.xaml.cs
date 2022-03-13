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
        private string artist;
        public string Artist
        {
            get => artist;
            set
            {
                artist = value;
                UpdateMetadata();
            }
        }
        private string album;
        public string Album
        {
            get => album;
            set
            {
                album = value;
                UpdateMetadata();
            }
        }
        private string title;
        public string Title
        {
            get => title;
            set
            {
                title = value;
                UpdateMetadata();
            }
        }

        public int Index;
        public string Position;
        public int Length;

        private readonly Player player;
        public QueueEntry(string artist, string album, string title, string position, int index, int length, Player player)
        {
            this.player = player;
            InitializeComponent();
            Artist = artist;
            Album = album;
            Title = title;
            
            Index = index;
            Position = position;
            Length = length;
            UpdatePosition();
            UpdateMetadata();
        }

        public void UpdatePosition()
        {
            if (player.Queue.Position == Index + 1) // actual position is index + 1, but i didn't want to convert to int
            {
                TitleLabel.FontWeight = FontWeights.Bold;
                ArtistAlbumLabel.FontWeight = FontWeights.Bold;
                PositionLabel.Text = ">";
            }
            else
            {
                TitleLabel.FontWeight = FontWeights.Regular;
                ArtistAlbumLabel.FontWeight = FontWeights.Regular;
                PositionLabel.Text = Position;
            }
        }

        public void UpdateMetadata()
        {
            ArtistAlbumLabel.Text = $"{Artist} ・ {Album}";
            TitleLabel.Text = Title;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            PlayButton.Visibility = DeleteButton.Visibility = PlayButtonHitbox.Visibility = DeleteButtonHitbox.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            PlayButton.Visibility = DeleteButton.Visibility = PlayButtonHitbox.Visibility = DeleteButtonHitbox.Visibility = Visibility.Collapsed;
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
