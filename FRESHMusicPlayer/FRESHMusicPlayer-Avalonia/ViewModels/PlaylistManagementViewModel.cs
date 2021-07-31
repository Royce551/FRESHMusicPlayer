using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATL.Playlist;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Views;
using ReactiveUI;

namespace FRESHMusicPlayer.ViewModels
{
    public class PlaylistManagementViewModel : ViewModelBase
    {
        public MainWindowViewModel MainWindow { get; set; }
        public string Track { get; set; }

        public ObservableCollection<DisplayPlaylist> Playlists { get; } = new();

        private string editingHeader;
        public string EditingHeader
        {
            get => editingHeader;
            set => this.RaiseAndSetIfChanged(ref editingHeader, value);
        }

        public void Initialize()
        {
            if (Track is not null)
                EditingHeader = string.Format(Properties.Resources.PlaylistManagement_Header, Path.GetFileName(Track));

            Playlists.Clear();
            var x = MainWindow.Library.Database.GetCollection<DatabasePlaylist>("playlists").Query().OrderBy("Name").ToList();
            foreach (var playlist in x)
            {
                Playlists.Add(new()
                {
                    Name = playlist.Name,
                    IsSelectedTrackHere = Track is not null && playlist.Tracks.Contains(Track),
                    ThingName = MainWindow?.SelectedTab switch
                    {
                        1 => $"+ {Properties.Resources.Artist}",
                        2 => $"+ {Properties.Resources.Album}",
                        _ => null
                    } ?? null,
                    ShouldThingBeVisible = MainWindow?.SelectedTab == 1 || MainWindow?.SelectedTab == 2
                });
            }
        }

        public void AddToPlaylistCommand(string playlist)
        {
            MainWindow.Library.AddTrackToPlaylist(playlist, Track);
            Initialize();
        }
        public void RemoveFromPlaylistCommand(string playlist)
        {
            MainWindow.Library.RemoveTrackFromPlaylist(playlist, Track);
            Initialize();
        }

        public async void AddThingToPlaylistCommand(string playlist)
        {
            List<DatabaseTrack> things;
            if (MainWindow.SelectedTab == 1)
                things = MainWindow.Library.ReadTracksForArtist(MainWindow.Library.GetFallbackTrack(Track).Artist);
            else
                things = MainWindow.Library.ReadTracksForAlbum(MainWindow.Library.GetFallbackTrack(Track).Album);
            await Task.Run(() =>
            {
                foreach (var thing in things)
                    MainWindow.Library.AddTrackToPlaylist(playlist, thing.Path);
            });
            Initialize();
        }

        public async void RenamePlaylistCommand(string playlist)
        {
            var dialog = new TextEntryBox().SetStuff(Properties.Resources.PlaylistManagement_PlaylistName);
            (dialog.DataContext as TextEntryBoxViewModel).Text = playlist;
            await dialog.ShowDialog(GetMainWindow());

            if (dialog.OK)
            {
                var x = MainWindow.Library.Database.GetCollection<DatabasePlaylist>("playlists").FindOne(y => y.Name == playlist);
                x.Name = (dialog.DataContext as TextEntryBoxViewModel).Text;
                MainWindow.Library.Database.GetCollection<DatabasePlaylist>("playlists").Update(x);
                Initialize();
            }
        }
        public void DeletePlaylistCommand(string playlist)
        {
            MainWindow.Library.DeletePlaylist(playlist);
            Initialize();
        }
        public async void ExportPlaylistCommand(string playlist)
        {
            var dialog = new SaveFileDialog()
            {
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter()
                    {
                        Name = "M3U UTF-8 Playlist",
                        Extensions = new(){ "m3u8" }
                    }
                }
            };
            var path = await dialog.ShowAsync(GetMainWindow());

            var tracks = MainWindow.Library.ReadTracksForPlaylist(playlist);
            IPlaylistIO pls = PlaylistIOFactory.GetInstance().GetPlaylistIO(path);
            IList<string> pathsToWrite = new List<string>();
            foreach (var track in tracks)
            {
                pathsToWrite.Add(track.Path);
            }
            pls.FilePaths = pathsToWrite;
        }

        public async void CreatePlaylistCommand()
        {
            var dialog = new TextEntryBox().SetStuff(Properties.Resources.PlaylistManagement_PlaylistName);
            await dialog.ShowDialog(GetMainWindow());

            if (dialog.OK)
            {
                if (string.IsNullOrWhiteSpace((dialog.DataContext as TextEntryBoxViewModel).Text))
                    new MessageBox().SetStuff(MainWindowViewModel.ProjectName, Properties.Resources.PlaylistManagement_InvalidName).Show(GetMainWindow());
                else
                {
                    MainWindow.Library.CreatePlaylist((dialog.DataContext as TextEntryBoxViewModel).Text, Track);
                    Initialize();
                }
            }
        }

        public async void ImportCommand()
        {
            var dialog = new OpenFileDialog()
            {
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter()
                    {
                        Name = "Playlist Files",
                        Extensions = new(){ "xspf", "asx", "wvx", "b4s", "m3u", "m3u8", "pls", "smil", "smi", "zpl"}
                    }
                }
            };
            var files = await dialog.ShowAsync(GetMainWindow());
            IPlaylistIO reader = PlaylistIOFactory.GetInstance().GetPlaylistIO(files[0]);
            foreach (string s in reader.FilePaths)
            {
                if (!File.Exists(s))
                {
                    MainWindow.Notifications.Add(new()
                    {
                        ContentText = string.Format(Properties.Resources.Notification_FileInPlaylistMissing, Path.GetFileName(s)),
                        DisplayAsToast = true,
                        IsImportant = true,
                        Type = NotificationType.Failure
                    });
                    continue;
                }
                MainWindow.Library.AddTrackToPlaylist(Path.GetFileNameWithoutExtension(files[0]), s);
            }
            Initialize();
        }
    }

    public class DisplayPlaylist
    {
        public string Name { get; init; }

        public bool IsSelectedTrackHere { get; init; }

        public string ThingName { get; init; } // bit hacky but it works lol
        public bool ShouldThingBeVisible { get; init; }
    }

    public class AndValueConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            var x = values[0];
            var z = values[1];
            if (x is bool object1 && z is bool object2) return object1 && object2;
            else return false;
        }

        public object ConvertBack(List<object> value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
