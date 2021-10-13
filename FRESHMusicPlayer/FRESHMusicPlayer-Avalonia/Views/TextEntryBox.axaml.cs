using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels;
using System;

namespace FRESHMusicPlayer.Views
{
    public partial class TextEntryBox : Window
    {
        public bool OK { get; private set; } = false;
        public string Text => ViewModel.Text;

        private TextEntryBoxViewModel ViewModel => DataContext as TextEntryBoxViewModel ?? throw new InvalidCastException();

        public TextEntryBox()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public TextEntryBox SetStuff(string header, string initialText = "")
        {
            ViewModel.Header = header;
            ViewModel.Text = initialText;
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
    }
}
