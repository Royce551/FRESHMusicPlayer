﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.IO;

namespace FRESHMusicPlayer.Controls.BlurryImage
{
    // Code taken from https://github.com/PieroCastillo/Aura.UI/blob/master/src/Aura.UI/Controls/BlurryImage/BlurryImage.cs;
    // I just didn't want to depend on everything else Aura.UI has

    public class BlurryImage : Control
    {
        static BlurryImage()
        {
            AffectsRender<BlurryImage>(BlurLevelProperty, SourceProperty, StretchDirectionProperty, StretchProperty);
            AffectsMeasure<BlurryImage>(BlurLevelProperty, SourceProperty, StretchDirectionProperty, StretchProperty);
            AffectsArrange<BlurryImage>(BlurLevelProperty, SourceProperty, StretchDirectionProperty, StretchProperty);

            ClipToBoundsProperty.OverrideDefaultValue<BlurryImage>(true);
        }

        public override void Render(DrawingContext context)
        {
            if (Source is null) return;
            var source = Source;
            var mem = new MemoryStream();
            Source.Save(mem);

            if (source != null && mem.Length > 0 && Bounds.Width > 0 && Bounds.Height > 0)
            {
                Rect viewPort = new Rect(Bounds.Size);
                Size sourceSize = source.Size;

                Vector scale = Stretch.CalculateScaling(Bounds.Size, sourceSize, StretchDirection);
                Size scaledSize = sourceSize * scale;
                Rect destRect = viewPort
                    .CenterRect(new Rect(scaledSize))
                    .Intersect(viewPort);
                Rect sourceRect = new Rect(sourceSize)
                    .CenterRect(new Rect(destRect.Size / scale));

                var interpolationMode = RenderOptions.GetBitmapInterpolationMode(this);
                context.Custom(new BlurImageRender(mem, destRect, sourceRect, BlurLevel, BlurLevel, null));
                // Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
            }
        }

        ///<inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
            var source = Source;
            var result = new Size();

            if (source != null)
            {
                result = Stretch.CalculateSize(availableSize, source.Size, StretchDirection);
            }

            return result;
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var source = Source;

            if (source != null)
            {
                var sourceSize = source.Size;
                var result = Stretch.CalculateSize(finalSize, sourceSize);
                return result;
            }
            else
            {
                return new Size();
            }
        }

        public float BlurLevel
        {
            get => GetValue(BlurLevelProperty);
            set => SetValue(BlurLevelProperty, value);
        }

        public IBitmap Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public Stretch Stretch
        {
            get => GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        public StretchDirection StretchDirection
        {
            get => GetValue(StretchDirectionProperty);
            set => SetValue(StretchDirectionProperty, value);
        }

        public readonly static StyledProperty<IBitmap> SourceProperty =
            AvaloniaProperty.Register<BlurryImage, IBitmap>(nameof(Source));

        public readonly static StyledProperty<Stretch> StretchProperty =
            Image.StretchProperty.AddOwner<BlurryImage>();

        public readonly static StyledProperty<StretchDirection> StretchDirectionProperty =
            Image.StretchDirectionProperty.AddOwner<BlurryImage>();

        public readonly static StyledProperty<float> BlurLevelProperty =
            AvaloniaProperty.Register<BlurryImage, float>(nameof(BlurLevel), 16);
    }
}
