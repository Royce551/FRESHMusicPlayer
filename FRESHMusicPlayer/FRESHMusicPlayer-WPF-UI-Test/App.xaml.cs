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
    }
}
