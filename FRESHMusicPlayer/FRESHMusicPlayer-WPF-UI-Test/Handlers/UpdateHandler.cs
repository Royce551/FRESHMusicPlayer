using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.Handlers.Notifications;
using Squirrel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace FRESHMusicPlayer.Handlers
{
    public class UpdateHandler
    {
        public static async Task UpdateApp(bool useDeltaPatching = true)
        {
            if (App.Config.UpdateMode == UpdateMode.Manual) return;
            // Updater not present, probably standalone
            if (!File.Exists(Path.Combine(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"../")), "Update.exe"))) return;
            App.Config.UpdatesLastChecked = DateTime.Now;
            var notification = new Notification();
            UpdateManager mgr = await UpdateManager.GitHubUpdateManager("https://github.com/Royce551/FRESHMusicPlayer");
            try
            {
                UpdateInfo updateInfo = await mgr.CheckForUpdate(!useDeltaPatching);
                if (updateInfo.CurrentlyInstalledVersion == null) return; // Standalone version of FMP, don't bother
                if (updateInfo.ReleasesToApply.Count == 0) return; // No updates to apply, don't bother

                notification.ContentText = Properties.Resources.NOTIFICATION_INSTALLINGUPDATE;
                MainWindow.NotificationHandler.Add(notification);

                await mgr.DownloadReleases(updateInfo.ReleasesToApply);
                await mgr.ApplyReleases(updateInfo);
                if (App.Config.UpdateMode == UpdateMode.Prompt)
                {
                    notification.ContentText = Properties.Resources.NOTIFICATION_UPDATEREADY;
                    notification.ButtonText = Properties.Resources.SETTINGS_RESTART_NOW;
                    notification.Type = NotificationType.Success;
                    notification.OnButtonClicked = () =>
                    {
                        RestartApp();
                        return true;
                    };
                    MainWindow.NotificationHandler.Update(notification);
                }
            
                else RestartApp();
            }
            catch (Exception e)
            {
                if (useDeltaPatching)
                {
                    await UpdateApp(false);
                    return;
                }
                notification.ContentText = string.Format(Properties.Resources.NOTIFICATION_UPDATEERROR, e.Message);
                notification.Type = NotificationType.Failure;
                MainWindow.NotificationHandler.Update(notification);
            }
            finally
            {
                mgr?.Dispose();
            }
        }
        private static void RestartApp()
        {
            Application.Current.Shutdown();
            WinForms.Application.Restart();
        }
    }
}
