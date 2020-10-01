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
using System.Windows.Shapes;

namespace FRESHMusicPlayer.Forms
{
    /// <summary>
    /// Interaction logic for TextEntryBox.xaml
    /// </summary>
    public partial class FMPTextEntryBox : Window
    {
        public bool OK = false;
        public string Response = "";
        public FMPTextEntryBox(string prompt, string boxstarttext = "")
        {
            InitializeComponent();
            Label.Text = prompt;
            Box.Text = boxstarttext;
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            OK = true;
            Response = Box.Text;
            Close();
        }
    }
}
