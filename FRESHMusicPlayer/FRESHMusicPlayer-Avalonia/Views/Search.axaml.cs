using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using FRESHMusicPlayer.ViewModels;

namespace FRESHMusicPlayer.Views
{
    public partial class Search : UserControl
    {
        private SearchViewModel ViewModel => DataContext as SearchViewModel;

        public Search()
        {
            InitializeComponent();
            var searchBox = this.FindControl<TextBox>("SearchBox");
            Dispatcher.UIThread.InvokeAsync(() => searchBox.Focus(), DispatcherPriority.ApplicationIdle);
        }

        public Search SetStuff(MainWindowViewModel mainWindow)
        {
            ViewModel.MainWindowWm = mainWindow;
            return this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && ViewModel.ContentItems.Count > 0)
            {
                ViewModel.MainWindowWm.Player.Queue.Clear();
                await ViewModel.MainWindowWm.Player.PlayAsync(ViewModel.ContentItems[0].Path);
            }
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
    }
}
