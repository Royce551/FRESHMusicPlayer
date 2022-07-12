using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace FRESHMusicPlayer.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        public MainWindowViewModel MainWindowWm { get; set; }

        public ObservableCollection<DatabaseTrack> ContentItems { get; set; } = new();

        private string search;
        public string Search
        {
            get => search;
            set
            {
                this.RaiseAndSetIfChanged(ref search, value);
                PerformSearch(value);
            }
        }

        private Queue<string> searchQueries = new();
        private bool isSearchOperationRunning = false;
        private async void PerformSearch(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                ContentItems.Clear();
                return;
            }
            searchQueries.Enqueue(query.ToUpper());
            async Task GetResults()
            {
                isSearchOperationRunning = true;
                var query = searchQueries.Dequeue();
                ContentItems.Clear();
                await Task.Run(() =>
                {
                    foreach (var thing in MainWindowWm.Library.Database.GetCollection<DatabaseTrack>("tracks")
                    .Query()
                    .Where(x => x.Title.ToUpper().Contains(query) || x.Artist.ToUpper().Contains(query) || x.Album.ToUpper().Contains(query))
                    .OrderBy("Title")
                    .ToList())
                    {
                        if (searchQueries.Count > 1) break;
                        ContentItems.Add(thing);
                    }
                });
                isSearchOperationRunning = false;
                if (searchQueries.Count != 0) await GetResults();
            }
            if (!isSearchOperationRunning) await GetResults();
        }
    }
}
