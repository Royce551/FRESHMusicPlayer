using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels;

namespace FRESHMusicPlayer.Views
{
    public partial class TrackInfo : Window
    {
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
            var viewModel = DataContext as TrackInfoViewModel;
            viewModel.Player = player;
            viewModel.StartStuff();
            return this;
        }
    }
}
