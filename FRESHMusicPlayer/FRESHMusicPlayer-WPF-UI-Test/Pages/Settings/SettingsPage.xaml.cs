using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.Utilities;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;

namespace FRESHMusicPlayer.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public bool AppRestartNeeded = false;

        private bool pageInitialized = false;
        private readonly ConfigurationFile workingConfig = new ConfigurationFile(); // Instance of config that's compared to the actual config file to set AppRestartNeeded.

        public SettingsPage()
        {
            InitializeComponent();
            InitFields();
        }

        private void InitFields()
        {
            General_ProgressCheck.IsChecked = App.Config.ShowTimeInWindow;
            Integration_DiscordRPCCheck.IsChecked = App.Config.IntegrateDiscordRPC;
            Integration_SMTCCheck.IsChecked = App.Config.IntegrateSMTC;
            switch (App.Config.Language) // i think this is bad code
            {
                case "en":
                    General_LanguageCombo.SelectedIndex = (int)LanguageCombo.English;
                    break;
                case "de":
                    General_LanguageCombo.SelectedIndex = (int)LanguageCombo.German;
                    break;
                case "vi":
                    General_LanguageCombo.SelectedIndex = (int)LanguageCombo.Vietnamese;
                    break;
                case "pt":
                    General_LanguageCombo.SelectedIndex = (int)LanguageCombo.Portuguese;
                    break;
            }
            switch (App.Config.Theme)
            {
                case Skin.Light:
                    Appearance_ThemeLightRadio.IsChecked = true;
                    break;
                case Skin.Dark:
                    Appearance_ThemeDarkRadio.IsChecked = true;
                    break;
            }
            pageInitialized = true;
        }

        public void SetAppRestartNeeded(bool value)
        {
            if (value)
            {
                AppRestartNeeded = true;
                RestartNeededHeader.Visibility = Visibility.Visible;
                RestartNowButton.Visibility = Visibility.Visible;
            }
            else
            {
                AppRestartNeeded = false;
                RestartNeededHeader.Visibility = Visibility.Collapsed;
                RestartNowButton.Visibility = Visibility.Collapsed;
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void General_ProgressChanged(object sender, RoutedEventArgs e)
        {
            App.Config.ShowTimeInWindow = (bool)General_ProgressCheck.IsChecked;
            ConfigurationHandler.Write(App.Config);
        }

        private void General_DiscordRPCChanged(object sender, RoutedEventArgs e)
        {
            if (pageInitialized)
            {
                App.Config.IntegrateDiscordRPC = (bool)Integration_DiscordRPCCheck.IsChecked;
                ConfigurationHandler.Write(App.Config);
                (Application.Current.MainWindow as MainWindow)?.UpdateIntegrations();
            }        
        }

        private void Integration_SMTCChanged(object sender, RoutedEventArgs e)
        {
            if (pageInitialized)
            {
                App.Config.IntegrateSMTC = (bool)Integration_SMTCCheck.IsChecked;
                ConfigurationHandler.Write(App.Config);
                (Application.Current.MainWindow as MainWindow)?.UpdateIntegrations();
            }  
        }

        private void General_LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pageInitialized)
            {
                switch (General_LanguageCombo.SelectedIndex) // i think this is bad code
                {
                    case (int)LanguageCombo.English:
                        workingConfig.Language = "en";
                        break;
                    case (int)LanguageCombo.German:
                        workingConfig.Language = "de";
                        break;
                    case (int)LanguageCombo.Vietnamese:
                        workingConfig.Language = "vi";
                        break;
                    case (int)LanguageCombo.Portuguese:
                        workingConfig.Language = "pt";
                        break;
                }
                SetAppRestartNeeded(App.Config.Language != workingConfig.Language);
                ConfigurationHandler.Write(App.Config);
            }
        }

        private void Appearance_ThemeChanged(object sender, RoutedEventArgs e)
        {
            if (pageInitialized)
            {
                var radioButton = (RadioButton)sender;
                switch (radioButton.Name)
                {
                    case "Appearance_ThemeLightRadio":
                        workingConfig.Theme = Skin.Light;
                        break;
                    case "Appearance_ThemeDarkRadio":
                        workingConfig.Theme = Skin.Dark;
                        break;
                }
                SetAppRestartNeeded(App.Config.Theme != workingConfig.Theme);
                ConfigurationHandler.Write(App.Config);
            }
        }

        private void Maintanence_ImportButton_Click(object sender, RoutedEventArgs e) => DatabaseUtils.Convertv1Tov2();

        private void Maintanence_ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationHandler.Write(new ConfigurationFile());
            InitFields();
        }
        private void Maintanence_NukeButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("You are about irreversibly clear your library. Are you sure?",
                                          "FRESHMusicPlayer",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes) DatabaseUtils.Nuke();
        }

        private void RestartNowButton_Click(object sender, RoutedEventArgs e)
        {
            WinForms.Application.Restart();
            Application.Current.Shutdown();
        }
    }
    public enum LanguageCombo
    {
        English,
        German,
        Vietnamese,
        Portuguese
    }
}
