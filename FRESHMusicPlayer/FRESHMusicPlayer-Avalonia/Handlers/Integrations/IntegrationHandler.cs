using ATL;
using System.Collections.Generic;

namespace FRESHMusicPlayer.Handlers.Integrations
{
    public class IntegrationHandler
    {
        public List<IPlaybackIntegration> AllIntegrations { get; private set; } = new();

        public void Add(IPlaybackIntegration integration)
        {
            AllIntegrations.Add(integration);

        }

        public void Remove(IPlaybackIntegration integration)
        {
            integration.Dispose();
            AllIntegrations.Remove(integration);
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
    }
}
