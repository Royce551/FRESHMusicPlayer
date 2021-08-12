using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATL;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Properties;
using FRESHMusicPlayer.Utilities;
using FRESHMusicPlayer.Views.TagEditor;
using ReactiveUI;
using Drawing = SixLabors.ImageSharp;

namespace FRESHMusicPlayer.ViewModels.TagEditor
{
    public class TagEditorViewModel : ViewModelBase
    {
        public Window Window { get; set; }
        public Player Player { get; set; }
        public Library Library { get; set; }

        private string artist;
        public string Artist
        {
            get => artist;
            set
            {
                this.RaiseAndSetIfChanged(ref artist, value);
                UnsavedChanges = true;
            }
        }
        private string title;
        public string Title
        {
            get => title;
            set 
            {
                this.RaiseAndSetIfChanged(ref title, value);
                UnsavedChanges = true;
            }
        }
        private string year;
        public string Year
        {
            get => year;
            set 
            {
                this.RaiseAndSetIfChanged(ref year, value);
                UnsavedChanges = true;
            }
        }
        private string genre;
        public string Genre
        {
            get => genre;
            set 
            {
                this.RaiseAndSetIfChanged(ref genre, value);
                UnsavedChanges = true;
            }
        }
        private string album;
        public string Album
        {
            get => album;
            set 
            {
                this.RaiseAndSetIfChanged(ref album, value);
                UnsavedChanges = true;
            }
        }

        private string albumArtist;
        public string AlbumArtist
        {
            get => albumArtist;
            set 
            {
                this.RaiseAndSetIfChanged(ref albumArtist, value);
                UnsavedChanges = true;
            }
        }
        private string composer;
        public string Composer
        {
            get => composer;
            set 
            {
                this.RaiseAndSetIfChanged(ref composer, value);
                UnsavedChanges = true;
            }
        }
        private string trackNumber;
        public string TrackNumber
        {
            get => trackNumber;
            set 
            {
                this.RaiseAndSetIfChanged(ref trackNumber, value);
                UnsavedChanges = true;
            }
        }
        private string discNumber;
        public string DiscNumber
        {
            get => discNumber;
            set
            {
                this.RaiseAndSetIfChanged(ref discNumber, value);
                UnsavedChanges = true;
            }
        }

        private Bitmap coverArt;
        public Bitmap CoverArt
        {
            get => coverArt;
            set => this.RaiseAndSetIfChanged(ref coverArt, value);
        }
        private string coverArtLabel;
        public string CoverArtLabel
        {
            get => coverArtLabel;
            set => this.RaiseAndSetIfChanged(ref coverArtLabel, value);
        }

        public ObservableCollection<string> AvailableCoverArts { get; set; } = new(); // this is used for display purposes; just 1-2-3
        public ObservableCollection<PictureInfo> CoverArts { get; set; } = new(); // actual available cover arts for internal purposes

