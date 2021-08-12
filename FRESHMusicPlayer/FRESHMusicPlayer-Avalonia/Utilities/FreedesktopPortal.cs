using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Desktop.DBus;
using JetBrains.Annotations;
using Tmds.DBus;

namespace FRESHMusicPlayer.Utilities
{
    public class FreedesktopPortal
    {
        public static async Task<bool> IsPortalAvailable()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && await Connection.Session.IsServiceActiveAsync("org.freedesktop.portal.Desktop");
        }

        public static async Task<string[]> OpenFiles(string windowTitle, IDictionary<string, object> options)
        {
            var fileChooser = Connection.Session.CreateProxy<IFileChooser>("org.freedesktop.portal.Desktop",
                "/org/freedesktop/portal/desktop");

            var completionSource = new TaskCompletionSource<string[]>();

            var requestObjectPath = await fileChooser.OpenFileAsync("", windowTitle, options);
            var request = Connection.Session.CreateProxy<IRequest>("org.freedesktop.portal.Desktop", requestObjectPath);
            await request.WatchResponseAsync((args =>
            {
                completionSource.TrySetResult((string[]) args.results["uris"]);
            }));

            var uris = await completionSource.Task;
            
            return uris.Select(uri => new Uri(uri).LocalPath).ToArray();
        }
    }
}