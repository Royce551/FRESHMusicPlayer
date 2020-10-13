using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Utilities
{
    public class Utils
    {
        public static IEnumerable<CultureInfo> GetAvailableCultures()
        {
            CultureInfo[] culture = CultureInfo.GetCultures(CultureTypes.AllCultures);
            string exeLocation = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));

            return culture.Where(cultureInfo => Directory.Exists(Path.Combine(exeLocation, cultureInfo.Name)));
        }
    }
}
