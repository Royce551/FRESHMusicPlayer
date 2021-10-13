using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.ViewModels;
using System;

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

        public Settings SetThings(Library library)
        {
            var viewModel = DataContext as SettingsViewModel ?? throw new InvalidCastException();
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
