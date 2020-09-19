using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Utilities
{
    public class Utils
    {
        public static string TruncateBytes(string str, int bytes) // TODO: move this validation to FMP Core instead of frontend
        {
            if (Encoding.UTF8.GetByteCount(str) <= bytes) return str;
            int i = 0;
            while (true)
            {
                if (Encoding.UTF8.GetByteCount(str.Substring(0, i)) > bytes) return str.Substring(0, i);
                i++;
            }
        }
    }
}
