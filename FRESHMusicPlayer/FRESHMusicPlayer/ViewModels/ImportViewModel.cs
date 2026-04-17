using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels
{
    public partial class ImportViewModel : ViewModelBase
    {
        public async Task BrowseTracks()
        {
            var topLevel = TopLevel.GetTopLevel(MainView.MainWindow);
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                AllowMultiple = true,
                FileTypeFilter = new[] { FilePickerFileTypes.All } // TODO: do this correctly
            });

            if (files.Count >= 1)
            {
                Debug.WriteLine(files[0].Path.LocalPath);
                await MainView.Library.ImportAsync(files.Select(x => x.Path.LocalPath).ToList());
            }
        }

        public async Task BrowseFolders()
        {
            var topLevel = TopLevel.GetTopLevel(MainView.MainWindow);
            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions
            {
                AllowMultiple = false
            });
            if (folders.Count >= 1)
            {
                Debug.WriteLine(folders[0].Path.LocalPath);
                string[] paths = await Task.Run(
                        () => Directory.EnumerateFiles(folders[0].Path.LocalPath, "*", SearchOption.AllDirectories)
                                .Where(name => name.EndsWith(".mp3")
                                || name.EndsWith(".wav") || name.EndsWith(".m4a") || name.EndsWith(".ogg")
                                || name.EndsWith(".flac") || name.EndsWith(".aiff")
                                || name.EndsWith(".wma")
                                || name.EndsWith(".aac"))
                                .ToArray());
                await MainView.Library.ImportAsync(paths);
            }
        }
    }
}
