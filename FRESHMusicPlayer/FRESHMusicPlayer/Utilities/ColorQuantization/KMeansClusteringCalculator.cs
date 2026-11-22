using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FRESHMusicPlayer.Utilities.ColorQuantization
{
    internal static class KMeansClusteringCalculator
    {
        public static List<Color> CalculateBgra(
            int clusterCount, byte[] pixels, int width, int height, int rowBytes, double convergenceThreshold)
        {
            var pixelCount = width * height;
            clusterCount = Math.Min(clusterCount, pixelCount);
            var clusters = new List<KCluster>();

            // A fixed seed gives the same starting centers for the same image.
            var random = new Random(473);
            var chosenPixels = new HashSet<int>();
            while (clusters.Count < clusterCount)
            {
                var pixelIndex = random.Next(pixelCount);
                if (!chosenPixels.Add(pixelIndex))
                {
                    continue;
                }

                // Pick starting centers in the same column-first order as the pixel loop.
                var x = pixelIndex / height;
                var y = pixelIndex % height;
                var offset = y * rowBytes + x * 4;
                var color = Color.FromArgb(pixels[offset + 3], pixels[offset + 2], pixels[offset + 1], pixels[offset]);
                clusters.Add(new KCluster(color));
            }

            bool centersMoved;
            do
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        var offset = y * rowBytes + x * 4;
                        var blue = pixels[offset];
                        var green = pixels[offset + 1];
                        var red = pixels[offset + 2];
                        var shortestDistance = int.MaxValue;
                        var closestCluster = clusters[0];

                        foreach (var cluster in clusters)
                        {
                            var distance = cluster.SquaredDistanceFromCenter(red, green, blue);
                            if (distance < shortestDistance)
                            {
                                shortestDistance = distance;
                                closestCluster = cluster;
                            }
                        }

                        closestCluster.Add(red, green, blue);
                    }
                }

                centersMoved = false;
                foreach (var cluster in clusters)
                {
                    if (cluster.RecalculateCenter(convergenceThreshold))
                    {
                        centersMoved = true;
                    }
                }
            } while (centersMoved);

            return clusters
                .OrderByDescending(cluster => cluster.PreviousPixelCount)
                .Select(cluster => cluster.Center)
                .ToList();
        }
    }
}
