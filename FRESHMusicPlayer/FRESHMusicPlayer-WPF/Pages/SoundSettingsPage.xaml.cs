using FRESHMusicPlayer.Backends;
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

namespace FRESHMusicPlayer.Pages
{
    /// <summary>
    /// Interaction logic for SoundSettingsPage.xaml
    /// </summary>
    public partial class SoundSettingsPage : UserControl
    {
        private readonly MainWindow window;
        public SoundSettingsPage(MainWindow window)
        {
            this.window = window;
            InitializeComponent();
        }

        private void UpdateEQValues()
        {
            if (window.Player.CurrentBackend is ISupportEqualization equalizableBackend)
            {
                Dispatcher.Invoke(() =>
                {
                    equalizableBackend.EqualizerBands = new List<EqualizerBand>()
                {
                    new EqualizerBand { Bandwidth = 0.8f, Frequency = 60, Gain = (float)EQBand1.Value },
                    new EqualizerBand { Bandwidth = 0.8f, Frequency = 150, Gain = (float)EQBand2.Value },
                    new EqualizerBand { Bandwidth = 0.8f, Frequency = 400, Gain = (float)EQBand3.Value },
                    new EqualizerBand { Bandwidth = 0.8f, Frequency = 1000, Gain = (float)EQBand4.Value },
                    new EqualizerBand { Bandwidth = 0.8f, Frequency = 2400, Gain = (float)EQBand5.Value },
                    new EqualizerBand { Bandwidth = 0.8f, Frequency = 15000, Gain = (float)EQBand6.Value },
                };
                    equalizableBackend.UpdateEqualizer();
                }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
               
            }
        }

        private void EQBand1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            EQBand1.ToolTip = $"{EQBand1.Value}dB";
            UpdateEQValues();
        }

        private void EQBand2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            EQBand2.ToolTip = $"{EQBand2.Value}dB";
            UpdateEQValues();
        }

        
        private void EQBand3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            EQBand3.ToolTip = $"{EQBand3.Value}dB";
            UpdateEQValues();
        }

        private void EQBand4_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            EQBand4.ToolTip = $"{EQBand4.Value}dB";
            UpdateEQValues();
        }

        private void EQBand5_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            EQBand5.ToolTip = $"{EQBand5.Value}dB";
            UpdateEQValues();
        }

        private void EQBand6_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            EQBand6.ToolTip = $"{EQBand6.Value}dB";
            UpdateEQValues();
        }
    }
}
