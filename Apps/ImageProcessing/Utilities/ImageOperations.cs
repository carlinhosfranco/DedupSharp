using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;

namespace DedupSharp.Apps.ImageProcessing.Utilities
{
    public class ImageOperations
    {
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
        public static Image LoadImage(string fname, string sourceDir, int count, int clockOffset)
        {
            int startTick = Environment.TickCount;
            Image info = null;
            Bitmap bitmap = new Bitmap(Path.Combine(sourceDir, fname));
            try
            {
                bitmap.Tag = fname;

                info = new Image(count, fname, bitmap, clockOffset);
                info.PhaseStartTick[0] = startTick - clockOffset;
                bitmap = null;
            }
            finally
            {
                if (bitmap != null) bitmap.Dispose();
            }

            if (info != null) info.PhaseEndTick[0] = Environment.TickCount - clockOffset;
            return info;
        }

        public static void ScaleImage(Image info)
        {
            int startTick = Environment.TickCount;
            var orig = info.OriginalImage;
            info.OriginalImage = null;
            const int scale = 200;
            var isLandscape = (orig.Width > orig.Height);
            var newWidth = isLandscape ? scale : scale * orig.Width / orig.Height;
            var newHeight = !isLandscape ? scale : scale * orig.Height / orig.Width;
            Bitmap bitmap = new Bitmap(orig, newWidth, newHeight);
            try
            {
                Bitmap bitmap2 = bitmap.AddBorder(15);
                try
                {
                    bitmap2.Tag = orig.Tag;
                    info.ThumbnailImage = bitmap2;
                    info.PhaseStartTick[1] = startTick - info.ClockOffset;
                    bitmap2 = null;
                }
                finally
                {
                    if (bitmap2 != null) bitmap2.Dispose();
                }
            }
            finally
            {
                bitmap.Dispose();
                orig.Dispose();
            }
            info.PhaseEndTick[1] = Environment.TickCount - info.ClockOffset;
        }

        public static void FilterImage(Image info)
        {
            const double GaussianNoiseAmount = 50.0;
            int startTick = Environment.TickCount;
            var sc = info.ThumbnailImage;
            info.ThumbnailImage = null;
            Bitmap bitmap = sc.AddNoise(GaussianNoiseAmount);

            try
            {
                bitmap.Tag = sc.Tag;
                info.FilteredImage = bitmap;
                info.PhaseStartTick[2] = startTick - info.ClockOffset;

                bitmap = null;
            }
            finally
            {
                if (bitmap != null) bitmap.Dispose();
                sc.Dispose();
            }
            info.PhaseEndTick[2] = Environment.TickCount - info.ClockOffset;
        }

        public static void DisplayImage(Image info, int count, Action<Image> displayFn, int duration)
        {
            int startTick = Environment.TickCount;
            info.ImageCount = count;
            info.PhaseStartTick[3] = startTick - info.ClockOffset;
            info.PhaseEndTick[3] = (duration > 0) ? startTick - info.ClockOffset + duration :
                                                     Environment.TickCount - info.ClockOffset;
            displayFn(info);
        }

    }
}