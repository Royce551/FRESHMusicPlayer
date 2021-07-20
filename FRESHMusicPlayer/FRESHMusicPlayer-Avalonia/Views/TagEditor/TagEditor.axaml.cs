using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels.TagEditor;
using FRESHMusicPlayer.Handlers;

namespace FRESHMusicPlayer.Views.TagEditor
{
    public partial class TagEditor : Window
    {
        public TagEditor()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public TagEditor SetStuff(Player player = null, Library library = null)
        {
            var dataContext = DataContext as TagEditorViewModel;
            dataContext.Player = player;
            dataContext.Library = library;
            dataContext.Window = this;
            return this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
