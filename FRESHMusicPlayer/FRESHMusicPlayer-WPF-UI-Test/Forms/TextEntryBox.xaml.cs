using System.Windows;

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
