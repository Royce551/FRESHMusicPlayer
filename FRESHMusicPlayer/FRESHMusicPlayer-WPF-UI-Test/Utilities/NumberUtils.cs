using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Utilities
{
    class NumberUtils
    {
        public static string Format(int secs)
        {
            int hours = 0;
            int mins = 0;

            while (secs >= 60)
            {
                mins++;
                secs -= 60;
            }

            while (mins >= 60)
            {
                hours++;
                mins -= 60;
            }

            string hourStr = hours.ToString(); if (hourStr.Length < 2) hourStr = "0" + hourStr;
            string minStr = mins.ToString(); if (minStr.Length < 2) minStr = "0" + minStr;
            string secStr = secs.ToString(); if (secStr.Length < 2) secStr = "0" + secStr;

            string durStr = "";
            if (hourStr != "00") durStr += hourStr + ":";
            durStr = minStr + ":" + secStr;

            return durStr;
        }
    }
}
