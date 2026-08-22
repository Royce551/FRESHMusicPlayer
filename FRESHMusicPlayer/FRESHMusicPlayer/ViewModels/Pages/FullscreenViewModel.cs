using ATL.AudioData.IO;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels
{
    public partial class FullscreenViewModel : ViewModelBase
    {
        public FullscreenView View { get; set; } = null!;

        public FullscreenViewModel()
        {
        }

        public bool FocusMode
        {
            get => field;
            set
            {
                SetProperty(ref field, value);
                CurrentMetadata?.UpdateFocusModeProperties();
            }
        }

        public override void AfterPageLoaded()
        {
            Update();
            MainView.Player.SongChanged += Player_SongChanged;
            MainView.ProgressTimer.Tick += ProgressTimer_Tick;
        }

        private void ProgressTimer_Tick(object? sender, EventArgs e) => CurrentMetadata.TickLyrics();

        public override void OnNavigatingAway()
        {
            MainView.Player.SongChanged -= Player_SongChanged;
            MainView.ProgressTimer.Tick -= ProgressTimer_Tick;
            View.LeaveFullscreen();
        }

        [ObservableProperty]
        public partial MetadataViewModel CurrentMetadata { get; set; }

        private void Player_SongChanged(object? sender, EventArgs e) => Update();
        public async void Update()
        {
            if (MainView.Player.FileLoaded)
            {
                var coverArt = await Task.Run(() => Bitmap.DecodeToWidth(new MemoryStream(MainView.Player.Metadata.CoverArt), 1000));
                CurrentMetadata = new MetadataViewModel(MainView.Player.Metadata, coverArt, MainView, this);
            }
        }

        public void Back() => MainView.NavigateBack();
    }

    public class MetadataViewModel : LyricsHandlingViewModel
    {
        public IMetadataProvider Metadata { get; }

        public string ArtistString => string.Join(", ", Metadata.Artists);

        public Bitmap CoverArt { get; }

        public bool LyricsAvailable => Lyrics != null && !viewModel.FocusMode;

        private readonly FullscreenViewModel viewModel;
        public MetadataViewModel(IMetadataProvider metadata, Bitmap coverArt, MainViewModel mainView, FullscreenViewModel viewModel)
        {
            Metadata = metadata;
            CoverArt = coverArt;
            MainView = mainView;
            this.viewModel = viewModel;

            UpdateLyrics();
        }

        public override void OnCurrentLineChanged() => viewModel.View.ScrollToCenter(CurrentLines);

        public bool IsBackgroundCoverArtVisible => !viewModel.FocusMode;

        public Stretch Stretch => viewModel.FocusMode ? Stretch.None : Stretch.Uniform;

        public int CoverSize => viewModel.FocusMode ? 150 : 300;

        public void UpdateFocusModeProperties()
        {
            OnPropertyChanged(nameof(IsBackgroundCoverArtVisible));
            OnPropertyChanged(nameof(LyricsAvailable));
            OnPropertyChanged(nameof(Stretch));
            OnPropertyChanged(nameof(CoverSize));
        }
    }
}
