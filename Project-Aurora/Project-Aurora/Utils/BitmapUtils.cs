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
                Global.logger.Error("BitmapUtils.GetRegionColor() Exception: " + exc);

                return Color.FromArgb(0, 0, 0);
            }
        }

        public static Color GetRegionColor(Bitmap map, Rectangle rectangle)
        {
            return GetRegionColor(map, new BitmapRectangle(rectangle));
        }


        /// <summary>
        /// Applies a color matrix to a givem Image
        /// </summary>
        /// <param name="img"></param>
        /// <param name="mtx"></param>
        /// <returns></returns>
        public static Image ApplyColorMatrix(Image img, ColorMatrix mtx)
        {
            using (var att = new ImageAttributes())
            {
                att.SetColorMatrix(mtx);
                using (var g = Graphics.FromImage(img))
                {
                    g.DrawImage(img,
                                new Rectangle(0, 0, img.Width, img.Height),
                                0,
                                0,
                                img.Width,
                                img.Height,
                                GraphicsUnit.Pixel,
                                att);
                }
            }
            return img;
        }

        /// <summary>
        /// Adjusts the brightness of an image using a color matrix. brightness is a value between -1 and 1
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Image AdjustImageBrightness(Image bmp, float b)
        {
            //https://docs.rainmeter.net/tips/colormatrix-guide/

            float[][] colorMatrix ={
                new float[] {1, 0, 0, 0, 0},//red
                new float[] {0, 1, 0, 0, 0},//green
                new float[] {0, 0, 1, 0, 0},//blue
                new float[] {0, 0, 0, 1, 0},//alpha
                new float[] {b, b, b, 0, 1}
            };

            return ApplyColorMatrix(bmp, new ColorMatrix(colorMatrix));
        }

        /// <summary>
        /// Adjusts the saturation of an image using a color matrix. Uses a value between 0 (grayscale) and ~2
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Image AdjustImageSaturation(Image bmp, float s)
        {
            //https://docs.rainmeter.net/tips/colormatrix-guide/

            const float lumR = 0.3086f;
            const float lumG = 0.6094f;
            const float lumB = 0.0820f;
            float sr = (1 - s) * lumR;
            float sg = (1 - s) * lumG;
            float sb = (1 - s) * lumB;

            float[][] colorMatrix ={
                new float[] {sr + s, sr,     sr,     0, 0},
                new float[] {sg,     sg + s, sg,     0, 0},
                new float[] {sb,     sb,     sb + s, 0, 0},
                new float[] {0,      0,      0,      1, 0},
                new float[] {0,      0,      0,      0, 1}
            };

            return ApplyColorMatrix(bmp, new ColorMatrix(colorMatrix));
        }

        public static Color GetAverageColor(Image screenshot)
        {
            var scaled_down_image = new Bitmap(16, 16);

            using (var graphics = Graphics.FromImage(scaled_down_image))
                graphics.DrawImage(screenshot, 0, 0, 16, 16);

            Color avg = Utils.ColorUtils.GetAverageColor(scaled_down_image);

            scaled_down_image?.Dispose();

            return avg;
        }
    }
}
