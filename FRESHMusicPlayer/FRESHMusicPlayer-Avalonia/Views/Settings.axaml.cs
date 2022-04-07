using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.ViewModels;

namespace FRESHMusicPlayer.Views
{
    public class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
        }

        public Settings SetThings(ConfigurationFile config, Library library)
        {
            var viewModel = DataContext as SettingsViewModel;
            viewModel.Config = config;
            viewModel.Library = library;
            viewModel.StartThings();
            return this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

        }
    }
}
