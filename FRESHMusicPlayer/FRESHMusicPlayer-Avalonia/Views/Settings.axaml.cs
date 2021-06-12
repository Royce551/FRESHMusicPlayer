using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.ViewModels;

namespace FRESHMusicPlayer.Views
{
    public class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
    #if DEBUG
            this.AttachDevTools();
    #endif
        }

        public Settings SetThings(ConfigurationFile config)
        {
            var viewModel = DataContext as SettingsViewModel;
            viewModel.Config = config;
            viewModel.StartThings();
            return this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
        }
    }
}
