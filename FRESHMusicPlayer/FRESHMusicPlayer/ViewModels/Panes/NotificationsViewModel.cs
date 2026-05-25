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
            MainView.Notifications.CollectionChanged += Notifications_CollectionChanged;
        }

        public override void OnNavigatingAway()
        {
            MainView.Notifications.CollectionChanged -= Notifications_CollectionChanged;
        }

        private async void Notifications_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (MainView.Notifications.Count <= 0) await MainView.OpenSidePaneAsync("FRESHMusicPlayer.Notifications", 300);
        }

        public void ClearAllNotifications()
        {
            MainView.Notifications.Clear();
        }
    }
}
