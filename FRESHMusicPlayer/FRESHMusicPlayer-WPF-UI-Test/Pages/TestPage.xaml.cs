using FRESHMusicPlayer.Forms.WPF;
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

namespace FRESHMusicPlayer_WPF_UI_Test.Pages
{
    /// <summary>
    /// Interaction logic for TestPage.xaml
    /// </summary>
    public partial class TestPage : Page
    {
        public TestPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WPFUserInterface.Player.AddQueue(@"C:\Users\poohw\OneDrive\Music\Splatune 2 - Splatoon 2 OST\Disc 2\2-05 Muck Warfare.flac");
            WPFUserInterface.Player.CurrentVolume = .3f;
            WPFUserInterface.Player.PlayMusic();
        }
    }
}
