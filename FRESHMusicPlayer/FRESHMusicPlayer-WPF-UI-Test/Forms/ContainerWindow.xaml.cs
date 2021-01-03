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
    /// Interaction logic for ContainerWindow.xaml
    /// </summary>
    public partial class ContainerWindow : Window
    {
        public ContainerWindow(Uri uri, string title)
        {
            InitializeComponent();
            ContentFrame.Source = uri;
            Title = title;
        }
    }
}
