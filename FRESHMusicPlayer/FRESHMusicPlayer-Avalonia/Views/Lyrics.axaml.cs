using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels;
using System.ComponentModel;

namespace FRESHMusicPlayer.Views
{
    public partial class Lyrics : UserControl
    {
        public Lyrics()
        {
            InitializeComponent();
            DetachedFromLogicalTree += OnClosing;
        }

        public Lyrics SetStuff(MainWindowViewModel mainWindow)
        {
            var context = DataContext as LyricsViewModel;
            context.MainWindow = mainWindow;
            context.Initialize();
            return this;
        }

        private void OnClosing(object sender, LogicalTreeAttachmentEventArgs e)
        {
            (DataContext as LyricsViewModel)?.Deinitialize();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
