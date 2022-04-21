using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels;

namespace FRESHMusicPlayer.Views
{
    public partial class ImportTab : UserControl
    {
        public ImportTab()
        {
            InitializeComponent();
        }

        public ImportTab SetStuff(MainWindowViewModel MainWindow)
        {
            var viewModel = DataContext as ImportTabViewModel;
            viewModel.Window = MainWindow;
            return this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
