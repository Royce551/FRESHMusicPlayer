using FRESHMusicPlayer.Forms;
using FRESHMusicPlayer.Forms.TagEditor;
using FRESHMusicPlayer.Handlers.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using FRESHMusicPlayer.Handlers;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using WinForms = System.Windows.Forms;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Threading;
using System.Globalization;

namespace FRESHMusicPlayer
{
    public enum Skin
    {
        Light, Dark, Classic, System
    }
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ConfigurationFile Config;
        public static string DataFolderLocation
        {
            get
            {
                if (Directory.Exists("Data")) return "Data";
                else return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer");
            }
        }

        private Window currentWindow;
        private Player player;
        async void App_Startup(object sender, StartupEventArgs e )
        {
            LoggingHandler.Log("Handling configuration...");
            Config = ConfigurationHandler.Read();
            player = new Player { Volume = Config.Volume };
            if (Config.Language != "automatic") Thread.CurrentThread.CurrentUICulture = new CultureInfo(Config.Language);

            ChangeSkin(Config.Theme);
            ChangeAccentColor(Config.AccentColor);

            LoggingHandler.Log("Handling command line args...");

            string[] initialFiles = null;
            if (e.Args.Length > 0)
            {
                initialFiles = e.Args.Where(x => x.Contains('.')).ToArray();

                if (e.Args.Contains("--tageditor"))
                {
                    currentWindow = new TagEditor(initialFiles.ToList(), player);
                    return;
                }
            }

            currentWindow = new MainWindow(player, initialFiles);

            if (Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft)
                currentWindow.FlowDirection = FlowDirection.RightToLeft;
            var persistenceFilePath = Path.Combine(DataFolderLocation, "Configuration", "FMP-WPF", "persistence");
            var startTime = TimeSpan.FromSeconds(0);
            if (File.Exists(persistenceFilePath))
            {
                var fields = File.ReadAllText(persistenceFilePath).Split(';');

                var top = double.Parse(fields[2]);
                var left = double.Parse(fields[3]);
                var height = double.Parse(fields[4]);
                var width = double.Parse(fields[5]);
                var rect = new System.Drawing.Rectangle((int)left, (int)top, (int)width, (int)height);
                if (WinForms.Screen.AllScreens.Any(y => y.WorkingArea.IntersectsWith(rect)))
                {
                    currentWindow.Top = top;
                    currentWindow.Left = left;
                    currentWindow.Height = height;
                    currentWindow.Width = width;
                }
                if (fields[0] != string.Empty)
                {
                    initialFiles = new string[] { fields[0] };
                    startTime = TimeSpan.FromSeconds(int.Parse(fields[1]));
                }
            }

            if (initialFiles != null)
            {
                player.Queue.Add(initialFiles);
                await player.PlayAsync();
                player.CurrentTime = startTime;
                if (currentWindow is MainWindow mainWindow)
                {
                    mainWindow.PlayPauseMethod();
                    mainWindow.ProgressTick();
                }
            }
            currentWindow.Show();
            
        }
        public static Skin CurrentSkin { get; set; } = Skin.Dark;
        public void ChangeSkin(Skin newSkin)
        {
            CurrentSkin = newSkin;

            foreach (ResourceDictionary dict in Resources.MergedDictionaries)
            {

                if (dict is SkinResourceDictionary skinDict)
                    skinDict.UpdateSource();
                else
                    dict.Source = dict.Source;
            }
        }

