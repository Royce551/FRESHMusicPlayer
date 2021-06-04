using ATL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.Integrations
{
    public class IntegrationHandler
    {
        public List<IPlaybackIntegration> AllIntegrations { get; private set; } = new();

        public event EventHandler UINeedsUpdate;

        public void Add(IPlaybackIntegration integration)
        {
            AllIntegrations.Add(integration);
            integration.UINeedsUpdate += Integration_UINeedsUpdate;
        }

        public void Remove(IPlaybackIntegration integration)
        {
            integration.Dispose();
            AllIntegrations.Remove(integration);
            integration.UINeedsUpdate -= Integration_UINeedsUpdate;
        }

        public void Update(Track track, PlaybackStatus status)
        {
            foreach (var integration in AllIntegrations)
                integration.Update(track, status);
        }

        public void Dispose()
        {
            foreach (var integration in AllIntegrations)
                integration.Dispose();
        }

        private void Integration_UINeedsUpdate(object sender, EventArgs e) => UINeedsUpdate?.Invoke(null, EventArgs.Empty);
    }
}
