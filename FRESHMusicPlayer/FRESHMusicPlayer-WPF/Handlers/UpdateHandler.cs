using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.Handlers.Notifications;
using Squirrel;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace FRESHMusicPlayer.Handlers
{
    public class UpdateHandler
    {
        private readonly string rootPath;

        private readonly NotificationHandler notificationHandler;
        public UpdateHandler(NotificationHandler notificationHandler)
        {
            this.notificationHandler = notificationHandler;
            rootPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"../");
        }
        public async Task UpdateApp(bool useDeltaPatching = true, bool forceUpdate = false)
        {
            if (App.Config.UpdateMode == UpdateMode.Manual && !forceUpdate) return;
            // Updater not present, probably standalone
            if (!File.Exists(Path.Combine(rootPath, "Update.exe"))) return;
            App.Config.UpdatesLastChecked = DateTime.Now;
            var notification = new Notification();

            var updatePath =
#if !BLUEPRINT
            "https://github.com/Royce551/FRESHMusicPlayer";
#else
            "https://github.com/Royce551/FRESHMusicPlayer-WindowsBlueprint-Releases";
#endif

            UpdateManager mgr = await UpdateManager.GitHubUpdateManager(updatePath);
            try
            {
                UpdateInfo updateInfo = await mgr.CheckForUpdate(!useDeltaPatching);
                if (updateInfo.CurrentlyInstalledVersion == null) return; // Standalone version of FMP, don't bother
                if (updateInfo.ReleasesToApply.Count == 0) return; // No updates to apply, don't bother

                notification.ContentText = Properties.Resources.NOTIFICATION_INSTALLINGUPDATE;
                notificationHandler.Add(notification);

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
                    notificationHandler.Update(notification);
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
                notificationHandler.Update(notification);
            }
            finally
            {
                mgr?.Dispose();
            }
        }
        private void RestartApp()
        {
            Application.Current.Shutdown();
            Process.Start(Path.Combine(rootPath, "FRESHMusicPlayer.exe"));
        }
    }
}
