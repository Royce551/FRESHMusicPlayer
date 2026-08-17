using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FRESHMusicPlayer.ViewModels
{
    public partial class FullscreenViewModel : ViewModelBase
    {
        public FullscreenView View { get; set; } = null!;

        public FullscreenViewModel()
        {
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
        }

        [ObservableProperty]
        public partial MetadataViewModel CurrentMetadata { get; set; }

        private void Player_SongChanged(object? sender, EventArgs e) => Update();
        public void Update()
        {
            if (MainView.Player.FileLoaded)
            {
                CurrentMetadata = new MetadataViewModel(MainView.Player.Metadata, MainView, this);
            }
        }
    }

    public class MetadataViewModel : LyricsHandlingViewModel
    {
        public IMetadataProvider Metadata { get; set; }

        public string ArtistString => string.Join(", ", Metadata.Artists);

        public Bitmap CoverArt => Bitmap.DecodeToWidth(new MemoryStream(Metadata.CoverArt), 900);

        public bool LyricsAvailable => Lyrics != null;

        private readonly FullscreenViewModel viewModel;
        public MetadataViewModel(IMetadataProvider metadata, MainViewModel mainView, FullscreenViewModel viewModel)
        {
            Metadata = metadata;
            MainView = mainView;
            this.viewModel = viewModel;

            UpdateLyrics();
        }

        public override void OnCurrentLineChanged() => viewModel.View.ScrollToCenter(CurrentLines);
    }
}
