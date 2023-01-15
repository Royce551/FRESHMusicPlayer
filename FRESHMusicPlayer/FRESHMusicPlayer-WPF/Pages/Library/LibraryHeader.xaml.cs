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

namespace FRESHMusicPlayer.Pages.Library
{
    /// <summary>
    /// Interaction logic for DiscHeader.xaml
    /// </summary>
    public partial class LibraryHeader : UserControl
    {
        private MainWindow window;
        private string header;
        private List<string> containedFilePaths;
        public LibraryHeader(MainWindow window, string header, List<string> containedFilePaths)
        {
            InitializeComponent();
            this.window = window;
            this.header = header;
            this.containedFilePaths = containedFilePaths;
            HeaderText.Text = header;
        }

        private void ShowButtons()
        {
            EnqueueAllButton.Visibility = PlayAllButton.Visibility = Visibility.Visible;

            HeaderText.SetResourceReference(ForegroundProperty, "SecondaryTextColorOverAccent");
            Border.SetResourceReference(ForegroundProperty, "SecondaryTextColorOverAccent");
        }

        private void HideButtons()
        {
            EnqueueAllButton.Visibility = PlayAllButton.Visibility = Visibility.Collapsed;

            HeaderText.SetResourceReference(ForegroundProperty, "SecondaryTextColor");
            Border.SetResourceReference(ForegroundProperty, "SecondaryTextColor");
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e) => ShowButtons();

        private void Border_MouseLeave(object sender, MouseEventArgs e) => HideButtons();

        private void EnqueueAllButton_Click(object sender, RoutedEventArgs e)
        {
            window.Player.Queue.Add(containedFilePaths.ToArray());
        }

        private async void PlayAllButton_Click(object sender, RoutedEventArgs e)
        {
            window.Player.Queue.Clear();
            window.Player.Queue.Add(containedFilePaths.ToArray());
            await window.Player.PlayAsync();
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (window.CurrentTab == Tab.Artists) window.ChangeTabs(Tab.Albums, header);
        }
    }
}
