using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels;

namespace FRESHMusicPlayer.Views
{
    public class MessageBox : Window
    {
        public MessageBox()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            
        }

        public MessageBox SetStuff(string title, string content)
        {
            var context = DataContext as MessageBoxViewModel;
            context.Title = title;
            context.Content = content;
            return this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
