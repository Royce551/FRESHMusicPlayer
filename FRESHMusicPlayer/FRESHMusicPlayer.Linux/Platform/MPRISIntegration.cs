using Avalonia.Controls;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Desktop;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.PlaybackIntegrations;
using FRESHMusicPlayer.Linux.DBus;
using FRESHMusicPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace FRESHMusicPlayer.Linux.Platform
{
    public class MPRISIntegration : IPlaybackIntegration
    {
        DBusMediaPlayer mediaPlayer;
        MainViewModel viewModel;
        Window window;

        public MPRISIntegration(MainViewModel viewModel, Window window)
        {
            this.viewModel = viewModel;
            this.window = window;

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var connection = new DBusConnection(DBusAddress.Session!);
            await connection.ConnectAsync();

            mediaPlayer = new DBusMediaPlayer(connection, viewModel, window);
            await mediaPlayer.AddToDBusAsync();
        }

        public void Close()
        {

        }

        public Task UpdateAsync(IMetadataProvider track, PlaybackStatus status)
        {
            mediaPlayer.UpdateMetadata(track, status);

            return Task.CompletedTask;
        }
    }

    class DBusMediaPlayer : DBusHandler,
        IMediaPlayer2Handler, IMediaPlayer2Properties,
        IPlayerHandler, IPlayerProperties
    {
        private const string ObjectPath = "/org/mpris/MediaPlayer2";
        private const string ServiceNamePrefix = "org.mpris.MediaPlayer2";

        private readonly MainViewModel viewModel;
        private readonly Window window;
        private bool _emitSignals;

        public bool CanQuit { get; set; }
        public bool CanRaise { get; set; }
        public bool HasTrackList { get; set; }
        public string Identity { get; set; } = "";
        public string DesktopEntry { get; set; } = "";
        public string[] SupportedUriSchemes { get; set; } = [];
        public string[] SupportedMimeTypes { get; set; } = [];
        public bool CanSetFullscreen { get; set; }

        private bool _fullscreen;
        public bool Fullscreen
        {
            get => _fullscreen;
            set
            {
                _fullscreen = value;
                EmitPropertyChanged(MediaPlayer2Property.Fullscreen);
            }
        }

        private string _playbackStatus = "Stopped";
        public string PlaybackStatus
        {
            get => _playbackStatus;
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                _playbackStatus = value;
                EmitPropertyChanged(PlayerProperty.PlaybackStatus);
            }
        }

        private string _loopStatus = "None";
        public string LoopStatus
        {
            get => _loopStatus;
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                _loopStatus = value;
                EmitPropertyChanged(PlayerProperty.LoopStatus);
            }
        }

        private double _rate = 1.0;
        public double Rate
        {
            get => _rate;
            set
            {
                _rate = value;
                EmitPropertyChanged(PlayerProperty.Rate);
            }
        }

        private bool _shuffle;
        public bool Shuffle
        {
            get => _shuffle;
            set
            {
                _shuffle = value;
                EmitPropertyChanged(PlayerProperty.Shuffle);
            }
        }

        private double _volume = 1.0;
        public double Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                EmitPropertyChanged(PlayerProperty.Volume);
            }
        }

        private Dictionary<string, VariantValue> _metadata = new();
        public Dictionary<string, VariantValue> Metadata
        {
            get => _metadata;
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                _metadata = value;
                EmitPropertyChanged(PlayerProperty.Metadata);
            }
        }

        public long Position { get; set; }
        public double MinimumRate { get; set; }
        public double MaximumRate { get; set; }
        public bool CanGoNext { get; set; }
        public bool CanGoPrevious { get; set; }
        public bool CanPlay { get; set; }
        public bool CanPause { get; set; }
        public bool CanSeek { get; set; }
        public bool CanControl { get; set; }

        public DBusMediaPlayer(DBusConnection connection, MainViewModel viewModel, Window window)
            : base(connection, ObjectPath, handlesChildPaths: false)
        {
            this.viewModel = viewModel;
            this.window = window;

            viewModel.ProgressTimer.Tick += ProgressTimer_Tick;

            Identity = "FRESHMusicPlayer";
            CanQuit = false;
            CanRaise = false;
            CanSetFullscreen = false;
            HasTrackList = false;
            SupportedUriSchemes = ["file"];
            SupportedMimeTypes = ["audio/mpeg", "audio/ogg"];
            PlaybackStatus = "Stopped";
            MinimumRate = 1.0;
            MaximumRate = 1.0;
            CanGoNext = true;
            CanGoPrevious = true;
            CanPlay = true;
            CanPause = true;
            CanSeek = true;
            CanControl = true;
            Position = 0;
            Metadata = new Dictionary<string, VariantValue>
            {
                ["xesam:title"] = "Example Song Title"
            };
        }

        public void UpdateMetadata(IMetadataProvider metadata, PlaybackStatus status)
        {
            var metadataDict = new Dictionary<string, VariantValue>
            {
                ["mpris:length"] = metadata.Length * 1000000,
                ["xesam:artist"] = string.Join(", ", metadata.Artists),
                ["xesam:album"] = metadata.Album,
                ["xesam:title"] = metadata.Title,
            };

            var mime = FindMimeType(metadata);
            if (metadata.CoverArt != null && mime != null)
            {
                var url = $"data:{mime};base64,{Convert.ToBase64String(metadata.CoverArt)}";
                metadataDict.Add("mpris:artUrl", url);
                LoggingHandler.Log($"MPRIS: Providing cover art URL via direct stream. Inferred mime is {mime}");
            }

            Metadata = metadataDict;

            ProgressTimer_Tick(this, EventArgs.Empty);
        }

        private static readonly byte[] BMP = { 66, 77 };
        private static readonly byte[] GIF = { 71, 73, 70, 56 };
        private static readonly byte[] JPG = { 255, 216, 255 };
        private static readonly byte[] PNG = { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82 };
        private static readonly byte[] TIFF = { 73, 73, 42, 0 };

        private string? FindMimeType(IMetadataProvider metadata)
        {
            if (metadata is FileMetadataProvider file && file.ATLTrack.EmbeddedPictures.Count != 0)
                return file.ATLTrack.EmbeddedPictures[0].MimeType;

            var cover = metadata.CoverArt;

            if (cover.Take(2).SequenceEqual(BMP)) return "image/bmp";
            if (cover.Take(4).SequenceEqual(GIF)) return "image/gif";
            if (cover.Take(3).SequenceEqual(JPG)) return "image/jpeg";
            if (cover.Take(16).SequenceEqual(PNG)) return "image/png";
            if (cover.Take(4).SequenceEqual(TIFF)) return "image/tiff";

            return null;
        }

        private void ProgressTimer_Tick(object? sender, EventArgs e)
        {
            if (!viewModel.Player.FileLoaded)
            {
                PlaybackStatus = "Stopped";
                return;
            }

            Position = (long)(viewModel.CurrentTimeSeconds * 1000000);
            if (viewModel.Player.Paused) PlaybackStatus = "Paused";
            else PlaybackStatus = "Playing";

            switch (viewModel.Player.Queue.RepeatMode)
            {
                case RepeatMode.None:
                    LoopStatus = "None";
                    break;
                case RepeatMode.RepeatOne:
                    LoopStatus = "Track";
                    break;
                case RepeatMode.RepeatAll:
                    LoopStatus = "Playlist";
                    break;
            }

            Shuffle = viewModel.Player.Queue.Shuffle;
        }

        public async Task<string> AddToDBusAsync()
        {
            Connection.AddMethodHandler(this);
            _emitSignals = true;
            string name = $"{ServiceNamePrefix}.FRESHMusicPlayer.instance{Environment.ProcessId}";
            await Connection.RequestNameAsync(name);
            return name;
        }

        private void EmitPropertyChanged(MediaPlayer2Property property)
        {
            if (_emitSignals)
                Connection.EmitPropertyChanged(ObjectPath, this, property);
        }

        private void EmitPropertyChanged(PlayerProperty property)
        {
            if (_emitSignals)
                Connection.EmitPropertyChanged(ObjectPath, this, property);
        }

        ValueTask IMediaPlayer2Handler.RaiseAsync()
        {
            Console.WriteLine("Raise requested");
            return default;
        }

        ValueTask IMediaPlayer2Handler.QuitAsync()
        {
            Console.WriteLine("Quit requested");
            Environment.Exit(0);
            return default;
        }

        ValueTask IPlayerHandler.NextAsync()
        {
            viewModel.Next();
            return default;
        }

        ValueTask IPlayerHandler.PreviousAsync()
        {
            viewModel.Previous();
            return default;
        }

        ValueTask IPlayerHandler.PauseAsync()
        {
            viewModel.TogglePause();
            PlaybackStatus = "Paused";
            return default;
        }

        ValueTask IPlayerHandler.PlayPauseAsync()
        {
            Console.WriteLine("PlayPause requested");
            PlaybackStatus = PlaybackStatus == "Playing" ? "Paused" : "Playing";
            return default;
        }

        async ValueTask IPlayerHandler.StopAsync()
        {
            viewModel.Player.Queue.Clear();
            await viewModel.Player.NextAsync();
            PlaybackStatus = "Stopped";
        }

        ValueTask IPlayerHandler.PlayAsync()
        {
            viewModel.TogglePause();
            PlaybackStatus = "Playing";
            return default;
        }

        ValueTask IPlayerHandler.SeekAsync(long offset)
        {
            Console.WriteLine($"Seek requested: offset={offset}");
            viewModel.CurrentTimeSeconds += offset;
            return default;
        }

        ValueTask IPlayerHandler.SetPositionAsync(ObjectPath trackId, long position)
        {
            Console.WriteLine($"SetPosition requested: trackId={trackId}, position={position}");
            viewModel.CurrentTimeSeconds = position;
            return default;
        }

        ValueTask IPlayerHandler.OpenUriAsync(string uri)
        {
            Console.WriteLine($"OpenUri requested: {uri}");
            return default;
        }

        ValueTask IMediaPlayer2Handler.HandleGetPropertyAsync(IMediaPlayer2Handler.GetPropertyContext context)
            => context.Handle(this);

        ValueTask IMediaPlayer2Handler.HandleGetAllPropertiesAsync(IMediaPlayer2Handler.GetAllPropertiesContext context)
            => context.Handle(this);

        ValueTask IMediaPlayer2Handler.HandleSetPropertyAsync(IMediaPlayer2Handler.SetPropertyContext context)
            => context.Handle(this);

        ValueTask IPlayerHandler.HandleGetPropertyAsync(IPlayerHandler.GetPropertyContext context)
            => context.Handle(this);

        ValueTask IPlayerHandler.HandleGetAllPropertiesAsync(IPlayerHandler.GetAllPropertiesContext context)
            => context.Handle(this);

        ValueTask IPlayerHandler.HandleSetPropertyAsync(IPlayerHandler.SetPropertyContext context)
            => context.Handle(this);
    }
}
