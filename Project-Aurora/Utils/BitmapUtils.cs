using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils
{
    public static class BitmapUtils
    {
        public static Color GetRegionColor(Bitmap map, BitmapRectangle rectangle)
        {
            try
            {
                if (rectangle.IsEmpty)
                    return Color.FromArgb(0, 0, 0);

                long Red = 0;
                long Green = 0;
                long Blue = 0;
                long Alpha = 0;

                BitmapData srcData = map.LockBits(
                    rectangle.Rectangle,
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                int stride = srcData.Stride;

                IntPtr Scan0 = srcData.Scan0;

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;

                    for (int y = 0; y < rectangle.Height; y++)
                    {
                        for (int x = 0; x < rectangle.Width; x++)
                        {
                            Blue += p[(y * stride) + x * 4];
                            Green += p[(y * stride) + x * 4 + 1];
                            Red += p[(y * stride) + x * 4 + 2];
                            Alpha += p[(y * stride) + x * 4 + 3];
                        }
                    }
                }

                map.UnlockBits(srcData);

                return Color.FromArgb((int)(Alpha / rectangle.Area), (int)(Red / rectangle.Area), (int)(Green / rectangle.Area), (int)(Blue / rectangle.Area));
            }
            catch (Exception exc)
            {
                Global.logger.LogLine("BitmapUtils.GetRegionColor() Exception: " + exc, Logging_Level.Error);

                return Color.FromArgb(0, 0, 0);
            }
        }

        public static Color GetRegionColor(Bitmap map, Rectangle rectangle)
        {
            return GetRegionColor(map, new BitmapRectangle(rectangle));
        }
    }
}
