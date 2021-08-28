using ATL;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        private readonly SystemMediaTransportControls smtc;

        private readonly MainWindow window;

        public SMTCIntegration(MainWindow window)
        {
            LoggingHandler.Log("Starting SMTC Integration");

            this.window = window;

            var smtcInterop = (ISystemMediaTransportControlsInterop)WindowsRuntimeMarshal.GetActivationFactory(typeof(SystemMediaTransportControls));
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

        public void Update(IMetadataProvider track, PlaybackStatus status)
        {
            smtc.PlaybackStatus = (MediaPlaybackStatus)status;
            var updater = smtc.DisplayUpdater;
            updater.Type = MediaPlaybackType.Music;
            updater.MusicProperties.Artist = string.Join(", ", track.Artists);
            updater.MusicProperties.AlbumTitle = track.Album;
            updater.MusicProperties.TrackNumber = (uint)track.TrackNumber;
            updater.MusicProperties.AlbumTrackCount = (uint)track.TrackTotal;
            //updater.Thumbnail = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromStream(System.IO.WindowsRuntimeStreamExtensions.AsRandomAccessStream(new MemoryStream(track.CoverArt)));
            updater.MusicProperties.Title = track.Title;
            updater.Update();
        }

        public void Close()
        {
            LoggingHandler.Log("Closing SMTC integration");
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
        [Guid("ddb0472d-c911-4a1f-86d9-dc3d71a95f5a")]
        [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
        interface ISystemMediaTransportControlsInterop
        {
            SystemMediaTransportControls GetForWindow(IntPtr Window, in Guid riid);
        }

        [ComImport]
        [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IInitializeWithWindow
        {
            void Initialize(IntPtr hwnd);

        }
    }
}
