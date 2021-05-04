using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.ViewModels;
using System.Timers;

namespace FRESHMusicPlayer.Views
{
    public class QueueManagement : Window
    {
        public QueueManagement()
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

        public QueueManagement SetStuff(Player player, Library library, Timer progressTimer)
        {
            var context = DataContext as QueueManagementViewModel;
            context.Player = player;
            context.Library = library;
            context.ProgressTimer = progressTimer;
            context.StartThings();
            return this;
        }
    }
}