        public void ChangeAccentColor(AccentColor accentColor)
        {
            byte r1, g1, b1, r2, g2, b2;
            r1 = g1 = b1 = r2 = g2 = b2 = default;
            switch (accentColor)
            {
                case AccentColor.Blue:
                    r1 = 51; g1 = 139; b1 = 193;
                    r2 = 105; g2 = 181; b2 = 120;
                    break;
                case AccentColor.Green:
                    r1 = 105; g1 = 181; b1 = 120;
                    r2 = 51; g2 = 139; b2 = 193;
                    break;
                case AccentColor.Red:
                    r1 = 213; g1 = 70; b1 = 63;
                    r2 = 233; g2 = 119; b2 = 195;
                    break;
                case AccentColor.Purple:
                    r1 = 193; g1 = 96; b1 = 195;
                    r2 = 0; g2 = 162; b2 = 195;
                    break;
                case AccentColor.Pink:
                    r1 = 248; g1 = 104; b1 = 200;
                    r2 = 248; g2 = 195; b2 = 114;
                    break;
                case AccentColor.ClassicBlue:
                    r1 = 4; g1 = 160; b1 = 219;
                    r2 = 119; g2 = 209; b2 = 137;
                    break;
                case AccentColor.CoverArt:
                    if (currentWindow is MainWindow window)
                        window.HandleAccentCoverArt();
                    else
                    {
                        r1 = 51; g1 = 139; b1 = 193;
                        r2 = 105; g2 = 181; b2 = 120;
                    }
                    break;
            }

            ApplyAccentColor(r1, g1, b1, r2, g2, b2);
        }

        public void ApplyAccentColor(byte r1, byte g1, byte b1, byte r2, byte g2, byte b2)
        {
            var accent = FindResource("AccentColor") as SolidColorBrush;
            var accent2 = accent.Clone();

            accent2.Color = Color.FromRgb(r1, g1, b1);
            var gradient = FindResource("AccentGradientColor") as LinearGradientBrush;
            var gradient2 = gradient.Clone();
            gradient2.GradientStops[0].Color = Color.FromRgb(r1, g1, b1);
            gradient2.GradientStops[1].Color = Color.FromRgb(r2, g2, b2);
            Current.Resources["AccentColor"] = accent2;
            Current.Resources["AccentGradientColor"] = gradient2;

            if (accent2.Color.R * 0.2126 + accent2.Color.G * 0.7152 + accent2.Color.B * 0.0722 < 255 / 2)
            {
                Current.Resources["PrimaryTextColorOverAccent"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                Current.Resources["SecondaryTextColorOverAccent"] = new SolidColorBrush(Color.FromRgb(218, 218, 218));
            }
            else
            {
                Current.Resources["PrimaryTextColorOverAccent"] = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                Current.Resources["SecondaryTextColorOverAccent"] = new SolidColorBrush(Color.FromRgb(82, 82, 82));
            }

            if (currentWindow is MainWindow window)
                window.UpdateControlsBoxColors();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string logPath = Path.Combine(DataFolderLocation, "Logs");
            string fileName = $"\\{DateTime.Now:M.d.yyyy hh mm tt}.txt";
            if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);
            File.WriteAllText(logPath + fileName, 
                $"FRESHMusicPlayer {Assembly.GetEntryAssembly().GetName().Version}\n" +
                $"{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}\n" +
                $"{Environment.OSVersion.VersionString}\n" +
                $"{e.Exception}");
            var message = string.Format(FRESHMusicPlayer.Properties.Resources.APPLICATION_CRITICALERROR, e.Exception.Message.ToString(), logPath + fileName);
            if (currentWindow is MainWindow maybeWindow)
            {
                try
                {
                    maybeWindow.NotificationHandler.Add(new Handlers.Notifications.Notification
                    {
                        DisplayAsToast = true,
                        IsImportant = true,
                        Type = Handlers.Notifications.NotificationType.Failure,
                        ContentText = message,
                        ButtonText = FRESHMusicPlayer.Properties.Resources.APPLICATION_OPEN_DEBUG_LOG,
                        OnButtonClicked = () =>
                        {
                            System.Diagnostics.Process.Start(logPath);
                            System.Diagnostics.Process.Start(logPath + fileName);
                            return true;
                        }
                    });
                }
                catch
                {
                    MessageBox.Show(message);
                }
            }
            else
            {
                MessageBox.Show(message);
            }
            LoggingHandler.Log($"There was an unhandled exception:\n{e.Exception}");
            e.Handled = true;
        }
    }
}
