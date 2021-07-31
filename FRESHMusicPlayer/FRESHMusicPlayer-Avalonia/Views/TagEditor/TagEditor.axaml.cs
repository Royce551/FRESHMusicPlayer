using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels.TagEditor;
using FRESHMusicPlayer.Handlers;
using System.ComponentModel;

namespace FRESHMusicPlayer.Views.TagEditor
{
    public partial class TagEditor : Window
    {
        private TagEditorViewModel ViewModel => DataContext as TagEditorViewModel;

        public TagEditor()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public TagEditor SetStuff(Player player = null, Library library = null)
        {
            ViewModel.Player = player;
            ViewModel.Library = library;
            ViewModel.Window = this;
            return this;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (ViewModel.UnsavedChanges)
            {

            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
