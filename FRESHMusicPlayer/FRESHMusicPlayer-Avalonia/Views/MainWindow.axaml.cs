using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using FRESHMusicPlayer.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;

namespace FRESHMusicPlayer.Views
{
    public class MainWindow : Window
    {
        private MainWindowViewModel ViewModel { get => DataContext as MainWindowViewModel; }
        public Panel RootPanel { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            //DoStuff();
            RootPanel = this.FindControl<Panel>("RootPanel");
            SetValue(DragDrop.AllowDropProperty, true);
            AddHandler(DragDrop.DragEnterEvent, (s, e) => OnDragEnter(s, e));
            AddHandler(DragDrop.DropEvent, (s, e) => OnDragDrop(s, e));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

        }

        public void SetAuxPaneOpened(bool state, bool fromLeft = false)
        {
            var auxPane = this.FindControl<ContentControl>("AuxPane");
            if (state)
            {
                if (fromLeft)
                {
                    DockPanel.SetDock(auxPane, Dock.Left);
                    auxPane.Classes.Remove("closed");
                    auxPane.Classes.Add("openedfromleft");
                    auxPane.Classes.Remove("closedfromleft");
                }
                else
                {
                    DockPanel.SetDock(auxPane, Dock.Right);
                    auxPane.Classes.Add("opened");
                    auxPane.Classes.Remove("closed");
                }
            }
            else
            {
                if (auxPane.Classes.Contains("openedfromleft"))
                {
                    auxPane.Classes.Add("closedfromleft");
                    auxPane.Classes.Remove("openedfromleft");
                }
                else
                {
                    auxPane.Classes.Add("closed");
                    auxPane.Classes.Remove("opened");
                }
                
            }
        }


        private void OnClosing(object sender, CancelEventArgs e)
        {
            ViewModel?.CloseThings();
        }

        private void OnInitialized(object sender, EventArgs e)
        {

        }

        private void OpenTrackInfo(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(sender as IVisual).Properties.IsLeftButtonPressed)
                ViewModel.ShowAuxiliaryPane(Pane.TrackInfo, openleft: true);    
        }

        private void ToggleShowRemainingProgress(object sender, PointerPressedEventArgs e) => ViewModel?.ShowRemainingProgressCommand();


        private void OnShowNotificationButtonClick(object sender, RoutedEventArgs e)
        {
            var button = this.FindControl<Button>("NotificationButton");
            button.ContextFlyout.ShowAt(button);
        }

        private void OnNotificationButtonClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button)sender;
            if (cmd.DataContext is Notification x)
            {
                if (x.OnButtonClicked?.Invoke() ?? true) ViewModel?.Notifications.Remove(x);
            }
        }
        private void OnNotificationClick(object sender, PointerPressedEventArgs e)
        {
            var cmd = (StackPanel)sender;
            if (cmd.DataContext is Notification x)
            {
                ViewModel?.Notifications.Remove(x);
            }
        }

        private void OnTracksPressed(object sender, PointerPressedEventArgs e) => ViewModel.ShowTab(Tab.Tracks);
        private void OnArtistsPressed(object sender, PointerPressedEventArgs e) => ViewModel.ShowTab(Tab.Artists);
        private void OnAlbumsPressed(object sender, PointerPressedEventArgs e) => ViewModel.ShowTab(Tab.Albums);
        private void OnPlaylistsPressed(object sender, PointerPressedEventArgs e) => ViewModel.ShowTab(Tab.Playlists);
        private void OnImportPressed(object sender, PointerPressedEventArgs e) => ViewModel.ShowTab(Tab.Import);

        private void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            ViewModel.Volume += ((float)((e.Delta.Y) / 100) * 3);
        }

        private async void OnKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Source is not TextBox && e.Source is not ListBoxItem) || e.KeyModifiers.HasFlag(KeyModifiers.Control))
                switch (e.Key)
                {
                    case Key.Q:
                        ViewModel.OpenSettingsCommand();
                        break;
                    case Key.A:
                        ViewModel.ShowTab(Tab.Tracks);
                        break;
                    case Key.S:
                        ViewModel.ShowTab(Tab.Artists);
                        break;
                    case Key.D:
                        ViewModel.ShowTab(Tab.Albums);
                        break;
                    case Key.F:
                        ViewModel.ShowTab(Tab.Playlists);
                        break;
                    case Key.G:
                        ViewModel.ShowTab(Tab.Import);
                        break;
                    case Key.E:
                        ViewModel.OpenSearchCommand();
                        break;
                    case Key.R:
                        ViewModel.ShowAuxiliaryPane(Pane.TrackInfo, openleft: true);
                        break;
                    case Key.W:
                        ViewModel.OpenQueueManagementCommand();
                        break;
                    case Key.Space:
                        ViewModel.PlayPauseCommand();
                        break;
                }
            switch (e.Key)
            {
                case Key.OemTilde:
                    var dialog = new TextEntryBox().SetStuff(Properties.Resources.FilePathOrUrl);
                    await dialog.ShowDialog(this);
                    if (dialog.OK) await ViewModel.Player.PlayAsync((dialog.DataContext as TextEntryBoxViewModel).Text);
                    break;
                case Key.F1:
                    InterfaceUtils.OpenURL("https://royce551.github.io/FRESHMusicPlayer/docs/index.html");
                    break;
                case Key.F2:
                    GC.Collect(2);
                    break;
                case Key.F3:
                    throw new Exception("Exception for debugging");
                case Key.F4:
                    Topmost = !Topmost;
                    break;
                case Key.F6:
                    ViewModel.Notifications.Add(new()
                    {
                        ContentText = "Catgirls are cute catgirls are cute catgirls are cute text wrap test text wrap test",
                        ButtonText = "Testing 1 2 3!",
                        DisplayAsToast = true,
                        OnButtonClicked = () =>
                        {
                            new MessageBox().SetStuff("wtf, that actually worked?").ShowDialog(this);
                            return true;
                        }
                    });
                    break;
            }
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            e.DragEffects &= DragDropEffects.Copy;
        }
        private async void OnDragDrop(object sender, DragEventArgs e)
        {
            var paths = e.Data.GetFileNames().ToArray();
            ViewModel.Library.Import(paths);
            ViewModel.Player.Queue.Add(paths);
            await ViewModel.Player.PlayAsync();
        }
    }


}
