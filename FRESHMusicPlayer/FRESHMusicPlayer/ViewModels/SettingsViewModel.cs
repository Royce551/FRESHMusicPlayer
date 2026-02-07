using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using FRESHMusicPlayer.Handlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel(MainViewModel mainView)
        {
            MainView = mainView;
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

        public void NukeLibrary() => MainView.Library.Nuke();
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
