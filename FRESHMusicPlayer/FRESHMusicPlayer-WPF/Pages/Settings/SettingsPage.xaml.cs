﻿using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;

namespace FRESHMusicPlayer.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        public bool AppRestartNeeded = false;

        private bool pageInitialized = false;
        private readonly ConfigurationFile workingConfig = new ConfigurationFile { Language = App.Config.Language, Theme = App.Config.Theme }; // Copy of config that's compared to the actual config file to set AppRestartNeeded.

        private readonly MainWindow window;
        public SettingsPage(MainWindow window)
        {
            this.window = window;
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
            FMPVersionLabel.Text = $"FRESHMusicPlayer {Assembly.GetEntryAssembly().GetName().Version}";
            switch (App.Config.Language) // TODO: investigate making this less ugly
            {
                case "automatic":
                    General_LanguageCombo.SelectedIndex = (int)LanguageCombo.Automatic;
                    break;
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
                case "da":
                    General_LanguageCombo.SelectedIndex = (int)LanguageCombo.Danish;
                    break;
                case "ar":
                    General_LanguageCombo.SelectedIndex = (int)LanguageCombo.Arabic;
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
#if UPDATER
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
#else
            UpdateHeader.Visibility =
            UpdateModeGrid.Visibility = 
            UpdateModeHeader.Visibility = 
            General_UpdateModeCombo.Visibility = 
            Updates_LastCheckedLabel.Visibility =
            Updates_CheckUpdatesButton.Visibility =
            Visibility.Collapsed;
#endif
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(Properties.Resources.SETTINGS_AUTOIMPORTDESCRIPTION);
            foreach (var path in App.Config.AutoImportPaths)
                stringBuilder.AppendLine(path);
            General_AutoImportTextBlock.Text = stringBuilder.ToString();
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
                    case (int)LanguageCombo.Automatic:
                        App.Config.Language = "automatic";
                        break;
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
                    case (int)LanguageCombo.Danish:
                        App.Config.Language = "da";
                        break;
                    case (int)LanguageCombo.Arabic:
                        App.Config.Language = "ar";
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
            if (result == MessageBoxResult.Yes) window.Library.Nuke();
        }

        private void RestartNowButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            WinForms.Application.Restart();
        }

        private async void Updates_CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            await new UpdateHandler(window.NotificationHandler).UpdateApp(forceUpdate:true);
            InitFields();
        }

        private async void Maintenence_UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                var tracks = window.Library.Read().Select(x => x.Path).Distinct();
                Dispatcher.Invoke(() => window.Library.Nuke(false));
                window.Library.Import(tracks.ToArray());
            });
        }

        private void General_AddFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new WinForms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == WinForms.DialogResult.OK)
                    App.Config.AutoImportPaths.Add(dialog.SelectedPath);
            }
            InitFields();
        }
        private void General_ClearButton_Click(object sender, RoutedEventArgs e)
        {
            App.Config.AutoImportPaths.Clear();
            InitFields();
        }
    }
    public enum LanguageCombo
    {
        Automatic,
        English,
        German,
        Vietnamese,
        Portuguese,
        Turkish,
        Dutch,
        Danish,
        Arabic
    }
    public enum UpdateCombo
    {
        Prompt,
        Manual,
        Automatic
    }
}
