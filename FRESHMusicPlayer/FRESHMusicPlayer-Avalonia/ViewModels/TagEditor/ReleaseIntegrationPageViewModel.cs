using ATL;
using FRESHMusicPlayer.Handlers.DatabaseIntegrations;
using FRESHMusicPlayer.Utilities;
using FRESHMusicPlayer.Views.TagEditor;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace FRESHMusicPlayer.ViewModels.TagEditor
{
    public class ReleaseIntegrationPageViewModel : ViewModelBase
    {
        public ReleaseIntegrationPage Window { get; set; }

        public TagEditorRelease Release { get; set; }

        public Track TrackToSave { get; set; }

        public Track InitialTrack { get; set; }

        private string artist;
        public string Artist
        {
            get => artist;
            set => this.RaiseAndSetIfChanged(ref artist, value);
        }
        private bool artistShouldBeVisible = true;
        public bool ArtistShouldBeVisible
        {
            get => artistShouldBeVisible;
            set => this.RaiseAndSetIfChanged(ref artistShouldBeVisible, value);
        }

        private string year;
        public string Year
        {
            get => year;
            set => this.RaiseAndSetIfChanged(ref year, value);
        }
        private bool yearShouldBeVisible = true;
        public bool YearShouldBeVisible
        {
            get => yearShouldBeVisible;
            set => this.RaiseAndSetIfChanged(ref yearShouldBeVisible, value);
        }

        private string genre;
        public string Genre
        {
            get => genre;
            set => this.RaiseAndSetIfChanged(ref genre, value);
        }
        private bool genreShouldBeVisible = true;
        public bool GenreShouldBeVisible
        {
            get => genreShouldBeVisible;
            set => this.RaiseAndSetIfChanged(ref genreShouldBeVisible, value);
        }

        private string album;
        public string Album
        {
            get => album;
            set => this.RaiseAndSetIfChanged(ref album, value);
        }
        private bool albumShouldBeVisible = true;
        public bool AlbumShouldBeVisible
        {
            get => albumShouldBeVisible;
            set => this.RaiseAndSetIfChanged(ref albumShouldBeVisible, value);
        }

        public ObservableCollection<string> Entries { get; set; } = new();
        private int selectedItem;
        public int SelectedItem
        {
            get => selectedItem;
            set => this.RaiseAndSetIfChanged(ref selectedItem, value);
        }

        public void Initialize()
        {
            if (string.IsNullOrEmpty(Release.Artist)) ArtistShouldBeVisible = false;
            else Artist = Release.Artist;

            if (string.IsNullOrEmpty(Release.Name)) AlbumShouldBeVisible = false;
            else Album = Release.Name;

            if (string.IsNullOrEmpty(Release.Genre)) GenreShouldBeVisible = false;
            else Genre = Release.Genre;

            if (Release.Year == 0) YearShouldBeVisible = false;
            else Year = Release.Year.ToString();

            foreach (var x in Release.Tracks)
            {
                Entries.Add($"{x.TrackNumber} - {x.Title}");
            }
        }

        public void OpenURLCommand() => InterfaceUtils.OpenURL(Release.URL);

        public void OKCommand()
        {
            if (SelectedItem == -1) return;
            TrackToSave = new Track
            {
                Artist = Release.Artist,
                Album = Release.Name,
                Year = Release.Year,
                Genre = Release.Genre,
                Title = Release.Tracks[SelectedItem].Title,
                TrackNumber = Release.Tracks[SelectedItem].TrackNumber
            };
            Window.OK = true;
            Window.Close();
        }
    }
}
