using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer_Avalonia.ViewModels;

namespace FRESHMusicPlayer_Avalonia.Views
{
    public class MainWindow : Window
    {
        private readonly MainWindowViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            viewModel = new MainWindowViewModel();
            DataContext = viewModel;
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
