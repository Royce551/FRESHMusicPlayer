using ATL.Playlist;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using LiteDB;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;

namespace FRESHMusicPlayer.Pages
{
    /// <summary>
    /// Interaction logic for ImportPage.xaml
    /// </summary>
    public partial class ImportPage : UserControl
    {
        private readonly MainWindow window;
        public ImportPage(MainWindow window)
        {
            this.window = window;
            InitializeComponent();
        }

        private async void BrowseTracksButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Audio Files|*.wav;*.aiff;*.mp3;*.wma;*.3g2;*.3gp;*.3gp2;*.3gpp;*.asf;*.wmv;*.aac;*.adts;*.avi;*.m4a;*.m4a;*.m4v;*.mov;*.mp4;*.sami;*.smi;*.flac|Other|*";
            if (dialog.ShowDialog() == true)
            {
                window.Player.Queue.Add(dialog.FileName);
                await window.Library.ImportAsync(dialog.FileName);
                await window.Player.PlayAsync();
            }
        }

        private async void BrowsePlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Playlist Files|*.xspf;*.asx;*.wax;*.wvx;*.b4s;*.m3u;*.m3u8;*.pls;*.smil;*.smi;*.zpl;";
            if (dialog.ShowDialog() == true)
            {
                var paths = new List<string>();
                IPlaylistIO reader = PlaylistIOFactory.GetInstance().GetPlaylistIO(dialog.FileName);
                foreach (string s in reader.FilePaths)
                {
                    if (!File.Exists(s))
                    {
                        window.NotificationHandler.Add(new Notification
                        {
                            ContentText = string.Format(Properties.Resources.NOTIFICATION_COULD_NOT_IMPORT_PLAYLIST, s),
                            IsImportant = true,
                            DisplayAsToast = true,
                            Type = NotificationType.Failure
                        });
                        continue;
                    }
                }
                window.Player.Queue.Add(reader.FilePaths.ToArray());
                await window.Library.ImportAsync(reader.FilePaths.ToArray());
                await window.Player.PlayAsync();
            }
        }

        private async void BrowseFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new WinForms.FolderBrowserDialog()) 
            {
                dialog.Description = "Note: This doesn't import everything FMP actually supports. If you need to import more obscure file formats, try drag and drop.";
                if (dialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    string[] paths = Directory.EnumerateFiles(dialog.SelectedPath, "*", SearchOption.AllDirectories)
                    .Where(name => name.EndsWith(".mp3")
                        || name.EndsWith(".wav") || name.EndsWith(".m4a") || name.EndsWith(".ogg")
                        || name.EndsWith(".flac") || name.EndsWith(".aiff")
                        || name.EndsWith(".wma")
                        || name.EndsWith(".aac")).ToArray();
                    window.Player.Queue.Add(paths);
                    await window.Library.ImportAsync(paths);
                    await window.Player.PlayAsync();
                }
            }
        }

        private void Page_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void Page_Drop(object sender, DragEventArgs e)
        {
            InterfaceUtils.DoDragDrop((string[])e.Data.GetData(DataFormats.FileDrop), window.Player, window.Library, clearqueue: false, enqueue: false);        
        }

        private async void TextBoxButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FilePathBox.Text)) return;
            window.Player.Queue.Add(FilePathBox.Text);
            await window.Library.ImportAsync(FilePathBox.Text);
            await window.Player.PlayAsync();
        }
    }
}
