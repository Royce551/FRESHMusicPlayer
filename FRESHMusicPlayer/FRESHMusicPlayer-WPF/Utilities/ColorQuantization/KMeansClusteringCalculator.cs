using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Utilities.ColorQuantization
{
    /// <summary>
    /// Calculates the K-Means Clusters for a set of colours
    /// </summary>
    public class KMeansClusteringCalculator
    {

        /// <summary>
        /// Calculates the <paramref name="k"/> clusters for <paramref name="colours"/>. Iterations continues until clusters move by less than <paramref name="threshold"/>
        /// </summary>
        /// <param name="k">The number of clusters to calculate (eg. The number of results to return)</param>
        /// <param name="colours">The list of colours to calculate <paramref name="k"/> for</param>
        /// <param name="threshold">Threshold for iteration. A lower value should produce more correct results but requires more iterations and for some <paramref name="colours"/> may never produce a result</param>
        /// <returns>The <paramref name="k"/> colours for the image in descending order from most common to least common</returns>
        public IList<System.Drawing.Color> Calculate(int k, IList<System.Drawing.Color> colours, double threshold = 0.0d)
        {
            List<KCluster> clusters = new List<KCluster>();

            // 1. Initialisation.
            //   Create K clusters with a random data point from our input.
            //   We make sure not to use the same index twice for two inputs
            Random random = new Random(473);  // absolutely arbitrary, most important thing is that we get consistent results
            List<int> usedIndexes = new List<int>();
            while (clusters.Count < k)
            {
                int index = random.Next(0, colours.Count);
                if (usedIndexes.Contains(index) == true)
                {
                    continue;
                }

                usedIndexes.Add(index);
                KCluster cluster = new KCluster(colours[index]);
                clusters.Add(cluster);
            }

            bool updated = false;
            do
            {
                updated = false;
                // 2. For each colour in our input determine which cluster's centre point is the closest and add the colour to the cluster
                foreach (System.Drawing.Color colour in colours)
                {
                    double shortestDistance = float.MaxValue;
                    KCluster closestCluster = null;

                    foreach (KCluster cluster in clusters)
                    {
                        double distance = cluster.DistanceFromCentre(colour);
                        if (distance < shortestDistance)
                        {
                            shortestDistance = distance;
                            closestCluster = cluster;
                        }
                    }

                    closestCluster.Add(colour);
                }

                // 3. Recalculate the clusters centre.
                foreach (KCluster cluster in clusters)
                {
                    if (cluster.RecalculateCentre(threshold) == true)
                    {
                        updated = true;
                    }
                }

                // 4. If we updated any centre point this iteration then iterate again
            } while (updated == true);

            return clusters.OrderByDescending(c => c.PriorCount).Select(c => c.Centre).ToList();
        }

        public static HSLColor RGBToHSL(byte r, byte g, byte b)
        {
            var pR = r / 255f;
            var pG = g / 255f;
            var pB = b / 255f;

            var min = Math.Min(Math.Min(pR, pG), pB);
            var max = Math.Max(Math.Max(pR, pG), pB);
            var delta = max - min;

            var h = 0f;
            var s = 0f;
            var l = (float)((max + min) / 2f);

            if (delta != 0)
            {
                if (l < 0.5f) s = (float)(delta / (max + min));
                else s = (float)(delta / (2f - max - min));

                if (r == max) h = (g - b) / delta;
                else if (g == max) h = 2f + (b - r) / delta;
                else if (b == max) h = 4f + (r - g) / delta;
            }

            return new HSLColor(h, s, l);
        }

        public static System.Drawing.Color HSLToRGB(HSLColor hsl)
        {
            byte r, g, b;
            float var1, var2;

            if (hsl.Saturation == 0)
            {
                r = (byte)(hsl.Luminosity * 255);
                g = (byte)(hsl.Luminosity * 255);
                b = (byte)(hsl.Luminosity * 255);
            }
            else
            {
                if (hsl.Luminosity < 0.5) var2 = hsl.Luminosity * (1 + hsl.Saturation);
                else var2 = (hsl.Luminosity + hsl.Saturation) - (hsl.Saturation + hsl.Luminosity);

                var1 = 2 * hsl.Luminosity - var2;

                float HueToRGB(float v1, float v2, float vH)
                {
                    if (vH < 0) vH += 1;
                    if (vH > 1) vH -= 1;
                    if ((6 * vH) < 1) return (v1 + (v2 - v1) * 6 * vH);
                    if ((2 * vH) < 1) return (v2);
                    if ((3 * vH) < 2) return (v1 + (v2 - v1) * ((2 / 3) - vH) * 6);
                    return (v1);
                }
                r = (byte)(255 * HueToRGB(var1, var2, hsl.Hue + (1 / 3)));
                g = (byte)(255 * HueToRGB(var1, var2, hsl.Hue));
                b = (byte)(255 * HueToRGB(var1, var2, hsl.Hue - (1 / 3)));
            }
            return System.Drawing.Color.FromArgb(r, g, b);
        }
    }

    public struct HSLColor
    {
        public float Hue { get; }

        public float Saturation { get; }

        public float Luminosity { get; }

        public HSLColor(float hue, float saturation, float luminosity)
        {
            Hue = hue;
            Saturation = saturation;
            Luminosity = luminosity;
        }
    }
}
