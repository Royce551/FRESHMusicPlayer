using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    }
}
