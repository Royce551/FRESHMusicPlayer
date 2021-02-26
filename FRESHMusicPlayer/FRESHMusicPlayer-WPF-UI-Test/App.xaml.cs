using FRESHMusicPlayer.Forms;
using FRESHMusicPlayer.Forms.TagEditor;
using FRESHMusicPlayer.Handlers.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace FRESHMusicPlayer
{
    public enum Skin
    {
        Light, Dark, Classic
    }
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ConfigurationFile Config;
        private Window currentWindow;
        private Player player;
        void App_Startup(object sender, StartupEventArgs e )
        {
            Config = ConfigurationHandler.Read();
            player = new Player { CurrentVolume = Config.Volume };
            if (Config.Language != "en") System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Config.Language);
            ChangeSkin(Config.Theme);

            if (e.Args.Length > 0)
            {
                var args = e.Args.Where(x => x.Contains('.'));
                if (e.Args.Contains("--tageditor")) currentWindow = new TagEditor(args.ToList(), player);
                else currentWindow = new MainWindow(player, args.ToArray());
            }
            else currentWindow = new MainWindow(player);
            currentWindow.Show();
        }
        public static Skin CurrentSkin { get; set; } = Skin.Dark;
        public void ChangeSkin(Skin newSkin)
        {
            CurrentSkin = newSkin;

            foreach (ResourceDictionary dict in Resources.MergedDictionaries)
            {

                if (dict is SkinResourceDictionary skinDict)
                    skinDict.UpdateSource();
                else
                    dict.Source = dict.Source;
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string logPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\FRESHMusicPlayer\\Logs";
            string fileName = $"\\{DateTime.Now:M.d.yyyy hh mm tt}.txt";
            if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);
            File.WriteAllText(logPath + fileName, 
                $"FRESHMusicPlayer {Assembly.GetEntryAssembly().GetName().Version}\n" +
                $"{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}\n" +
                $"{Environment.OSVersion.VersionString}\n" +
                $"{e.Exception}");
            try
            {
                var box = new CriticalErrorBox(e, logPath, fileName);
                box.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                box.ShowDialog();
            }
            catch
            {
                MessageBox.Show(string.Format(FRESHMusicPlayer.Properties.Resources.APPLICATION_CRITICALERROR, e.Exception.Message.ToString(), logPath + fileName));
            }
            e.Handled = true;
        }
    }
}
