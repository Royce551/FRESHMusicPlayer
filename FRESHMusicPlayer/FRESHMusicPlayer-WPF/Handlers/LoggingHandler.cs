using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers
{
    public class LoggingHandler
    {
        [Conditional("DEBUG")]
        public static void Log(string message)
        {
#if DEBUG
            var line = $"[{DateTime.Now:T}] {message}";
            Console.WriteLine(line);
            Console.ResetColor();
            var logFilePath = "log.txt";
            using (var sw = File.AppendText(logFilePath))
            {
                sw.WriteLine(line);
            }
#endif
        }
    }
}
