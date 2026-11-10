using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace FRESHMusicPlayer.Utilities.ColorQuantization;

internal static class CoverArtAccentColors
{
    public static List<Color> ExtractPalette(Bitmap bitmap)
    {
        using var readable = new WriteableBitmap(bitmap.PixelSize, bitmap.Dpi, PixelFormat.Bgra8888, AlphaFormat.Unpremul);
        using var framebuffer = readable.Lock();
        bitmap.CopyPixels(framebuffer);
        var pixels = new byte[framebuffer.RowBytes * framebuffer.Size.Height];
        Marshal.Copy(framebuffer.Address, pixels, 0, pixels.Length);

        var clustering = new KMeansClusteringCalculator();
        var dominantColors = clustering.CalculateBgra(5, pixels, framebuffer.Size.Width, framebuffer.Size.Height, framebuffer.RowBytes, 5.0d);

        var palette = dominantColors.Where(color => color.A != 0).Select(color => Color.FromRgb(color.R, color.G, color.B)).Distinct().ToList();

        const double minimumVibrancy = 0.2;
        var colorfulPalette = palette.Where(color => GetVibrancy(color) >= minimumVibrancy).ToList();
        if (colorfulPalette.Count > 0) palette = colorfulPalette;

        return palette;
    }

    public static (Color primary, Color secondary) SelectColors(IReadOnlyList<Color> palette, bool isDarkTheme)
    {
        var primaryColor = AdjustForTheme(palette[0], isDarkTheme);
        var secondaryCandidates = palette.Skip(1).OrderByDescending(GetVibrancy).ToList();
        var secondaryColor = secondaryCandidates.FirstOrDefault(color => IsSuitableForTheme(color, isDarkTheme), secondaryCandidates.FirstOrDefault(palette[0]));
        secondaryColor = AdjustForTheme(secondaryColor, isDarkTheme);
        return (primaryColor, secondaryColor);
    }

    private static bool IsSuitableForTheme(Color color, bool isDarkTheme)
    {
        var brightness = (color.R * 0.2126 + color.G * 0.7152 + color.B * 0.0722) / 255;
        return isDarkTheme ? brightness >= 0.35 : brightness <= 0.65;
    }

    private static Color AdjustForTheme(Color color, bool isDarkTheme)
    {
        var hsl = color.ToHsl();
        var lightness = hsl.L;
        while (!IsSuitableForTheme(color, isDarkTheme))
        {
            lightness = Math.Clamp(lightness + (isDarkTheme ? 0.01 : -0.01), 0, 1);
            color = new HslColor(hsl.A, hsl.H, hsl.S, lightness).ToRgb();
        }
        return color;
    }

    private static double GetVibrancy(Color color)
    {
        var hsl = color.ToHsl();
        return hsl.S * (1 - Math.Abs(2 * hsl.L - 1));
    }
}
