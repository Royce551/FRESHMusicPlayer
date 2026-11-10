using System;
using System.Drawing;

namespace FRESHMusicPlayer.Utilities.ColorQuantization
{
    /// <summary>
    /// Describes a cluster in a K-Means Clustering set
    /// </summary>
    public class KCluster
    {
        private long redSum;
        private long greenSum;
        private long blueSum;
        private int count;

        /// <summary>
        /// Creates a new K-Means Cluster set
        /// </summary>
        /// <param name="center">The initial center point for the set</param>
        public KCluster(Color center)
        {
            Center = center;
        }

        /// <summary>
        /// The current center point of the cluster
        /// </summary>
        public Color Center { get; set; }

        /// <summary>
        /// The number of points this cluster had before <seealso cref="RecalculateCenter"/> was called
        /// </summary>
        public int PriorCount { get; set; }

        /// <summary>
        /// Add <paramref name="color"/> to the cluster. This means that the next time <seealso cref="RecalculateCenter"/> <paramref name="color"/> will be considered in the center calculation
        /// </summary>
        /// <param name="color"></param>
        public void Add(Color color) => Add(color.R, color.G, color.B);

        internal void Add(int red, int green, int blue)
        {
            redSum += red;
            greenSum += green;
            blueSum += blue;
            count++;
        }

        /// <summary>
        /// Based on all the items that have been <seealso cref="Add">Added</seealso> to this cluster calculates the center.
        /// </summary>
        /// <param name="threshold">If the center has moved by at least this value cluster has not yet converged and needs to be recalculated</param>
        /// <returns><c>true</c> if the recalculated center's euclidean distance from the old center is at least <paramref name="threshold"/>. <c>false</c> if it is less than this value</returns>
        public bool RecalculateCenter(double threshold = 0.0d)
        {
            Color updatedCenter;

            if (count > 0)
            {
                updatedCenter = Color.FromArgb(
                    (int)Math.Round((float)redSum / count),
                    (int)Math.Round((float)greenSum / count),
                    (int)Math.Round((float)blueSum / count));
            }
            else updatedCenter = Color.FromArgb(0, 0, 0, 0);

            var distance = EuclideanDistance(Center, updatedCenter);
            Center = updatedCenter;

            PriorCount = count;
            redSum = greenSum = blueSum = 0;
            count = 0;

            return distance > threshold;
        }

        /// <summary>
        /// Returns the Euclidean distance of <paramref name="color"/> from the current cluster center point
        /// </summary>
        public double DistanceFromCenter(Color color) => EuclideanDistance(color, Center);

        /// <summary>
        /// Calcultes the Euclidean distance between two colors, <paramref name="c1"/> and <paramref name="c2"/>
        /// </summary>
        public static double EuclideanDistance(Color c1, Color c2) => Math.Sqrt(SquaredDistance(c1.R, c1.G, c1.B, c2));

        internal int SquaredDistanceFromCenter(int red, int green, int blue) => SquaredDistance(red, green, blue, Center);

        private static int SquaredDistance(int red, int green, int blue, Color center)
        {
            var redDifference = red - center.R;
            var greenDifference = green - center.G;
            var blueDifference = blue - center.B;
            return redDifference * redDifference + greenDifference * greenDifference + blueDifference * blueDifference;
        }
    }
}

