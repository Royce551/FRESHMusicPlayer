using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using FRESHMusicPlayer.Handlers;
using Avalonia.Controls;
using FRESHMusicPlayer.Views;

namespace FRESHMusicPlayer.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public ConfigurationFile Config;
        public Library Library;

        public bool IsRunningOnLinux { get => RuntimeInformation.IsOSPlatform(OSPlatform.Linux); }
        public bool IsRunningOnMac { get => RuntimeInformation.IsOSPlatform(OSPlatform.OSX); }

        public ObservableCollection<DisplayLanguage> AvailableLanguages { get; } = new()
        {
            new("Automatic", "automatic"),
            new("English", "en"),
            new("German", "de"),
            new("Vietnamese", "vi"),
            new("Arabic (Saudi Arabia)", "ar")
        };

        public string Version => MainWindowViewModel.ProjectName;

        public DisplayLanguage Language
        {
            get
            {
                if (Config is not null)
                    return AvailableLanguages.First(x => x.Code == Config?.Language);
                else return AvailableLanguages[0];
            }
            set
            {
                Config.Language = value.Code;
                CheckRestartNeeded();
            }
        }

        public string AutoImportText
        {
            get
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(Properties.Resources.Settings_AutoImport_Info);
                if (Config.AutoImportPaths is not null)
                {
                    foreach (var path in Config.AutoImportPaths)
                        stringBuilder.AppendLine(path);
                }
                return stringBuilder.ToString();
            }
        }

        private bool isRestartNeeded = false;
        public bool IsRestartNeeded
        {
            get => isRestartNeeded;
            set => this.RaiseAndSetIfChanged(ref isRestartNeeded, value);
        }

        public bool PlaytimeLogging
        {
            get => Config?.PlaybackTracking ?? false;
            set
            {
                Config.PlaybackTracking = value;
            }
        }
        public bool ShowTimeInWindow
        {
            get => Config?.ShowTimeInWindow ?? false;
            set => Config.ShowTimeInWindow = value;
        }
        public bool IntegrateDiscordRPC
        {
            get => Config?.IntegrateDiscordRPC ?? false;
            set
            {
                Config.IntegrateDiscordRPC = value;
                CheckRestartNeeded();
            }
        }
        public bool IntegrateMPRIS
        {
            get => Config?.IntegrateMPRIS ?? false;
            set
            {
                Config.IntegrateMPRIS = value;
                CheckRestartNeeded();
            }
        }
        public bool MPRISShowCoverArt
        {
            get => Config?.MPRISShowCoverArt ?? false;
            set
            {
                Config.MPRISShowCoverArt = value;
            }
        }
        public bool CheckForUpdates
        {
            get => Config?.CheckForUpdates ?? false;
            set
            {
                Config.CheckForUpdates = value;
            }
        }

        private ConfigurationFile workingConfig;

        private Window Window
        {
            get
            {
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    return desktop.MainWindow;
                else return null;
            }
        }

        public SettingsViewModel()
        {
            
        }

        public void StartThings()
        {
            workingConfig = new ConfigurationFile { Language = Config.Language, IntegrateDiscordRPC = Config.IntegrateDiscordRPC, IntegrateMPRIS = Config.IntegrateMPRIS }; // Copy of config that's compared to the actual config file to set AppRestartNeeded.
            this.RaisePropertyChanged(nameof(Language));
            this.RaisePropertyChanged(nameof(AutoImportText));
            this.RaisePropertyChanged(nameof(PlaytimeLogging));
            this.RaisePropertyChanged(nameof(ShowTimeInWindow));
            this.RaisePropertyChanged(nameof(IntegrateDiscordRPC));
            this.RaisePropertyChanged(nameof(IntegrateMPRIS));
            this.RaisePropertyChanged(nameof(MPRISShowCoverArt));
            this.RaisePropertyChanged(nameof(CheckForUpdates));
            this.RaisePropertyChanged(nameof(IsRestartNeeded));
        }

        public void ReportIssueCommand() => InterfaceUtils.OpenURL(@"https://github.com/Royce551/FRESHMusicPlayer/issues/new");
        public void ViewSourceCodeCommand() => InterfaceUtils.OpenURL(@"https://github.com/Royce551/FRESHMusicPlayer");
        public void ViewLicenseCommand() => InterfaceUtils.OpenURL(@"https://choosealicense.com/licenses/gpl-3.0/");
        public void ViewWebsiteCommand() => InterfaceUtils.OpenURL(@"https://royce551.github.io/FRESHMusicPlayer");

        public void RestartCommand()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifeTime)
            {
                lifeTime.Shutdown();
                Process.Start(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "FRESHMusicPlayer"));
            }
        }

        public async void AddFolderCommand()
        {
            var dialog = new OpenFolderDialog();
            var directory = await dialog.ShowAsync(Window);
            if (directory is not null)
                Config.AutoImportPaths.Add(directory);
            this.RaisePropertyChanged(nameof(AutoImportText));
        }
        public void ClearAllCommand()
        {
            Config.AutoImportPaths.Clear();
            this.RaisePropertyChanged(nameof(AutoImportText));
        }

        public void ResetSettingsCommand()
        {
            var mainWindow = GetMainWindow().DataContext as MainWindowViewModel; // little messy, maybe figure out how to make this cleaner
            mainWindow.Config = new ConfigurationFile();
            Program.Config = mainWindow.Config;
            Config = mainWindow.Config;
            StartThings();
        }
        public async void CleanLibraryCommand()
        {
            await Task.Run(() =>
            {
                var tracks = Library.Read().Select(x => x.Path).Distinct();
                Library.Nuke(false);
                Library.Import(tracks.ToArray());
            });
        }
        public async void NukeLibraryCommand()
        {
            var messageBox = new MessageBox().SetStuff("You are about to irreversibly clear your library.", true, false, false, true);
            await messageBox.ShowDialog(Window);
            if (messageBox.OK) Library.Nuke();
        }

        private void CheckRestartNeeded()
        {
            if (workingConfig.Language == Config.Language && workingConfig.IntegrateDiscordRPC == Config.IntegrateDiscordRPC && workingConfig.IntegrateMPRIS == Config.IntegrateMPRIS) IsRestartNeeded = false;
            else IsRestartNeeded = true;
        }
    }

    public class DisplayLanguage
    {
        public string Name { get; }
        public string Code { get; }

        public DisplayLanguage(string name, string code)
        {
            Name = name;
            Code = code;
        }
    }
}
