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

namespace FRESHMusicPlayer.Forms.TagEditor.Integrations
{
    /// <summary>
    /// Interaction logic for IntegrationDisambiguation.xaml
    /// </summary>
    public partial class IntegrationDisambiguation : Window
    {
        public bool OK { get; set; } = false;
        public int SelectedIndex { get; set; } = 0;

        public IntegrationDisambiguation(List<(string Name, string Id)> releases)
        {
            InitializeComponent();
            foreach (var release in releases)
            {
                ReleaseBox.Items.Add($"{release.Name} - {release.Id}");
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReleaseBox.SelectedIndex != -1)
            {
                SelectedIndex = ReleaseBox.SelectedIndex;
                OK = true;
            }
            Close();
        }
    }
}
