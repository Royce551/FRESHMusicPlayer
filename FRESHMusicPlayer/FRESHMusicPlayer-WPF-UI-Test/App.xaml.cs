using System;
using System.IO;
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
        void App_Startup(object sender, StartupEventArgs e )
        {
            //Force Viet for the time being
            //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("vi-VN");

            MainWindow window = new MainWindow();
            window.Show();
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
            File.WriteAllText(logPath + fileName, e.Exception.ToString());
            MessageBox.Show($"An error has occured: {e.Exception.Message}. " +
                            $"\nIf you have the time, please report this to the devs at https://github.com/royce551/freshmusicplayer/issues." +
                            $"\nThe log file they'll need is at {logPath + fileName}.");       
            e.Handled = true;
        }
    }
}
