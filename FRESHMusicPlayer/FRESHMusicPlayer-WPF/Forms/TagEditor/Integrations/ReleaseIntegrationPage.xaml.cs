using ATL;
using FRESHMusicPlayer.Utilities;
using System.Diagnostics;
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
        public ReleaseIntegrationPage(TagEditorRelease release, Track track)
        {
            this.release = release;
            this.track = track;
            InitializeComponent();
            InitFields();
        }
        public void InitFields()
        {
            InterfaceUtils.SetField(ArtistBox, ArtistLabel, release.Artist);
            InterfaceUtils.SetField(AlbumBox, AlbumLabel, release.Name);
            InterfaceUtils.SetField(GenreBox, GenreLabel, release.Genre);
            InterfaceUtils.SetField(YearBox, YearLabel, release.Year.ToString() == "0" ? null : release.Year.ToString());
            Link.Text = release.URL;
            Title = release.Name;
            IntegrationItemBox.SelectedIndex = --track.TrackNumber;
            PopulateLists();
        }
        public void PopulateLists()
        {
            int i = 1;
            IntegrationItemBox.Items.Clear();
            foreach (var x in release.Tracks)
            {
                IntegrationItemBox.Items.Add($"{x.TrackNumber} - {x.Title}");
                i++;
            }
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (IntegrationItemBox.SelectedIndex == -1) return;
            TrackToSave = new Track
            {
                Artist = release.Artist,
                Album = release.Name,
                Year = release.Year,
                Genre = release.Genre,
                Title = release.Tracks[IntegrationItemBox.SelectedIndex].Title,
                TrackNumber = release.Tracks[IntegrationItemBox.SelectedIndex].TrackNumber
            };
            OK = true;
            Close();
        }

        private void Link_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => Process.Start(release.URL);
    }
}