        private int selectedCoverArt = -1;
        public int SelectedCoverArt
        {
            get => selectedCoverArt;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedCoverArt, value);
                ChangeCoverArt();
            }
        }

        public async void ImportCoverArtCommand()
        {
            string[] files = null;

            if (await FreedesktopPortal.IsPortalAvailable())
            {
                files = await FreedesktopPortal.OpenFiles("", new Dictionary<string, object>()
                {
                    {"filters", new[]
                    {
                        (Resources.FileFilter_ImageFiles, new[]
                        {
                            (0, "*.png"),
                            (0, "*.jpg")
                        }),
                        (Resources.FileFilter_Other, new[]
                        {
                            (0, "*")
                        })
                    }}
                });
            }
            else
            {
                var dialog = new OpenFileDialog()
                {
                    Filters = new List<FileDialogFilter>
                    {
                        new FileDialogFilter()
                        {
                            Name = Resources.FileFilter_ImageFiles,
                            Extensions = new List<string>() { "png", "jpg" }
                        },
                        new FileDialogFilter()
                        {
                            Name = Resources.FileFilter_Other,
                            Extensions = new List<string>() { "*" }
                        }
                    },
                    AllowMultiple = true
                };
                files = await dialog.ShowAsync(Window);
            }

            if (files == null) return;
            
            CoverArts[SelectedCoverArt] = PictureInfo.fromBinaryData(File.ReadAllBytes(files[0]), CoverArts[SelectedCoverArt].PicType);
            ChangeCoverArt();
            UnsavedChanges = true;
        }
        public bool CanImportCoverArt => FilePaths.Any();

        public void AddCoverArtCommand()
        {
            CoverArts.Add(new PictureInfo(PictureInfo.PIC_TYPE.Front));
            if (SelectedCoverArt == -1) SelectedCoverArt = 0;
            UpdateCoverArtSelector();
            ChangeCoverArt();
            UnsavedChanges = true;
        }

        public void RemoveCoverArtCommand()
        {
            CoverArts.RemoveAt(SelectedCoverArt);
            UpdateCoverArtSelector();
            ChangeCoverArt();
            UnsavedChanges = true;
        }
        public bool CanRemoveCoverArt => AvailableCoverArts.Any();

        public void ChangeCoverArt()
        {
            this.RaisePropertyChanged(nameof(CanImportCoverArt));
            this.RaisePropertyChanged(nameof(CanRemoveCoverArt));
            if (SelectedCoverArt == -1) SelectedCoverArt = 0;
            var currentCover = CoverArts[SelectedCoverArt];
            if (currentCover.PictureData is null)
            {
                CoverArtLabel = "No data for this cover";
                CoverArt = null;
                return;
            }
            var currentCoverImage = Drawing.Image.Identify(new MemoryStream(currentCover.PictureData), out var coverFormat);
            CoverArt = new Bitmap(new MemoryStream(currentCover.PictureData));
            CoverArtLabel =
                $"{currentCoverImage.Width}x{currentCoverImage.Height}\n" +
                $"{coverFormat.Name} Image\n" +
                $"{currentCover.PicType}";
        }
        public void UpdateCoverArtSelector()
        {
            var selectedIndex = SelectedCoverArt;
            AvailableCoverArts.Clear();
            int i = 1;
            foreach (var cover in CoverArts)
            {
                AvailableCoverArts.Add(i.ToString());
                i++;
            }
            if (SelectedCoverArt > AvailableCoverArts.Count) SelectedCoverArt = AvailableCoverArts.Count;
        }

        public List<string> FilePaths { get; set; } = new();

        private const string windowName = "FRESHMusicPlayer Tag Editor";
        public string WindowTitle
        {
            get
            {
                string initialString;
                if (FilePaths.Count > 0)
                {
                    initialString = $"{string.Join(", ", FilePaths)} | {windowName}";
                    if (UnsavedChanges) initialString = $"*{initialString}";
                    return initialString;
                }
                else
                {
                    return windowName;
                }
            }
        }

        private bool unsavedChanges = false;
        public bool UnsavedChanges
        {
            get => unsavedChanges;
            set
            {
                this.RaiseAndSetIfChanged(ref unsavedChanges, value);
                this.RaisePropertyChanged(nameof(WindowTitle));
            }
        }
        private bool isBackgroundSaveNeeded = false;
        public bool IsBackgroundSaveNeeded
        {
            get => isBackgroundSaveNeeded;
            set => this.RaiseAndSetIfChanged(ref isBackgroundSaveNeeded, value);
        }

        public void Initialize(List<string> filePaths)
        {
            FilePaths = filePaths;
            AvailableCoverArts.Clear();
            CoverArts.Clear();
            this.RaisePropertyChanged(nameof(WindowTitle));
            foreach (var path in filePaths)
            {
                var track = new Track(path);
                Artist = track.Artist;
                Title = track.Title;
                Album = track.Album;
                Genre = track.Genre;
                Year = track.Year.ToString();

                AlbumArtist = track.Album;
                Composer = track.Composer;
                TrackNumber = track.TrackNumber.ToString();
                DiscNumber = track.DiscNumber.ToString();

                int i = 1;
                if (track.EmbeddedPictures.Count != 0)
                {
                    foreach (var cover in track.EmbeddedPictures)
                    {
                        AvailableCoverArts.Add(i.ToString());
                        CoverArts.Add(cover);
                        i++;
                    }
                }
            }
            if (CoverArts.Count != 0)
            {
                SelectedCoverArt = 0;
                ChangeCoverArt();
            }
            UnsavedChanges = false; // override setting these usually making unsaved changes true, kinda jank but it works lol
        }

        public void NewWindowCommand() => new Views.TagEditor.TagEditor().SetStuff(Player, Library).SetInitialFiles(FilePaths).Show();

        private List<string> acceptableFilePaths = "wav;aiff;mp3;wma;3g2;3gp;3gp2;3gpp;asf;wmv;aac;adts;avi;m4a;m4a;m4v;mov;mp4;sami;smi;flac".Split(';').ToList();
        public async void OpenCommand()
        {
            string[] files;

            if (await FreedesktopPortal.IsPortalAvailable())
            {
                files = await FreedesktopPortal.OpenFiles("", new Dictionary<string, object>()
                {
                    {"multiple", true},
                    {"filters", new[]
                    {
                        (Resources.FileFilter_AudioFiles, acceptableFilePaths.Select(type => (0u, "*." + type)).ToArray()),
                        (Resources.FileFilter_Other, new[]
                        {
                            (0u, "*")
                        })
                    }}
                });
            }
            else
            {
                var dialog = new OpenFileDialog()
                {
                    Filters = new List<FileDialogFilter>
                    {
                        new FileDialogFilter()
                        {
                            Name = Resources.FileFilter_AudioFiles,
                            Extensions = acceptableFilePaths
                        },
                        new FileDialogFilter()
                        {
                            Name = Resources.FileFilter_Other,
                            Extensions = new List<string>() { "*" }
                        }
                    },
                    AllowMultiple = true
                };
                files = await dialog.ShowAsync(Window);
            }
            
            Initialize(files.ToList());
        }

        private List<string> filePathsToSaveInBackground = new();
        public void SaveCommand()
        {
            foreach (string path in FilePaths)
            {
                if (path != Player.FilePath) continue;
                else
                {
                    filePathsToSaveInBackground.AddRange(FilePaths);
                    IsBackgroundSaveNeeded = true;
                }
            }

            foreach (string path in FilePaths)
            {
                var track = new Track(path)
                {
                    Artist = Artist,
                    Title = Title,
                    Album = Album,
                    Genre = Genre,
                    Year = Convert.ToInt32(Year),
                    AlbumArtist = AlbumArtist,
                    Composer = Composer,
                    TrackNumber = Convert.ToInt32(TrackNumber),
                    DiscNumber = Convert.ToInt32(DiscNumber)
                };
                track.EmbeddedPictures.Clear();
                foreach (var cover in CoverArts) track.EmbeddedPictures.Add(cover);
                track.Save();
                Library?.Remove(path); // update library entry, if available
                Library?.Import(path);
            }
            UnsavedChanges = false;
        }

        public void ExitCommand() => Window.Close();
    }
}
