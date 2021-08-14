using ReactiveUI;

namespace FRESHMusicPlayer.ViewModels
{
    public class TextEntryBoxViewModel : ViewModelBase
    {
        private string header;
        public string Header
        {
            get => header;
            set => this.RaiseAndSetIfChanged(ref header, value);
        }

        private string text;
        public string Text
        {
            get => text;
            set => this.RaiseAndSetIfChanged(ref text, value);
        }
    }
}
