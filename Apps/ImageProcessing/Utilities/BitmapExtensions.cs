using DedupSharp.Helpers.IO;
using System;
using System.Drawing;

namespace DedupSharp.Apps.ImageProcessing.Utilities
{
    public static class BitmapExtensions
    {
        public static Bitmap AddBorder(this Bitmap source, int borderWidth)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Bitmap bitmap = null;
            Bitmap tempBitmap = null;
            try
            {
                int width = source.Width;
                int height = source.Height;
                tempBitmap = new Bitmap(width, height);
                for (int y = 0; y < height; y++)
                {
                    bool yFlag = (y < borderWidth || (height - y) < borderWidth);
                    for (int x = 0; x < width; x++)
                    {
                        bool xFlag = (x < borderWidth || (width - x) < borderWidth);
                        if (xFlag || yFlag)
                        {
                            var distance = Math.Min(y, Math.Min(height - y, Math.Min(x, width - x)));
                            var percent = distance / (double)borderWidth;
                            var percent2 = percent * percent;
                            var pixel = source.GetPixel(x, y);
                            var color = Color.FromArgb((int)(pixel.R * percent2), (int)(pixel.G * percent2), (int)(pixel.B * percent2));
                            tempBitmap.SetPixel(x, y, color);
                        }
                        else
                        {
                            tempBitmap.SetPixel(x, y, source.GetPixel(x, y));
                        }
                    }
                }
                bitmap = tempBitmap;
                tempBitmap = null;
            }
            finally
            {
                if (tempBitmap != null) tempBitmap.Dispose();
            }
            return bitmap;
        }

        public static Bitmap AddNoise(this Bitmap source, double amount)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            Bitmap bitmap = null;
            Bitmap tempBitmap = null;
            try
            {
                var generator = new GaussianRandom(0.0, amount, BasicUtilities.MakeRandomSeed());
                tempBitmap = new Bitmap(source.Width, source.Height);
                for (int y = 0; y < tempBitmap.Height; y++)
                {
                    for (int x = 0; x < tempBitmap.Width; x++)
                    {
                        var pixel = source.GetPixel(x, y);
                        Color newPixel = AddPixelNoise(pixel, generator);
                        tempBitmap.SetPixel(x, y, newPixel);
                    }
                }
                bitmap = tempBitmap;
                tempBitmap = null;
            }
            finally
            {
                if (tempBitmap != null) tempBitmap.Dispose();
            }
            return bitmap;
        }

        static Color AddPixelNoise(Color pixel, GaussianRandom generator)
        {
            int newR = (int)pixel.R + generator.NextInteger();
            int newG = (int)pixel.G + generator.NextInteger();
            int newB = (int)pixel.B + generator.NextInteger();
            int r = Math.Max(0, Math.Min(newR, 255));
            int g = Math.Max(0, Math.Min(newG, 255));
            int b = Math.Max(0, Math.Min(newB, 255));
            return Color.FromArgb(r, g, b);
        }
    }
}