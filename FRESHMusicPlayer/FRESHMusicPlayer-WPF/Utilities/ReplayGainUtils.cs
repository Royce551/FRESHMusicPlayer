using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NReplayGain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Utilities
{
    public class ReplayGainUtils
    {
        public static TrackGain CalculateReplayGainDataForTrack(string track)
        {
            var trackGain = new TrackGain(44100);

            var fileReader = new AudioFileReader(track);

            var buffer = new float[44100 * 2];
            while (true)
            {
                var numRead = fileReader.Read(buffer, 0, buffer.Length);
                if (numRead == 0) break;

                trackGain.AnalyzeSamples(buffer, numRead);
            }

            fileReader.Dispose();
            return trackGain;
        }
    }
}
