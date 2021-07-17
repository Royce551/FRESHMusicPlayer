using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
