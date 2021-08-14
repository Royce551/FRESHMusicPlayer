using System;
using System.Diagnostics;
using System.IO;

namespace FRESHMusicPlayer.Handlers
{
    public class LoggingHandler
    {
        [Conditional("DEBUG")]
        public static void Log(string message)
        {
#if DEBUG
            try
            {
                var line = $"[{DateTime.Now:T}] {message}";
                Console.WriteLine(line);
                Debug.WriteLine(line);
                Console.ResetColor();
                var logFilePath = "log.txt";
                using (var sw = File.AppendText(logFilePath))
                {
                    sw.WriteLine(line);
                }
            }
            catch (IOException)
            {
                // ignored
            }
#endif
        }
    }
}
