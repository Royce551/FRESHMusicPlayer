using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels;
using System.ComponentModel;

namespace FRESHMusicPlayer.Views
{
    public partial class TrackInfo : Window
    {
        private TrackInfoViewModel ViewModel { get => DataContext as TrackInfoViewModel; }

        public TrackInfo()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public TrackInfo SetStuff(Player player)
        {
            ViewModel.Player = player;
            ViewModel.StartThings();
            return this;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            ViewModel?.CloseThings();
        }
    }
}
