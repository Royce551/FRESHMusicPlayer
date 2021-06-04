﻿using ATL;
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
    public interface IPlayer : IDBusObject
    {
        Task NextAsync();
        Task PreviousAsync();
        Task PlayPauseAsync();
        Task StopAsync();
        Task Seek(long offset);
        Task SetPosition(ObjectPath trackID, long position);
        Task OpenUri();

        Task<IDisposable> WatchSeekedAsync(Action<ObjectPath> handler, Action<Exception> onError = null);

        Task<IDictionary<string, object>> GetAllAsync();
        Task<object> GetAsync(string prop);
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public class Player : IPlayer
    {
        private FRESHMusicPlayer.Player player;
        public Player(FRESHMusicPlayer.Player player)
        {
            this.player = player;
        }

        public ObjectPath ObjectPath => new("/org/mpris/MediaPlayer2");

        public Task<IDictionary<string, object>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<object> GetAsync(string prop)
        {
            throw new NotImplementedException();
        }


        public async Task NextAsync()
        {
            player.NextSong();
        }

        public async Task OpenUri()
        {
            throw new NotImplementedException();
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

        public async Task Seek(long offset)
        {
            player.CurrentTime.Add(TimeSpan.FromMilliseconds(offset * 1000));
        }

        public Task SetAsync(string prop, object val)
        {
            throw new NotImplementedException();
        }

        public async Task SetPosition(ObjectPath trackID, long position)
        {
            player.CurrentTime = TimeSpan.FromMilliseconds(position * 1000);
        }

        public async Task StopAsync()
        {
            player.StopMusic();
        }

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
        {
            throw new NotImplementedException();
        }

        public Task<IDisposable> WatchSeekedAsync(Action<ObjectPath> handler, Action<Exception> onError = null)
        {
            throw new NotImplementedException();
        }
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}
