using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            
        }

        public override void AfterPageLoaded()
        {
            base.AfterPageLoaded();
            Debug.WriteLine(MainView.Config.AutoQueue);
        }

        public override void OnNavigatingAway()
        {
            base.OnNavigatingAway();
        }
    }

    public partial class SettingsItem : ObservableRecipient
    {
        private readonly ViewModelBase viewModel;

        public SettingsItem(ViewModelBase viewModel)
        {
            this.viewModel = viewModel;
        }
    }
}
