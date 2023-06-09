using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATL.Playlist;
using Avalonia.Controls;
using FRESHMusicPlayer.Controls;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Properties;
using ReactiveUI;

namespace FRESHMusicPlayer.ViewModels
{
    public class ImportTabViewModel : ViewModelBase
    {
        public MainWindowViewModel Window { get; set; }

        private List<string> acceptableFilePaths = "wav;aiff;mp3;wma;3g2;3gp;3gp2;3gpp;asf;wmv;aac;adts;avi;m4a;m4a;m4v;mov;mp4;sami;smi;flac".Split(';').ToList();
        public async void BrowseTracksCommand()
        {
            var dialog = new OpenFileDialogEx()
            {
                Filters = new[]
                {
                    new FileDialogFilter()
                    {
                        Name = Resources.FileFilter_AudioFiles,
                        Extensions = acceptableFilePaths
                    }
                },
                AllowMultiple = true,
                WindowTitle = Resources.Import,
                AcceptButtonLabel = Resources.Import
            };
            var files = await dialog.ShowAsync(GetMainWindow());
            if (files?.Length > 0) await Task.Run(() => Window.Library.Import(files));
        }

        public async void BrowsePlaylistsCommand()
        {
            string[] acceptableFiles = { "xspf", "asx", "wvx", "b4s", "m3u", "m3u8", "pls", "smil", "smi", "zpl" };
            var dialog = new OpenFileDialogEx()
            {
                Filters = new[]
                {
                    new FileDialogFilter()
                    {
                        Name = Resources.FileFilter_PlaylistFiles,
                        Extensions = acceptableFiles.ToList()
                    }
                },
                WindowTitle = Resources.ImportPlaylistFiles,
                AcceptButtonLabel = Resources.ImportPlaylistFiles
            };
            var files = await dialog.ShowAsync(GetMainWindow());

            if (files.Length <= 0 && files is null) return;

            var reader = PlaylistIOFactory.GetInstance().GetPlaylistIO(files[0]);
            foreach (var s in reader.FilePaths)
            {
                if (!File.Exists(s))
                {
                    Window.Notifications.Add(new()
                    {
                        ContentText = string.Format(Properties.Resources.Notification_FileInPlaylistMissing,
                            Path.GetFileName(s)),
                        DisplayAsToast = true,
                        IsImportant = true,
                        Type = NotificationType.Failure
                    });
                    continue;
                }
            }

            Window.Player.Queue.Add(reader.FilePaths.ToArray());
            await Task.Run(() => Window.Library.Import(reader.FilePaths.ToArray()));
            await Window.Player.PlayAsync();
        }

        //public void BrowseFoldersCommand()
        //{

        //}

        private string filePath;
        public string FilePath
        {
            get => filePath;
            set => this.RaiseAndSetIfChanged(ref filePath, value);
        }

        public async void ImportFilePathCommand()
        {
            if (string.IsNullOrEmpty(FilePath)) return;
            Window.Library.Import(FilePath);
            await Window.Player.PlayAsync(FilePath);
        }
    }
}
