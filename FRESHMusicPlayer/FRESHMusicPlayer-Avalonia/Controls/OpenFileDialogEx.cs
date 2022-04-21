using Avalonia.Controls;
using FRESHMusicPlayer.Properties;
using FRESHMusicPlayer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Controls
{
    public class OpenFileDialogEx
    {
        public FileDialogFilter[] Filters { get; init; }

        public bool AllowMultiple { get; init; }

        public string WindowTitle { get; init; }

        public string AcceptButtonLabel { get; init; }

        public bool IncludeOtherOption { get; init; }

        public async Task<string[]> ShowAsync(Window window)
        {
            if (await FreedesktopPortal.IsPortalAvailable()) // FreeDesktopPortal allows us to display a more appropiate file
            {                                                // selector for the user's DE on Linux rather than always using the GTK
                var dialogProperties = new Dictionary<string, object>() // one
                {
                    { "multiple", AllowMultiple },
                    { "accept_label", AcceptButtonLabel ?? Resources.OK },
                };

                // pain
                List<(string, (int, string)[])> nativeFilters = new();
                foreach (var filter in Filters)
                {
                    var nativeExtensions = new List<(int, string)>();
                    foreach (var extension in filter.Extensions)
                    {
                        nativeExtensions.Add(new(0, $"*.{extension}"));
                    }
                    if (IncludeOtherOption) nativeExtensions.Add(new(0, "*"));
                    nativeFilters.Add((filter.Name, nativeExtensions.ToArray()));
                }

                dialogProperties.Add("filters", nativeFilters.ToArray());

                return await FreedesktopPortal.OpenFiles(WindowTitle, dialogProperties);
            }
            else
            {
                var dialog = new OpenFileDialog()
                {
                    Filters = Filters.ToList(),
                    AllowMultiple = AllowMultiple,
                    Title = WindowTitle
                };
                if (IncludeOtherOption)
                    dialog.Filters.Add(new() { Name = Resources.FileFilter_Other, Extensions = new() { "*" } });
                return await dialog.ShowAsync(window);
            }
        }
    }
}
