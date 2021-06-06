using ATL;
using Avalonia.Controls;
using FRESHMusicPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus;

[assembly: InternalsVisibleTo(Tmds.DBus.Connection.DynamicAssemblyName)]
namespace FRESHMusicPlayer.Handlers.Integrations
{
    public class MPRISIntegration : IPlaybackIntegration
    {
        public event EventHandler UINeedsUpdate;

        private Player player;
        private MediaPlayer2 mediaPlayer2;
        private Connection connection;

        public MPRISIntegration(MainWindowViewModel viewModel, Window window)
        {
            player = new(viewModel);
            mediaPlayer2 = new(window);
            Initialize();
        }

        public async Task Initialize()
        {
            try
            {
                var server = new ServerConnectionOptions();
                connection = new Connection(Address.Session);
                await connection.ConnectAsync();

                await connection.RegisterObjectAsync(player);
                await connection.RegisterObjectAsync(mediaPlayer2);

                await connection.RegisterServiceAsync("org.mpris.MediaPlayer2.FRESHMusicPlayer");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        public void Update(Track track, PlaybackStatus status)
        {
            
        }
    }

    [DBusInterface("org.mpris.MediaPlayer2.Player")]
    interface IPlayer : IDBusObject
    {
        Task NextAsync();
        Task PreviousAsync();
        Task PlayPauseAsync();
        Task StopAsync();
        Task SeekAsync(long offset);
        Task SetPositionAsync(ObjectPath trackID, long position);
        Task OpenUriAsync();

        //Task<IDisposable> WatchSeekedAsync(Action<ObjectPath> handler, Action<Exception> onError = null);

        Task<IDictionary<string, object>> GetAllAsync();
        Task<object> GetAsync(string prop);
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }
    [DBusInterface("org.mpris.MediaPlayer2")]
    interface IMediaPlayer2 : IDBusObject
    {
        Task RaiseAsync();
        Task QuitAsync();

        Task<IDictionary<string, object>> GetAllAsync();
        Task<object> GetAsync(string prop);
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    class Player : IPlayer
    {
        public event Action<PropertyChanges> OnPropertiesChanged;

        private MainWindowViewModel viewModel;
        private IDictionary<string, object> properties = new Dictionary<string, object>()
        {
            { "Volume", 1d }
        };

        public Player(MainWindowViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public ObjectPath ObjectPath => new("/org/mpris/MediaPlayer2");

        public async Task<IDictionary<string, object>> GetAllAsync() => properties;

        public async Task<object> GetAsync(string prop) => properties[prop];

        public async Task NextAsync() => viewModel.SkipNextCommand();

        public async Task OpenUriAsync() => viewModel.OpenTrackCommand();

        public async Task PlayPauseAsync() => viewModel.PlayPauseCommand();

        public async Task PreviousAsync() => viewModel.SkipPreviousCommand();

        public async Task SeekAsync(long offset)
        {
            viewModel.CurrentTime.Add(TimeSpan.FromMilliseconds(offset * 1000));
        }

        public async Task SetAsync(string prop, object val)
        {
            properties[prop] = val;
        }

        public async Task SetPositionAsync(ObjectPath trackID, long position)
        {
            viewModel.CurrentTime = TimeSpan.FromMilliseconds(position * 1000);
        }

        public async Task StopAsync()
        {
            viewModel.Player.StopMusic();
        }

        public async Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler) => SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);

        //public async Task<IDisposable> WatchSeekedAsync(Action<ObjectPath> handler, Action<Exception> onError = null)
        //{
        //    Console.WriteLine("Not implemented: WatchSeekedAsync");
        //    return new LiteDB.LiteDatabase("lols2.db");

        //}
    }

    class MediaPlayer2 : IMediaPlayer2
    {
        public event Action<PropertyChanges> OnPropertiesChanged;

        private IDictionary<string, object> properties = new Dictionary<string, object>()
        {
            { "CanQuit", true },
            { "CanRaise", true },
            { "HasTrackList", false },
            { "Identity", "FRESHMusicPlayer" },
            { "SupportedUriSchemes", new string[]{"file", "http"} },
            { "SupportedMimeTypes", new string[]{ "audio/mp3", "audio/mpeg", "audio/mpeg3", "audio/wav", "audio/ogg", "audio/mp4", "video/avi", "video/msvideo", "video/mpeg", "video/quicktime", "video/x-ms-wmv" } }
        };

        private Window window;
        public MediaPlayer2(Window window) => this.window = window;

        public ObjectPath ObjectPath => new("/org/mpris");

        public async Task<IDictionary<string, object>> GetAllAsync() => properties;

        public async Task<object> GetAsync(string prop) => properties[prop];

        public async Task QuitAsync() => window.Close();

        public async Task RaiseAsync() => window.Activate();

        public async Task SetAsync(string prop, object val) => properties[prop] = val;

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler) => SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
