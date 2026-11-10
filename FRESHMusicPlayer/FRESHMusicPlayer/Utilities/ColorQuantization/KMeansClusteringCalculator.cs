using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FRESHMusicPlayer.Utilities.ColorQuantization
{
    /// <summary>
    /// Calculates the K-Means Clusters for a set of colors
    /// </summary>
    public class KMeansClusteringCalculator
    {
        /// <summary>
        /// Calculates the <paramref name="k"/> clusters for <paramref name="colors"/>. Iterations continues until clusters move by less than <paramref name="threshold"/>
        /// </summary>
        /// <param name="k">The number of clusters to calculate (eg. The number of results to return)</param>
        /// <param name="colors">The list of colors to calculate <paramref name="k"/> for</param>
        /// <param name="threshold">Threshold for iteration. A lower value should produce more correct results but requires more iterations and for some <paramref name="colors"/> may never produce a result</param>
        /// <returns>The <paramref name="k"/> colors for the image in descending order from most common to least common</returns>
        public IList<Color> Calculate(int k, IList<Color> colors, double threshold = 0.0d)
            => Calculate(k, colors.Count, 1, (x, _) => colors[x].ToArgb(), threshold);

        internal IList<Color> CalculateBgra(int k, byte[] pixels, int width, int height, int rowBytes, double threshold)
            => Calculate(k, width, height, (x, y) => BinaryPrimitives.ReadInt32LittleEndian(pixels.AsSpan(y * rowBytes + x * 4, 4)), threshold);

        private static IList<Color> Calculate(int k, int width, int height, Func<int, int, int> getPixel, double threshold)
        {
            var pixelCount = width * height;
            k = Math.Min(k, pixelCount);
            if (k <= 0) return [];

            var clusters = new List<KCluster>();

            // 1. Initialization.
            //   Create K clusters with a random data point from our input.
            //   We make sure not to use the same index twice for two inputs
            var random = new Random(473);  // absolutely arbitrary, most important thing is that we get consistent results
            var usedIndexes = new List<int>();
            while (clusters.Count < k)
            {
                var index = random.Next(0, pixelCount);
                if (usedIndexes.Contains(index)) continue;

                usedIndexes.Add(index);
                // Match the column-first order used when building the original color list.
                var cluster = new KCluster(Color.FromArgb(getPixel(index / height, index % height)));
                clusters.Add(cluster);
            }

            bool updated = false;
            do
            {
                updated = false;
                // 2. For each color in our input determine which cluster's center point is the closest and add the color to the cluster
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        var argb = getPixel(x, y);
                        var red = (argb >> 16) & 0xFF;
                        var green = (argb >> 8) & 0xFF;
                        var blue = argb & 0xFF;
                        int shortestDistance = int.MaxValue;
                        KCluster? closestCluster = null;

                        foreach (var cluster in clusters)
                        {
                            var distance = cluster.SquaredDistanceFromCenter(red, green, blue);
                            if (distance < shortestDistance)
                            {
                                shortestDistance = distance;
                                closestCluster = cluster;
                            }
                        }

                        closestCluster?.Add(red, green, blue);
                    }
                }

                // 3. Recalculate the clusters center.
                foreach (var cluster in clusters)
                {
                    if (cluster.RecalculateCenter(threshold)) updated = true;
                }

                // 4. If we updated any center point this iteration then iterate again
            } while (updated);

            return clusters.OrderByDescending(c => c.PriorCount).Select(c => c.Center).ToList();
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

                if (pR == max) h = (pG - pB) / delta;
                else if (pG == max) h = 2f + (pB - pR) / delta;
                else if (pB == max) h = 4f + (pR - pG) / delta;

                h /= 6f;
                if (h < 0) h += 1f;
            }

            return new HSLColor(h, s, l);
        }

        public static Color HSLToRGB(HSLColor hsl)
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
                else var2 = (hsl.Luminosity + hsl.Saturation) - (hsl.Saturation * hsl.Luminosity);

                var1 = 2 * hsl.Luminosity - var2;

                float HueToRGB(float v1, float v2, float vH)
                {
                    if (vH < 0) vH += 1;
                    if (vH > 1) vH -= 1;
                    if ((6 * vH) < 1) return (v1 + (v2 - v1) * 6 * vH);
                    if ((2 * vH) < 1) return (v2);
                    if ((3 * vH) < 2) return (v1 + (v2 - v1) * ((2f / 3f) - vH) * 6);
                    return (v1);
                }
                r = (byte)(255 * HueToRGB(var1, var2, hsl.Hue + (1f / 3f)));
                g = (byte)(255 * HueToRGB(var1, var2, hsl.Hue));
                b = (byte)(255 * HueToRGB(var1, var2, hsl.Hue - (1f / 3f)));
            }
            return Color.FromArgb(r, g, b);
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
