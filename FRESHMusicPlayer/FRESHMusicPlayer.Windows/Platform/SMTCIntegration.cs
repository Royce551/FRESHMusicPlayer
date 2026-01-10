using Avalonia.Controls;
using Avalonia.Threading;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.PlaybackIntegrations;
using FRESHMusicPlayer.ViewModels;
using FRESHMusicPlayer.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage.Streams;
using WinRT;

namespace FRESHMusicPlayer.Windows.Platform
{
    public class SMTCIntegration : IPlaybackIntegration
    {
        private readonly SystemMediaTransportControls smtc;

        private readonly MainViewModel viewModel;

        public SMTCIntegration(MainViewModel viewModel, Window window)
        {
            LoggingHandler.Log("Starting SMTC Integration");

            this.viewModel = viewModel;

            IntPtr hWnd = window.TryGetPlatformHandle()?.Handle ?? throw new PlatformNotSupportedException();
            smtc = SystemMediaTransportControlsInterop.GetForWindow(hWnd);
            smtc.IsPlayEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsNextEnabled = true;
            smtc.IsStopEnabled = true;
            smtc.IsPreviousEnabled = true;
            smtc.ButtonPressed += Smtc_ButtonPressed;
        }

        public async Task UpdateAsync(IMetadataProvider track, PlaybackStatus status)
        {
            smtc.PlaybackStatus = (MediaPlaybackStatus)status;

            if (status == PlaybackStatus.Changing) return;

            var updater = smtc.DisplayUpdater;
            updater.Type = MediaPlaybackType.Music;
            updater.MusicProperties.Artist = string.Join(", ", track.Artists);
            updater.MusicProperties.AlbumTitle = track.Album;
            updater.MusicProperties.TrackNumber = (uint)track.TrackNumber;
            updater.MusicProperties.AlbumTrackCount = (uint)track.TrackTotal;
            if (track.CoverArt != null)
            {
                var decoder = await BitmapDecoder.CreateAsync(new MemoryStream(track.CoverArt, 0, track.CoverArt.Length, true, true).AsRandomAccessStream());
                var transcodedImage = new InMemoryRandomAccessStream();

                BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(transcodedImage, decoder);
                await encoder.FlushAsync();

                transcodedImage.Seek(0);
                updater.Thumbnail = RandomAccessStreamReference.CreateFromStream(transcodedImage);
            }
            else updater.Thumbnail = null;

            updater.MusicProperties.Title = track.Title;
            updater.Update();
        }

        public void Close()
        {
            LoggingHandler.Log("Closing SMTC integration");
        }

        private void Smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Dispatcher.UIThread.Invoke(viewModel.TogglePause);
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    Dispatcher.UIThread.Invoke(viewModel.TogglePause);
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Dispatcher.UIThread.Invoke(viewModel.Next);
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Dispatcher.UIThread.Invoke(viewModel.Previous);
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        viewModel.Player.Stop();
                    });
                    break;
                default:
                    break;
            }
        }
    }
}
