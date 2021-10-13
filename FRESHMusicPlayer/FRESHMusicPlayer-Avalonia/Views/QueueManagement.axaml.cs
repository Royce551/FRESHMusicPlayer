using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.ViewModels;
using System;
using System.ComponentModel;
using System.Timers;

namespace FRESHMusicPlayer.Views
{
    public class QueueManagement : Window
    {
        private QueueManagementViewModel ViewModel { get => DataContext as QueueManagementViewModel ?? throw new InvalidCastException(); }

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
            var context = DataContext as QueueManagementViewModel ?? throw new InvalidCastException();
            context.Player = player;
            context.Library = library;
            context.ProgressTimer = progressTimer;
            context.StartThings();
            return this;
        }

        private void OnJumpToButtonClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button)sender;
            if (cmd.DataContext is QueueManagementEntry x)
            {
                ViewModel?.JumpToCommand(x.Position);
            }
        }

        private void OnRemoveButtonClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button)sender;
            if (cmd.DataContext is QueueManagementEntry x)
            {
                ViewModel?.RemoveCommand(x.Position);
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            ViewModel?.CloseThings();
        }
    }
}
