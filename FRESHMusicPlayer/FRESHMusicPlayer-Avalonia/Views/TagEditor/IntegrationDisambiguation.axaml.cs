using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels.TagEditor;
using System.Collections.Generic;

namespace FRESHMusicPlayer.Views.TagEditor
{
    public partial class IntegrationDisambiguation : Window
    {
        public bool OK { get; set; } = false;
        public int SelectedItem => ViewModel.SelectedItem;

        private IntegrationDisambiguationViewModel ViewModel => DataContext as IntegrationDisambiguationViewModel;

        public IntegrationDisambiguation()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public IntegrationDisambiguation SetStuff(List<(string Name, string Id)> releases)
        {
            foreach (var release in releases)
            {
                ViewModel.Results.Add($"{release.Name} - {release.Id}");
            }
            ViewModel.Window = this;
            return this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
