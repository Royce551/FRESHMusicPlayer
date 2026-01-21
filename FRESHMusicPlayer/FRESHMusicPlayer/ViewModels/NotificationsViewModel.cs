using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels
{
    public class NotificationsViewModel : ViewModelBase
    {
        public NotificationsViewModel(MainViewModel mainView)
        {
            MainView = mainView;
        }

        public void ClearAllNotifications()
        {
            MainView.Notifications.Clear();
        }
    }
}
