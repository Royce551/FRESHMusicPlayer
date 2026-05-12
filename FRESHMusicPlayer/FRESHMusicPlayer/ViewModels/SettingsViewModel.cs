using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.PlaybackIntegrations;
using FRESHMusicPlayer.Views;
using Newtonsoft.Json.Linq;
using SIADL.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase, IRecipient<PropertyChangedMessage<bool>>
    {
        public SettingsViewModel(MainViewModel mainView)
        {
            MainView = mainView;
            UpdateLastFMStatus();
        }

        public override void AfterPageLoaded()
        {
            base.AfterPageLoaded();
        }

        public override void OnNavigatingAway()
        {
            base.OnNavigatingAway();
        }

        public async void CleanAndUpdateLibrary()
        {
            var tracks = MainView.Library.GetAllTracks().Select(x => x.Path);
            var tracksWithoutDuplicates = tracks.Distinct();

            var tracksToRemove = tracks.Except(tracksWithoutDuplicates);
            foreach (var track in tracksToRemove)
                MainView.Library.Remove(track);

            var remainingTracks = MainView.Library.GetAllTracks();
            foreach (var track in remainingTracks)
                if (!track.Path.StartsWith("http") && !File.Exists(track.Path))
                    MainView.Library.Remove(track.Path);

            var remainingTracks2 = MainView.Library.GetAllTracks();
            foreach (var track in remainingTracks2)
            {
                track.HasBeenProcessed = false;
                MainView.Library.Database.GetCollection<DatabaseTrack>(Library.TracksCollectionName).Update(track);
            }

            await MainView.Library.ProcessDatabaseMetadataAsync();
        }

        [ObservableProperty]
        public partial string LastFMStatus { get; set; }
        [ObservableProperty]
        public partial bool IsLastFMSignInButtonVisible { get; set; } = false;
        [ObservableProperty]
        public partial bool IsLastFMConfirmButtonVisible { get; set; } = false;
        [ObservableProperty]
        public partial bool IsLastFMSignOutButtonVisible { get; set; } = false;

        private LastFMIntegration lastFMIntegration;

        private void UpdateLastFMStatus()
        {
            if (MainView.PlaybackIntegrations.Find(x => x is LastFMIntegration) is not LastFMIntegration lastFMIntegration) return;
            this.lastFMIntegration = lastFMIntegration;

            if (lastFMIntegration.IsInitialized)
            {
                LastFMStatus = "Signed in to last.fm";
                IsLastFMSignInButtonVisible = false;
                IsLastFMConfirmButtonVisible = false;
                IsLastFMSignOutButtonVisible = true;
            }
            else
            {
                LastFMStatus = "Not signed in to last.fm";
                IsLastFMSignInButtonVisible = true;
                IsLastFMConfirmButtonVisible = false;
                IsLastFMSignOutButtonVisible = false;
            }
        }

        private string lastFMToken;

        public async void LastFMSignIn()
        {
            var httpClient = MainView.HttpClient;

            lastFMToken = await lastFMIntegration.GetAuthorizationToken();

            SIADLUtilities.OpenURL($"http://www.last.fm/api/auth?api_key={LastFMIntegration.ApiKey}&token={lastFMToken}");

            LastFMStatus = "Complete last.fm authentication in the browser window that just opened";
            IsLastFMSignInButtonVisible = false;
            IsLastFMConfirmButtonVisible = true;
            IsLastFMSignOutButtonVisible = false;
           
        }
        public async void LastFMConfirm()
        {
            var result = await lastFMIntegration.ConfirmSignIn(lastFMToken);

            if (!result.IsSuccessStatusCode)
            {
                LastFMStatus = "FRESHMusicPlayer was not granted access; press sign in to try again";
                IsLastFMSignInButtonVisible = true;
                IsLastFMConfirmButtonVisible = false;
                IsLastFMSignOutButtonVisible = false;
            }
            else
            {
                var json = JObject.Parse(await result.Content.ReadAsStringAsync());
                MainView.Config.LastFMToken = json?.SelectToken("session.key").ToString();

                lastFMIntegration.Initialize();
                UpdateLastFMStatus();
            }
        }
        public void LastFMSignOut()
        {
            MainView.Config.LastFMToken = null;
            lastFMIntegration.Initialize();
            UpdateLastFMStatus();
        }

        public void NukeLibrary() => MainView.Library.Nuke();

        public void OpenReportIssuePage() => SIADLUtilities.OpenURL("https://github.com/Royce551/FRESHMusicPlayer/issues?q=is%3Aissue");

        public void OpenSourceCodePage() => SIADLUtilities.OpenURL("https://github.com/Royce551/FRESHMusicPlayer");

        public void Receive(PropertyChangedMessage<bool> message)
        {
            if (message is { Sender: ConfigurationFile, PropertyName: nameof(ConfigurationFile.IntegrateLastFM) }) UpdateLastFMStatus();
        }
    }

    //public partial class SettingsItem : ObservableRecipient
    //{
    //    private readonly ViewModelBase viewModel;

    //    public SettingsItem(ViewModelBase viewModel)
    //    {
    //        this.viewModel = viewModel;
    //    }
    //}
}
