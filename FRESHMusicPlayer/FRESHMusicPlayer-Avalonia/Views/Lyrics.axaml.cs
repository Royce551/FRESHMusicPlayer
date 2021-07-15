using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels;
using System.ComponentModel;

namespace FRESHMusicPlayer.Views
{
    public partial class Lyrics : Window
    {
        public Lyrics()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public Lyrics SetStuff(MainWindowViewModel mainWindow)
        {
            var context = DataContext as LyricsViewModel;
            context.MainWindow = mainWindow;
            context.Initialize();
            return this;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            (DataContext as LyricsViewModel)?.Deinitialize();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
