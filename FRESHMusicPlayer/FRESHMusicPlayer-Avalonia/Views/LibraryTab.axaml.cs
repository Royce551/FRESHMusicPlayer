using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels;

namespace FRESHMusicPlayer.Views
{
    public partial class LibraryTab : UserControl
    {
        public LibraryTab()
        {
            InitializeComponent();
        }

        public LibraryTab SetStuff(MainWindowViewModel mainWindowVm, Tab selectedTab, string initialSearch = null)
        {
            var viewModel = DataContext as LibraryTabViewModel;
            viewModel.MainWindowWm = mainWindowVm;
            viewModel.Initialize(selectedTab, initialSearch);
            return this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
