using ATL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus;

namespace FRESHMusicPlayer.Handlers.Integrations
{
    public class MPRISIntegration : IPlaybackIntegration
    {
        public event EventHandler UINeedsUpdate;

        private Player player;

        public MPRISIntegration(FRESHMusicPlayer.Player player)
        {
            this.player = new(player);
            Initialize();
        }

        public async Task Initialize()
        {
            try
            {
                Console.WriteLine("Initializing");
                var server = new ServerConnectionOptions();
                Console.WriteLine("2");
                using var connection = new Connection(Address.Session);
                Console.WriteLine("3");
                await connection.ConnectAsync();
                Console.WriteLine("4");
                await connection.RegisterObjectAsync(player);
                Console.WriteLine("5");
                await connection.RegisterServiceAsync("org.mpris.MediaPlayer2.FRESHMusicPlayer");
                Console.WriteLine("Initialization Complete");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Dispose()
        {
           
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
        //Task SetPosition(ObjectPath trackID, long position);
        Task OpenUriAsync();

        //Task<IDisposable> WatchSeekedAsync(Action<ObjectPath> handler, Action<Exception> onError = null);

        Task<IDictionary<string, object>> GetAllAsync();
        Task<object> GetAsync(string prop);
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    class Player : IPlayer
    {
        public event Action<PropertyChanges> OnPropertiesChanged;

        private FRESHMusicPlayer.Player player;
        private IDictionary<string, object> properties = new Dictionary<string, object>()
        {
            { "Volume", 1d }
        };

        public Player(FRESHMusicPlayer.Player player)
        {
            this.player = player;
        }

        public ObjectPath ObjectPath => new("/org/mpris/MediaPlayer2");

        public async Task<IDictionary<string, object>> GetAllAsync() => properties;

        public async Task<object> GetAsync(string prop) => properties[prop];

        public async Task NextAsync()
        {
            player.NextSong();
        }

        public async Task OpenUriAsync()
        {
            Console.WriteLine("Not implemented: OpenUriAsync");
        }

        public async Task PlayPauseAsync()
        {
            if (player.Paused) player.ResumeMusic();
            else player.PauseMusic();
        }

        public async Task PreviousAsync()
        {
            player.PreviousSong();
        }

        public async Task SeekAsync(long offset)
        {
            player.CurrentTime.Add(TimeSpan.FromMilliseconds(offset * 1000));
        }

        public async Task SetAsync(string prop, object val)
        {
            properties[prop] = val;
        }
/*
        public async Task SetPosition(ObjectPath trackID, long position)
        {
            player.CurrentTime = TimeSpan.FromMilliseconds(position * 1000);
        }
*/
        public async Task StopAsync()
        {
            player.StopMusic();
        }

        public async Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler) => SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);

        //public async Task<IDisposable> WatchSeekedAsync(Action<ObjectPath> handler, Action<Exception> onError = null)
        //{
        //    Console.WriteLine("Not implemented: WatchSeekedAsync");
        //    return new LiteDB.LiteDatabase("lols2.db");

        //}
    }

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
