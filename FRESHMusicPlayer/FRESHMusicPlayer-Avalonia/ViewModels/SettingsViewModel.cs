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

namespace FRESHMusicPlayer.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public ConfigurationFile Config;

        public bool IsRunningOnLinux { get => RuntimeInformation.IsOSPlatform(OSPlatform.Linux); }
        public bool IsRunningOnMac { get => RuntimeInformation.IsOSPlatform(OSPlatform.OSX); }

        public ObservableCollection<DisplayLanguage> AvailableLanguages { get; } = new()
        {
            new("Automatic", "automatic"),
            new("Vietnamese", "vi")
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
            set => Config.Language = value.Code;
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
            }
        }
        public bool IntegrateMPRIS
        {
            get => Config?.IntegrateMPRIS ?? false;
            set
            {
                Config.IntegrateMPRIS = value;
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

        public SettingsViewModel()
        {

        }

        public void StartThings()
        {
            this.RaisePropertyChanged(nameof(Language));
            this.RaisePropertyChanged(nameof(PlaytimeLogging));
            this.RaisePropertyChanged(nameof(ShowTimeInWindow));
            this.RaisePropertyChanged(nameof(IntegrateDiscordRPC));
            this.RaisePropertyChanged(nameof(IntegrateMPRIS));
            this.RaisePropertyChanged(nameof(MPRISShowCoverArt));
            this.RaisePropertyChanged(nameof(CheckForUpdates));
        }

        public void ReportIssueCommand() => InterfaceUtils.OpenURL(@"https://github.com/Royce551/FRESHMusicPlayer/issues/new");
        public void ViewSourceCodeCommand() => InterfaceUtils.OpenURL(@"https://github.com/Royce551/FRESHMusicPlayer");
        public void ViewLicenseCommand() => InterfaceUtils.OpenURL(@"https://choosealicense.com/licenses/gpl-3.0/");
        public void ViewWebsiteCommand() => InterfaceUtils.OpenURL(@"https://royce551.github.io/FRESHMusicPlayer");
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
