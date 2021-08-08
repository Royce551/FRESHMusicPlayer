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
        public string Title => MainWindowViewModel.ProjectName;

        private string content;
        public string Content
        {
            get => content;
            set => this.RaiseAndSetIfChanged(ref content, value);
        }

        private bool hasOK = true;
        public bool HasOK
        {
            get => hasOK;
            set => this.RaiseAndSetIfChanged(ref hasOK, value);
        }

        private bool hasYes = false;
        public bool HasYes
        {
            get => hasYes;
            set => this.RaiseAndSetIfChanged(ref hasYes, value);
        }

        private bool hasNo = false;
        public bool HasNo
        {
            get => hasNo;
            set => this.RaiseAndSetIfChanged(ref hasNo, value);
        }

        private bool hasCancel = false;
        public bool HasCancel
        {
            get => hasCancel;
            set => this.RaiseAndSetIfChanged(ref hasCancel, value);
        }
    }
}
