using System;
using System.Drawing;

namespace FRESHMusicPlayer.Utilities.ColorQuantization
{
    internal sealed class KCluster
    {
        private long redSum;
        private long greenSum;
        private long blueSum;
        private int pixelCount;

        public KCluster(Color center)
        {
            Center = center;
        }

        public Color Center { get; private set; }
        public int PreviousPixelCount { get; private set; }

        public void Add(int red, int green, int blue)
        {
            redSum += red;
            greenSum += green;
            blueSum += blue;
            pixelCount++;
        }

        public bool RecalculateCenter(double convergenceThreshold)
        {
            var newCenter = Color.FromArgb(0, 0, 0, 0);
            if (pixelCount > 0)
            {
                newCenter = Color.FromArgb(
                    (int)Math.Round((float)redSum / pixelCount),
                    (int)Math.Round((float)greenSum / pixelCount),
                    (int)Math.Round((float)blueSum / pixelCount));
            }

            var distance = Math.Sqrt(SquaredDistanceFromCenter(newCenter.R, newCenter.G, newCenter.B));
            Center = newCenter;

            PreviousPixelCount = pixelCount;
            redSum = 0;
            greenSum = 0;
            blueSum = 0;
            pixelCount = 0;

            return distance > convergenceThreshold;
        }

        public int SquaredDistanceFromCenter(int red, int green, int blue)
        {
            var redDifference = red - Center.R;
            var greenDifference = green - Center.G;
            var blueDifference = blue - Center.B;
            return redDifference * redDifference + greenDifference * greenDifference + blueDifference * blueDifference;
        }
    }
}
