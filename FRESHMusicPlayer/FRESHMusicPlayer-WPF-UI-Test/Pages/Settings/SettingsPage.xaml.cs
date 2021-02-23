using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly ConfigurationFile workingConfig = new ConfigurationFile { Language = App.Config.Language, Theme = App.Config.Theme }; // Copy of config that's compared to the actual config file to set AppRestartNeeded.

        public SettingsPage()
        {
            InitializeComponent();
            InitFields();
        }

        private void InitFields()
        {
            General_ProgressCheck.IsChecked = App.Config.ShowTimeInWindow;
            General_TrackingCheck.IsChecked = App.Config.PlaybackTracking;
            Integration_DiscordRPCCheck.IsChecked = App.Config.IntegrateDiscordRPC;
            Integration_SMTCCheck.IsChecked = App.Config.IntegrateSMTC;
            Updates_LastCheckedLabel.Text = string.Format(Properties.Resources.SETTINGS_UPDATESLASTCHECKED, App.Config.UpdatesLastChecked);
            switch (App.Config.Language) // TODO: investigate making this less ugly
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
                case "tr":
                    General_LanguageCombo.SelectedIndex = (int)LanguageCombo.Turkish;
                    break;
                case "nl":
                    General_LanguageCombo.SelectedIndex = (int)LanguageCombo.Dutch;
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
            switch (App.Config.UpdateMode)
            {
                case UpdateMode.Automatic:
                    General_UpdateModeCombo.SelectedIndex = (int)UpdateCombo.Automatic;
                    break;
                case UpdateMode.Manual:
                    General_UpdateModeCombo.SelectedIndex = (int)UpdateCombo.Manual;
                    break;
                case UpdateMode.Prompt:
                    General_UpdateModeCombo.SelectedIndex = (int)UpdateCombo.Prompt;
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
        }

        private void General_DiscordRPCChanged(object sender, RoutedEventArgs e)
        {
            if (pageInitialized)
            {
                App.Config.IntegrateDiscordRPC = (bool)Integration_DiscordRPCCheck.IsChecked;
                (Application.Current.MainWindow as MainWindow)?.UpdateIntegrations();
            }        
        }

        private void Integration_SMTCChanged(object sender, RoutedEventArgs e)
        {
            if (pageInitialized)
            {
                App.Config.IntegrateSMTC = (bool)Integration_SMTCCheck.IsChecked;
                (Application.Current.MainWindow as MainWindow)?.UpdateIntegrations();
            }  
        }
        private void General_TrackingChanged(object sender, RoutedEventArgs e)
        {
            if (pageInitialized)
            {
                App.Config.PlaybackTracking = (bool)General_TrackingCheck.IsChecked;
                (Application.Current.MainWindow as MainWindow)?.ProcessSettings();
            }
        }
        private void General_LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pageInitialized)
            {
                switch (General_LanguageCombo.SelectedIndex)
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
                    case (int)LanguageCombo.Turkish:
                        App.Config.Language = "tr";
                        break;
                    case (int)LanguageCombo.Dutch:
                        App.Config.Language = "nl";
                        break;
                }
                SetAppRestartNeeded(App.Config.Language != workingConfig.Language);
            }
        }
        private void General_UpdateModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pageInitialized)
            {
                switch (General_UpdateModeCombo.SelectedIndex)
                {
                    case (int)UpdateCombo.Prompt:
                        App.Config.UpdateMode = UpdateMode.Prompt;
                        break;
                    case (int)UpdateCombo.Manual:
                        App.Config.UpdateMode = UpdateMode.Manual;
                        break;
                    case (int)UpdateCombo.Automatic:
                        App.Config.UpdateMode = UpdateMode.Automatic;
                        break;
                }
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
                        App.Config.Theme = Skin.Light;
                        break;
                    case "Appearance_ThemeDarkRadio":
                        App.Config.Theme = Skin.Dark;
                        break;
                }
                SetAppRestartNeeded(App.Config.Theme != workingConfig.Theme);
            }
        }

        private void Maintanence_ImportButton_Click(object sender, RoutedEventArgs e) => DatabaseUtils.Convertv1Tov2();

        private void Maintanence_ResetButton_Click(object sender, RoutedEventArgs e)
        {
            App.Config = new ConfigurationFile();
            InitFields();
        }
        private void Maintanence_NukeButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(Properties.Resources.SETTINGS_NUKE_LIBRARY_WARNING,
                                          "FRESHMusicPlayer",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes) DatabaseUtils.Nuke();
        }

        private void RestartNowButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            WinForms.Application.Restart();
        }

        private async void Updates_CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            await UpdateHandler.UpdateApp(forceUpdate:true);
            InitFields();
        }

        private async void Maintenence_UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                var tracks = DatabaseUtils.Read().Select(x => x.Path).Distinct();
                Dispatcher.Invoke(() => DatabaseUtils.Nuke(false));
                DatabaseUtils.Import(tracks.ToArray());
            });
        }

        private void Plugins_OpenFolder_Click(object sender, RoutedEventArgs e) => Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "FRESHMusicPlayer", "Plugins", "FMP-WPF"));

    }
    public enum LanguageCombo
    {
        English,
        German,
        Vietnamese,
        Portuguese,
        Turkish,
        Dutch
    }
    public enum UpdateCombo
    {
        Prompt,
        Manual,
        Automatic
    }
}
