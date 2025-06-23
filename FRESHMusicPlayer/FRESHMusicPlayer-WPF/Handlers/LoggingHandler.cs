using System;
using System.Diagnostics;
using System.IO;

namespace FRESHMusicPlayer.Handlers
{
    public class LoggingHandler
    {
        [Conditional("DEBUG"), Conditional("BLUEPRINT")]
        public static void Log(string message)
        {
#if DEBUG || BLUEPRINT
            var line = $"[{DateTime.Now:T}] {message}";
            Console.WriteLine(line);
            Debug.WriteLine(line);
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
