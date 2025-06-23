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
        public bool IsPassword = false;
        public FMPTextEntryBox(string prompt, string boxstarttext = "", bool isPassword = false)
        {
            InitializeComponent();
            IsPassword = isPassword;
            if (IsPassword)
            {
                PasswordBox.Visibility = Visibility.Visible;
                Box.Visibility = Visibility.Collapsed;
            }
            Label.Text = prompt;
            Box.Text = boxstarttext;
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            OK = true;
            Response = IsPassword ? PasswordBox.Password : Box.Text;
            Close();
        }
    }
}
