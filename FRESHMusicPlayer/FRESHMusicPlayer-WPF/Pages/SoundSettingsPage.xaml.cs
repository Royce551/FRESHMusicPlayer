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
        private bool isInitialized = false;
        public SoundSettingsPage(MainWindow window)
        {
            this.window = window;
            InitializeComponent();
            EQBand1.Value = App.Config.EQBand1;
            EQBand2.Value = App.Config.EQBand2;
            EQBand3.Value = App.Config.EQBand3;
            EQBand4.Value = App.Config.EQBand4;
            EQBand5.Value = App.Config.EQBand5;
            EQBand6.Value = App.Config.EQBand6;
            isInitialized = true;
        }

        private void UpdateEQValues()
        {
            if (window.Player.CurrentBackend is ISupportEqualization equalizableBackend)
            {
                App.Config.EQBand1 = (float)EQBand1.Value;
                App.Config.EQBand2 = (float)EQBand2.Value;
                App.Config.EQBand3 = (float)EQBand3.Value;
                App.Config.EQBand4 = (float)EQBand4.Value;
                App.Config.EQBand5 = (float)EQBand5.Value;
                App.Config.EQBand6 = (float)EQBand6.Value;
                window.UpdateEqualizer();
            }
        }

        private void EQBand1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitialized) return;
            EQBand1.ToolTip = $"{EQBand1.Value}dB";
            UpdateEQValues();
        }

        private void EQBand2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitialized) return;
            EQBand2.ToolTip = $"{EQBand2.Value}dB";
            UpdateEQValues();
        }
   
        private void EQBand3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitialized) return;
            EQBand3.ToolTip = $"{EQBand3.Value}dB";
            UpdateEQValues();
        }

        private void EQBand4_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitialized) return;
            EQBand4.ToolTip = $"{EQBand4.Value}dB";
            UpdateEQValues();
        }

        private void EQBand5_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitialized) return;
            EQBand5.ToolTip = $"{EQBand5.Value}dB";
            UpdateEQValues();
        }

        private void EQBand6_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isInitialized) return;
            EQBand6.ToolTip = $"{EQBand6.Value}dB";
            UpdateEQValues();
        }

        private void EQResetButton_Click(object sender, RoutedEventArgs e)
        {
            EQBand1.Value = 0;
            EQBand2.Value = 0;
            EQBand3.Value = 0;
            EQBand4.Value = 0;
            EQBand5.Value = 0;
            EQBand6.Value = 0;
            UpdateEQValues();
        }

        private void EQPresetBassBoostButton_Click(object sender, RoutedEventArgs e)
        {
            EQBand1.Value = 3.5;
            EQBand2.Value = 3;
            EQBand3.Value = 1;
            EQBand4.Value = 0;
            EQBand5.Value = 0;
            EQBand6.Value = 0;
            UpdateEQValues();
        }

        private void EQPresetTrebleBoostButton_Click(object sender, RoutedEventArgs e)
        {
            EQBand1.Value = 0;
            EQBand2.Value = 0;
            EQBand3.Value = 0;
            EQBand4.Value = 1;
            EQBand5.Value = 1.5;
            EQBand6.Value = 3;
            UpdateEQValues();
        }
    }
}
