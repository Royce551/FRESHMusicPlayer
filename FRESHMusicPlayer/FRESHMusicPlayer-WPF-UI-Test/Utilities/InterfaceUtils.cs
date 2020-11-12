using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace FRESHMusicPlayer.Utilities
{
    public class InterfaceUtils
    {
        public static void SetField(TextBlock box, TextBlock label, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                box.Visibility = Visibility.Collapsed;
                label.Visibility = Visibility.Collapsed;
            }
            else box.Text = value;
        }
        public static IEnumerable<CultureInfo> GetAvailableCultures()
        {
            CultureInfo[] culture = CultureInfo.GetCultures(CultureTypes.AllCultures);
            string exeLocation = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));

            return culture.Where(cultureInfo => Directory.Exists(Path.Combine(exeLocation, cultureInfo.Name)));
        }
    }
}
