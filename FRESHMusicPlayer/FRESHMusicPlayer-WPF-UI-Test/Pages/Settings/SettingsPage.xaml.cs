using FRESHMusicPlayer;
using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FRESHMusicPlayer.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public bool AppRestartNeeded = false;
        private bool pageInitialized = false;
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
                case Skin.Classic:
                    Appearance_ThemeClassicRadio.IsChecked = true;
                    break;
            }
            pageInitialized = true;
        }

        public void SetAppRestartNeeded(bool value)
        {
            if (value)
            {
                AppRestartNeeded = true;
            }
            else
            {
                AppRestartNeeded = false;
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
            switch (General_LanguageCombo.SelectedIndex) // i think this is bad code
            {
                case (int)LanguageCombo.English:
                    App.Config.Language = "en";
                    break;
                case (int)LanguageCombo.German:
                    App.Config.Language = "de";
                    break;
                case (int)LanguageCombo.Vietnamese:
                    App.Config.Language = "vi";
                    break;
                case (int)LanguageCombo.Portuguese:
                    App.Config.Language = "pt";
                    break;
            }
            ConfigurationHandler.Write(App.Config);
            SetAppRestartNeeded(true);
        }

        private void Appearance_ThemeChanged(object sender, RoutedEventArgs e)
        {
            var radioButton = (RadioButton)sender;
            switch (radioButton.Name)
            {
                case "Appearance_ThemeLightRadio":
                    App.Config.Theme = Skin.Light;
                    break;
                case "Appearance_ThemeDarkRadio":
                    App.Config.Theme = Skin.Dark;
                    break;
                case "Appearance_ThemeClassicRadio":
                    App.Config.Theme = Skin.Classic;
                    break;
            }
            ConfigurationHandler.Write(App.Config);
            SetAppRestartNeeded(true);
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

    }
    public enum LanguageCombo
    {
        English,
        German,
        Vietnamese,
        Portuguese
    }
}
