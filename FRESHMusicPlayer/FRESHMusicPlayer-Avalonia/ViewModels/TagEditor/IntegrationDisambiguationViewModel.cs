using FRESHMusicPlayer.Views.TagEditor;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace FRESHMusicPlayer.ViewModels.TagEditor
{
    public class IntegrationDisambiguationViewModel : ViewModelBase
    {
        public IntegrationDisambiguation Window { get; set; }

        public ObservableCollection<string> Results { get; set; } = new();
        private int selectedItem;
        public int SelectedItem
        {
            get => selectedItem;
            set => this.RaiseAndSetIfChanged(ref selectedItem, value);
        }

        public void OKCommand()
        {
            Window.OK = true;
            Window.Close();
        }
    }
}
