using FRESHMusicPlayer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public void ReportIssueCommand() => InterfaceUtils.OpenURL(@"https://github.com/Royce551/FRESHMusicPlayer/issues/new");
        public void ViewSourceCodeCommand() => InterfaceUtils.OpenURL(@"https://github.com/Royce551/FRESHMusicPlayer");
        public void ViewLicenseCommand() => InterfaceUtils.OpenURL(@"https://choosealicense.com/licenses/gpl-3.0/");
        public void ViewWebsiteCommand() => InterfaceUtils.OpenURL(@"https://royce551.github.io/FRESHMusicPlayer");
    }
}
