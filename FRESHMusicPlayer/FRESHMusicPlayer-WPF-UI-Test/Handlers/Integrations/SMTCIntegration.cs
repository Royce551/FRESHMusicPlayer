using ATL;
using FRESHMusicPlayer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Windows.Media;

namespace FRESHMusicPlayer.Handlers.Integrations
{
    class SMTCIntegration : IPlaybackIntegration
    {
        private SystemMediaTransportControls smtc;

        private readonly MainWindow window;

        public SMTCIntegration(MainWindow window)
        {
            this.window = window;

            var smtcInterop = (WindowsInteropUtils.ISystemMediaTransportControlsInterop)WindowsRuntimeMarshal.GetActivationFactory(typeof(SystemMediaTransportControls));
            var wih = new WindowInteropHelper(window);
            IntPtr hWnd = wih.Handle;
            smtc = smtcInterop.GetForWindow(hWnd, new Guid("99FA3FF4-1742-42A6-902E-087D41F965EC"));
            smtc.IsPlayEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsNextEnabled = true;
            smtc.IsStopEnabled = true;
            smtc.IsPreviousEnabled = true;
            smtc.ButtonPressed += Smtc_ButtonPressed;
        }

        public void Update(Track track, PlaybackStatus status)
        {
            smtc.PlaybackStatus = (MediaPlaybackStatus)status;
            var updater = smtc.DisplayUpdater;
            updater.Type = MediaPlaybackType.Music;
            updater.MusicProperties.Artist = track.Artist;
            updater.MusicProperties.AlbumArtist = track.AlbumArtist;
            updater.MusicProperties.Title = track.Title;
            updater.Update();
        }

        public void Close()
        {
            // nothing needs to be done
        }

        private void Smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    window.Dispatcher.Invoke(() => window.PlayPauseMethod());
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    window.Dispatcher.Invoke(() => window.PlayPauseMethod());
                    break;
                case SystemMediaTransportControlsButton.Next:
                    window.Dispatcher.Invoke(() => window.NextTrackMethod());
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    window.Dispatcher.Invoke(() => window.PreviousTrackMethod());
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    window.Dispatcher.Invoke(() => window.StopMethod());
                    break;
                default:
                    break;
            }
        }
    }
}
