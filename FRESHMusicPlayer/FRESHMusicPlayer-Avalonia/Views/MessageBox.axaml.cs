using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels;

namespace FRESHMusicPlayer.Views
{
    public class MessageBox : Window
    {
        private MessageBoxViewModel ViewModel => DataContext as MessageBoxViewModel;

        public bool OK { get; private set; } = false;
        public bool Yes { get; private set; } = false;
        public bool No { get; private set; } = false;
        public bool Cancel { get; private set; } = false;

        public MessageBox()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

        }

        public MessageBox SetStuff(
                                   string content,
                                   bool hasOK = true,
                                   bool hasYes = false,
                                   bool hasNo = false,
                                   bool hasCancel = false)
        {
            ViewModel.Content = content;
            ViewModel.HasOK = hasOK;
            ViewModel.HasYes = hasYes;
            ViewModel.HasNo = hasNo;
            ViewModel.HasCancel = hasCancel;
            return this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            OK = true;
            Close();
        }
        private void OnYesButtonClick(object sender, RoutedEventArgs e)
        {
            Yes = true;
            Close();
        }
        private void OnNoButtonClick(object sender, RoutedEventArgs e)
        {
            No = true;
            Close();
        }
        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Cancel = true;
            Close();
        }
    }
}
