using ATL;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace FRESHMusicPlayer.Forms.TagEditor.Integrations
{
    /// <summary>
    /// Interaction logic for ReleaseIntegrationPage.xaml
    /// </summary>
    public partial class ReleaseIntegrationPage : Window
    {
        public Track TrackToSave { get; set; }
        public bool OK { get; set; } = false;

        private readonly TagEditorRelease release;
        private readonly Track track;
        private readonly string filePath;

        private int currentFilePosition = 0;
        public ReleaseIntegrationPage(TagEditorRelease release, Track track, string filePath)
        {
            this.release = release;
            this.track = track;
            this.filePath = filePath;
            currentFilePosition = track.TrackNumber;
            InitializeComponent();
            if (currentFilePosition >= release.Tracks.Count) currentFilePosition = 0;
            InitFields();
            ValidatePosition();
        }
        public void InitFields()
        {
            ArtistBox.Text = release.Artist;
            AlbumBox.Text = Title = release.Name;
            GenreBox.Text = release.Genre;
            YearBox.Text = release.Year.ToString();
            Link.Text = release.URL;

            PopulateLists();
        }
        public void PopulateLists()
        {
            int i = 1;
            IntegrationItemBox.Items.Clear();
            FileItemBox.Items.Clear();
            foreach (var x in release.Tracks)
            {
                IntegrationItemBox.Items.Add($"{x.TrackNumber} - {x.Title}");
                if (i == currentFilePosition) FileItemBox.Items.Add($"{track.TrackNumber} - {Path.GetFileName(filePath)}");
                else FileItemBox.Items.Add(string.Empty);
                i++;
            }
        }
        public void ValidatePosition()
        {
            if (currentFilePosition <= 1)
            {
                UpButton.IsEnabled = false;
                DownButton.IsEnabled = true;
            }
            else if (currentFilePosition >= release.Tracks.Count)
            {
                UpButton.IsEnabled = true;
                DownButton.IsEnabled = false;
            }
            else
            {
                UpButton.IsEnabled = true;
                DownButton.IsEnabled = true;
            }
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            TrackToSave = new Track
            {
                Artist = release.Artist,
                Album = release.Name,
                Year = release.Year,
                Genre = release.Genre,
                Title = release.Tracks[currentFilePosition - 1].Title,
                TrackNumber = release.Tracks[currentFilePosition - 1].TrackNumber
            };
            OK = true;
            Close();
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            currentFilePosition--;
            ValidatePosition();
            PopulateLists();
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            currentFilePosition++;
            ValidatePosition();
            PopulateLists();
        }

        private void Link_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => Process.Start(release.URL);
    }
}
