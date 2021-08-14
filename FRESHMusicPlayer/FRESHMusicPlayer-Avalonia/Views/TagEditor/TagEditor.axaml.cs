using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.ViewModels.TagEditor;
using System.Collections.Generic;
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

        public TagEditor SetInitialFiles(List<string> initialFiles)
        {
            ViewModel.Initialize(initialFiles);
            return this;
        }

        private async void OnClosing(object sender, CancelEventArgs e)
        {
            if (ViewModel.UnsavedChanges)
            {
                e.Cancel = true;
                var messageBox = new MessageBox().SetStuff($"Save changes to {string.Join(", ", ViewModel.FilePaths)}?", false, true, true, true);
                await messageBox.ShowDialog(this);
                if (messageBox.Yes)
                {
                    ViewModel.SaveCommand();
                    e.Cancel = false;
                    Close();
                }
                else if (messageBox.No)
                {
                    e.Cancel = false;
                    ViewModel.UnsavedChanges = false; // this is a lie, but it's ok because it's gonna close anyway :)
                    Close();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
