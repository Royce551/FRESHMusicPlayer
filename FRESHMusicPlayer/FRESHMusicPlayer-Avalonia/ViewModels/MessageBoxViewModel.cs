using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels
{
    public class MessageBoxViewModel : ViewModelBase
    {
        private string title;
        public string Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        private string content;
        public string Content
        {
            get => content;
            set => this.RaiseAndSetIfChanged(ref content, value);
        }
    }
}
