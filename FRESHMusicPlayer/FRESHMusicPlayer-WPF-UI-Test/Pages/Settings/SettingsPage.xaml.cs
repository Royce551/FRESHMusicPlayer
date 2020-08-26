using FRESHMusicPlayer;
using FRESHMusicPlayer.Handlers.Configuration;
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

namespace FRESHMusicPlayer_WPF_UI_Test.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
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
            if (App.Config.ControlBoxPosition == Dock.Top) ControlAppearance_ControlPosCheck.IsChecked = true;
            else ControlAppearance_ControlPosCheck.IsChecked = false;
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
            App.Config.IntegrateDiscordRPC = (bool)Integration_DiscordRPCCheck.IsChecked;
            ConfigurationHandler.Write(App.Config);
        }

        private void Integration_SMTCChanged(object sender, RoutedEventArgs e)
        {
            App.Config.IntegrateSMTC = (bool)Integration_SMTCCheck.IsChecked;
            ConfigurationHandler.Write(App.Config);
        }

        private void ControlAppearance_ControlPosChanged(object sender, RoutedEventArgs e)
        {
            if ((bool)ControlAppearance_ControlPosCheck.IsChecked) App.Config.ControlBoxPosition = Dock.Top;
            else App.Config.ControlBoxPosition = Dock.Bottom;
            ConfigurationHandler.Write(App.Config);
        }
    }
}
