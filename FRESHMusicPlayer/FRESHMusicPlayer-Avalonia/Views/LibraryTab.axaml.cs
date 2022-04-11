using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels;

namespace FRESHMusicPlayer.Views
{
    public partial class LibraryTab : UserControl
    {
        private LibraryTabViewModel ViewModel => DataContext as LibraryTabViewModel;

        public LibraryTab()
        {
            InitializeComponent();
        }

        public LibraryTab SetStuff(MainWindowViewModel mainWindowVm, Tab selectedTab, string initialSearch = null)
        {
            ViewModel.MainWindowWm = mainWindowVm;
            ViewModel.Initialize(selectedTab, initialSearch);
            return this;
        }

        private async void OnPlayButtonClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button)sender;
            if (cmd.DataContext is DatabaseTrack x)
            {
                ViewModel.MainWindowWm.Player.Queue.Clear();
                await ViewModel.MainWindowWm.Player.PlayAsync(x.Path);
            }
        }

        private void OnEnqueueButtonClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button)sender;
            if (cmd.DataContext is DatabaseTrack x)
            {
                ViewModel.MainWindowWm.Player.Queue.Add(x.Path);
            }
        }

        private void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button)sender;
            if (cmd.DataContext is DatabaseTrack x)
            {
                ViewModel.MainWindowWm.Library.Remove(x.Path);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
