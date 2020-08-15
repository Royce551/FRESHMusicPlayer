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
            //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("pt");

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
            MessageBox.Show(string.Format(FRESHMusicPlayer_WPF_UI_Test.Properties.Resources.APPLICATION_CRITICALERROR, e.Exception.Message.ToString(), logPath + fileName));       
            e.Handled = true;
        }
    }
}
