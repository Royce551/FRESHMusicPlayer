using ATL;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Forms.TagEditor.Integrations;
using FRESHMusicPlayer.Handlers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FRESHMusicPlayer.Forms.TagEditor
{
    /// <summary>
    /// Interaction logic for TagEditor.xaml
    /// </summary>
    public partial class TagEditor : Window
    {
        public List<string> FilePaths = new List<string>();
        
        private readonly List<string> filePathsToSaveInBackground = new List<string>();
        private readonly List<string> Displayfilepaths = new List<string>();
        private readonly List<PictureInfo> CoverArts = new List<PictureInfo>();
        private bool unsavedChanges = false;
        private readonly HttpClient httpClient = new HttpClient();

        private readonly Player player;
        private readonly GUILibrary library;
        public TagEditor(List<string> filePaths, Player player = null, GUILibrary library = null)
        {
            this.player = player ?? new Player();
            this.library = library;
            InitializeComponent();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("FRESHMusicPlayer/12.0.0 (https://github.com/Royce551/FRESHMusicPlayer)");
            FilePaths = filePaths;
            player.SongChanged += Player_SongChanged;
            InitFields();
        }

        public void InitFields()
        {
            unsavedChanges = true; // hack in order to make the TextChanged handler not go off   
            CoverArtSelector.Items.Clear();
            int iterations = 1;           
            foreach (string path in FilePaths)
            {
                var track = new Track(path);
                ArtistBox.Text = track.Artist;
                TitleBox.Text = track.Title;
                AlbumBox.Text = track.Album;
                GenreBox.Text = track.Genre;
                YearBox.Text = track.Year.ToString();

                AlbumArtistBox.Text = track.AlbumArtist;
                ComposerBox.Text = track.Composer;
                TrackNumBox.Text = track.TrackNumber.ToString();
                DiscNumBox.Text = track.DiscNumber.ToString();

                UntimedLyricsBox.Text = track.Lyrics.UnsynchronizedLyrics;
                Displayfilepaths.Add(Path.GetFileName(path));
                CoverArts.AddRange(track.EmbeddedPictures);
                
                int i = 1;
                if (track.EmbeddedPictures.Count != 0)
                {
                    foreach (var cover in track.EmbeddedPictures)
                    {
                        CoverArtSelector.Items.Add(i.ToString());
                        i++;
                    }
                }
                else
                {
                    ImportCoverButton.IsEnabled = false;
                    RemoveCoverButton.IsEnabled = false;
                }
                iterations++;
            }
            if (CoverArts.Count != 0)
            {
                CoverArtSelector.SelectedIndex = 0;
                ChangeCoverArt();
            }
            if (iterations <= 5) EditingHeader.Text = Properties.Resources.TAGEDITOR_EDITINGHEADER + string.Join(", ", Displayfilepaths);
            else EditingHeader.Text = Properties.Resources.TAGEDITOR_EDITINGHEADER + string.Join(", ", Displayfilepaths.Take(5)) + " + " + (Displayfilepaths.Count - 4);
            Title = $"{string.Join(", ", Displayfilepaths)} | FRESHMusicPlayer Tag Editor";
            unsavedChanges = false;
        }

        public async Task SaveChanges(List<string> filePaths)
        {
            foreach (string path in filePaths)
            {
                var track = new Track(path)
                {
                    Artist = ArtistBox.Text,
                    Title = TitleBox.Text,
                    Album = AlbumBox.Text,
                    Genre = GenreBox.Text,
                    Year = Convert.ToInt32(YearBox.Text),
                    AlbumArtist = AlbumArtistBox.Text,
                    Composer = ComposerBox.Text,
                    TrackNumber = Convert.ToInt32(TrackNumBox.Text),
                    DiscNumber = Convert.ToInt32(DiscNumBox.Text),
                    Lyrics = new LyricsInfo()
                };
                track.Lyrics.UnsynchronizedLyrics = UntimedLyricsBox.Text;
                track.EmbeddedPictures.Clear();
                foreach (var cover in CoverArts) track.EmbeddedPictures.Add(cover);
                await Task.Run(() => track.Save());
                library?.Update(path, new FileMetadataProvider(path));
            }
           
        }

        public async Task SaveButton()
        {
            unsavedChanges = false;
            Title = $"{string.Join(", ", Displayfilepaths)} | FRESHMusicPlayer Tag Editor";
            foreach (string path in FilePaths)
            {
                if (path != player.FilePath) continue; // We're good
                else
                {
                    filePathsToSaveInBackground.AddRange(FilePaths); // SongChanged event handler will handle this
                    BackgroundSaveIndicator.Visibility = Visibility.Visible;
                    return;
                }
            }
            await SaveChanges(FilePaths);
        }

        public void ChangeFiles()
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                FilePaths = dialog.FileNames.ToList();
                InitFields();
            }
        }

        public void ChangeCoverArt()
        {
            void ShowNoCoverArt()
            {
                CoverArtLabel.Text = Properties.Resources.TAGEDITOR_NOCOVERART;
                CoverArtBox.Source = null;
            }
            RemoveCoverButton.IsEnabled = true;
            ImportCoverButton.IsEnabled = true;
            int selectedIndex = CoverArtSelector.SelectedIndex;
            if (selectedIndex == -1) selectedIndex = 0;
            if (CoverArts.Count <= 0)
            {
                RemoveCoverButton.IsEnabled = false;
                ShowNoCoverArt();
                CoverArtSelector.SelectedIndex = 0;
                return;
            }

            var currentCover = CoverArts[selectedIndex];
            if (currentCover.PictureData is null)
            {
                ShowNoCoverArt();
                return;
            }
            var currentCoverImage = System.Drawing.Image.FromStream(new MemoryStream(currentCover.PictureData));
            CoverArtBox.Source = BitmapFrame.Create(new MemoryStream(currentCover.PictureData), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            CoverArtLabel.Text =
                $"{currentCoverImage.Width}x{currentCoverImage.Height}\n" +
                $"{new System.Drawing.ImageFormatConverter().ConvertToString(currentCoverImage.RawFormat).ToUpper()}\n" +
                $"{currentCover.PicType}";
        }

        private void CoverArtSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) => ChangeCoverArt();

        private void AddCoverButton_Click(object sender, RoutedEventArgs e)
        {
            CoverArts.Add(new PictureInfo(PictureInfo.PIC_TYPE.Front));
            UpdateCoverArtSelector();
            if (CoverArtSelector.SelectedIndex == -1) CoverArtSelector.SelectedIndex = 0;
            ChangeCoverArt();
            SetUnsavedChanges(true);
        }
        private void RemoveCoverButton_Click(object sender, RoutedEventArgs e)
        {
            CoverArts.RemoveAt(CoverArtSelector.SelectedIndex);
            UpdateCoverArtSelector();
            ChangeCoverArt();
            SetUnsavedChanges(true);
        }
        private void ImportCoverButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            var image = CoverArts[CoverArtSelector.SelectedIndex];
            dialog.Filter = "Image Files|*.png;*.jpg|Other|*";
            if (dialog.ShowDialog() == true)
            {
                CoverArts[CoverArtSelector.SelectedIndex] = PictureInfo.fromBinaryData(
                    File.ReadAllBytes(dialog.FileName),
                    image.PicType);
            }
            ChangeCoverArt();
            SetUnsavedChanges(true);
        }
        private void UpdateCoverArtSelector()
        {
            var selectedIndex = CoverArtSelector.SelectedIndex;
            CoverArtSelector.Items.Clear();
            int i = 1;
            foreach (var cover in CoverArts)
            {
                CoverArtSelector.Items.Add(i.ToString());
                i++;
            }
            if (selectedIndex <= CoverArtSelector.Items.Count) CoverArtSelector.SelectedIndex = selectedIndex;
        }

        private async void Player_SongChanged(object sender, EventArgs e)
        {
            if (filePathsToSaveInBackground.Count != 0)
            {
                foreach (string path in filePathsToSaveInBackground)
                {
                    if (path == player.FilePath) break; // still listening to files that can't be properly saved
                }
                await SaveChanges(filePathsToSaveInBackground);
                filePathsToSaveInBackground.Clear();
                Close();
            }            
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (unsavedChanges == true)
            {
                MessageBoxResult result = MessageBox.Show(string.Format(Properties.Resources.TAGEDITOR_SAVECHANGES, string.Join(", ", Displayfilepaths)),
                                          "FRESHMusicPlayer Tag Editor",
                                          MessageBoxButton.YesNoCancel,
                                          MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes) await SaveButton();
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else return;
            }
            if (filePathsToSaveInBackground.Count != 0) // this logic is needed because you cannot save tags to a file that the player is currently playing
            {
                Hide(); // window will later close itself in the songchanged handler once all necessary files have been saved
                e.Cancel = true;   
            }
            else
            {
                player.SongChanged -= Player_SongChanged;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e) => await SaveButton();

        private void Button_Click_1(object sender, RoutedEventArgs e) => ChangeFiles();

        private void TextChanged(object sender, TextChangedEventArgs e) => SetUnsavedChanges(true);

        private void SetUnsavedChanges(bool state)
        {
            if (state)
            {
                unsavedChanges = true;
                Title = $"*{string.Join(", ", Displayfilepaths)} | FRESHMusicPlayer Tag Editor";
            }
            else unsavedChanges = false;
        }

        private void NewWindowItem_MouseDown(object sender, RoutedEventArgs e)
        {
            var tagEditor = new TagEditor(FilePaths, player, library);
            tagEditor.Show();
        }

        private void OpenMenu_MouseDown(object sender, RoutedEventArgs e) => ChangeFiles();

        private async void SaveMenuItem(object sender, RoutedEventArgs e) => await SaveButton();

        private void ExitMenuItem(object sender, RoutedEventArgs e) => Close();

        private void Window_DragEnter(object sender, DragEventArgs e) => e.Effects = DragDropEffects.Copy;

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] tracks = (string[])e.Data.GetData(DataFormats.FileDrop);
            FilePaths = tracks.ToList();
            InitFields();
        }

        private void DiscogsSourceMenuItem_Click(object sender, RoutedEventArgs e) => OpenAlbumIntegration(new DiscogsIntegration(httpClient));
        private void MusicBrainzSourceMenuItem_Click(object sender, RoutedEventArgs e) => OpenAlbumIntegration(new MusicBrainzIntegration(httpClient));
        private void OpenAlbumIntegration(IReleaseIntegration integration)
        {

            var dialog = new FMPTextEntryBox(Properties.Resources.TRACKINFO_ALBUM, AlbumBox.Text);
            dialog.ShowDialog();
            if (!dialog.OK) return;

            string query = dialog.Response;
            var results = integration.Search(query);

            var index = 0;
            if (!integration.Worked && integration.NeedsInternetConnection)
            {
                MessageBox.Show(Properties.Resources.TAGEDITOR_NOINTERNET, "FRESHMusicPlayer Tag Editor", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (results.Count == 0 | !integration.Worked)
            {
                MessageBox.Show(Properties.Resources.TAGEDITOR_NORESULTS, "FRESHMusicPlayer Tag Editor", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (results.Count > 1 && (results.Where(x => x.Name == AlbumBox.Text).Count() != 1))
            {
                var disambiguation = new IntegrationDisambiguation(results);
                disambiguation.ShowDialog();
                if (disambiguation.OK) index = disambiguation.SelectedIndex;
                else return;
            }

            var filePath = FilePaths[0];
            var release = integration.Fetch(results[index].Id);
            var editor = new ReleaseIntegrationPage(release, new Track(filePath));
            editor.ShowDialog();
            if (editor.OK)
            {
                ArtistBox.Text = editor.TrackToSave.Artist;
                TitleBox.Text = editor.TrackToSave.Title;
                AlbumBox.Text = editor.TrackToSave.Album;
                GenreBox.Text = editor.TrackToSave.Genre;
                YearBox.Text = editor.TrackToSave.Year.ToString();
                TrackNumBox.Text = editor.TrackToSave.TrackNumber.ToString();
            }
        }
    }
}
