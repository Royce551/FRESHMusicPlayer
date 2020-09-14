using FRESHMusicPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ATL;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using System.Threading;
using System.ComponentModel;
using System.Windows.Shell;
using System.Diagnostics;
using LiteDB;
using FRESHMusicPlayer.Utilities;

namespace FRESHMusicPlayer.Pages.Library
{
    /// <summary>
    /// Interaction logic for LibraryPage.xaml
    /// </summary>
    public partial class LibraryPage : Page
    {
        public LibraryPage()
        {
            InitializeComponent();
            LoadLibrary();
            MainWindow.TabChanged += MainWindow_TabChanged;
        }

        public void LoadLibrary()
        {
            switch (MainWindow.SelectedMenu) // all of this stuff is here so that i can avoid copying and pasting the same page thrice, maybe there's a better way?
            {
                case SelectedMenus.Tracks:
                    ShowTracks();
                    break;
                case SelectedMenus.Artists:
                    ShowArtists();
                    break;
                case SelectedMenus.Albums:
                    ShowAlbums();
                    break;
            }
        }
        public async void ShowTracks()
        {
            LeftSide.Width = new GridLength(0);
            int length = 0;
            await Task.Run(() =>
            {          
                foreach (var thing in DatabaseUtils.Read())
                {
                    Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, thing.Title)));
                    length += thing.Length;
                }        
            });
            InfoLabel.Text = $"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {new TimeSpan(0, 0, 0, length):hh\\:mm\\:ss}";
        }
        public async void ShowArtists()
        {
            await Task.Run(() =>
            {
                foreach (var thing in DatabaseUtils.Read("Artist"))
                {
                    if (CategoryPanel.Items.Contains(thing.Artist)) continue;
                    Dispatcher.Invoke(() => CategoryPanel.Items.Add(thing.Artist));
                }
            });
        }
        public async void ShowAlbums()
        {
            await Task.Run(() =>
            {
                    foreach (var thing in DatabaseUtils.Read("Album"))
                    {
                        if (CategoryPanel.Items.Contains(thing.Album)) continue;
                        Dispatcher.Invoke(() => CategoryPanel.Items.Add(thing.Album));
                    }
            });
        }
        public async void ShowTracksforArtist(string selectedItem)
        {
            TracksPanel.Items.Clear();
            int length = 0;
            await Task.Run(() =>
            {
                foreach (var thing in DatabaseUtils.ReadTracksForArtist(selectedItem))
                {
                    Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, thing.Title)));
                    length += thing.Length;
                }
            });
            InfoLabel.Text = $"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {new TimeSpan(0, 0, 0, length):hh\\:mm\\:ss}";
        }
        public async void ShowTracksforAlbum(string selectedItem)
        {
            TracksPanel.Items.Clear();
            int length = 0;
            await Task.Run(() =>
            {
                foreach (var thing in DatabaseUtils.ReadTracksForAlbum(selectedItem))
                {
                    Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, thing.Title)));
                    length += thing.Length;
                }
            });
            InfoLabel.Text = $"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {new TimeSpan(0, 0, 0, length):hh\\:mm\\:ss}";
        }

        private void CategoryPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (string)CategoryPanel.SelectedItem;
            if (selectedItem == null) return;
            if (MainWindow.SelectedMenu == SelectedMenus.Artists) ShowTracksforArtist(selectedItem);
            else ShowTracksforAlbum(selectedItem);
        }
        private void MainWindow_TabChanged(object sender, EventArgs e)
        {
            TracksPanel.Items.Clear();
            CategoryPanel.Items.Clear();
            LeftSide.Width = new GridLength(222);
            DetailsPane.Height = new GridLength(45);
            LoadLibrary();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.TabChanged -= MainWindow_TabChanged;
            CategoryPanel.Items.Clear();
            TracksPanel.Items.Clear();
        }

        private void Page_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private async void Page_Drop(object sender, DragEventArgs e)
        {
            string[] tracks = (string[])e.Data.GetData(DataFormats.FileDrop);
            MainWindow.Player.AddQueue(tracks);
            await Task.Run(() =>
            {
                DatabaseUtils.Import(tracks);
            });
            MainWindow.Player.PlayMusic();
            LoadLibrary();
        }

        private void QueueAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (SongEntry entry in TracksPanel.Items)
            {
                MainWindow.Player.AddQueue(entry.FilePath);
            }
        }

        private void PlayAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (SongEntry entry in TracksPanel.Items)
            {
                MainWindow.Player.AddQueue(entry.FilePath);
            }
            MainWindow.Player.PlayMusic();
        }
    }
}
