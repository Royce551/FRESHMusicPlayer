using ATL;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.Handlers.DatabaseIntegrations;
using FRESHMusicPlayer.ViewModels.TagEditor;

namespace FRESHMusicPlayer.Views.TagEditor
{
    public partial class ReleaseIntegrationPage : Window
    {
        public bool OK { get; set; } = false;
        public Track TrackToSave => ViewModel.TrackToSave;

        private ReleaseIntegrationPageViewModel ViewModel => DataContext as ReleaseIntegrationPageViewModel;

        public ReleaseIntegrationPage()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public ReleaseIntegrationPage SetStuff(TagEditorRelease release, Track track)
        {
            ViewModel.InitialTrack = track;
            ViewModel.Release = release;
            ViewModel.Window = this;
            ViewModel.Initialize();
            return this;
        }

        private void OnLinkClicked(object sender, PointerPressedEventArgs e)
        {
            ViewModel.OpenURLCommand();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
