using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        public static Color GetAverageColor(Image screenshot)
        {
            var scaled_down_image = new Bitmap(16, 16);

            using (var graphics = Graphics.FromImage(scaled_down_image))
                graphics.DrawImage(screenshot, 0, 0, 16, 16);

            Color avg = Utils.ColorUtils.GetAverageColor(scaled_down_image);

            scaled_down_image?.Dispose();

            return avg;
        }

        /// <summary>
        /// Returns a color matrix that when applied to an image alters its brightness.
        /// Taken from https://docs.rainmeter.net/tips/colormatrix-guide/
        /// </summary>
        /// <param name="b">Brightness (0..4)</param>
        /// <returns></returns>
        public static float[][] GetBrightnessColorMatrix(float b) => new float[][] {
                new float[] {b, 0, 0, 0, 0},//red
                new float[] {0, b, 0, 0, 0},//green
                new float[] {0, 0, b, 0, 0},//blue
                new float[] {0, 0, 0, 1, 0},//alpha
                new float[] {0, 0, 0, 0, 1}
        };

        /// <summary>
        /// Returns a color matrix that when applied to an image alters its saturation.
        /// Taken from https://docs.rainmeter.net/tips/colormatrix-guide/
        /// </summary>
        /// <param name="s">Saturation (0..4)</param>
        /// <returns></returns>
        public static float[][] GetSaturationColorMatrix(float s)
        {
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
            return colorMatrix;
        }

        /// <summary>
        /// Returns a color matrix that when applied to an image rotates its hue.
        /// Taken from https://stackoverflow.com/questions/25856660/rotate-hue-in-c-sharp
        /// </summary>
        /// <param name="d">Degrees (0..360)</param>
        /// <returns></returns>
        public static float[][] GetHueShiftColorMatrix(float d)
        {
            float theta = d / 360 * 2 * (float)Math.PI;
            float c = (float)Math.Cos(theta);
            float s = (float)Math.Sin(theta);

            float A00 = 0.213f + 0.787f * c - 0.213f * s;
            float A01 = 0.213f - 0.213f * c + 0.413f * s;
            float A02 = 0.213f - 0.213f * c - 0.787f * s;

            float A10 = 0.715f - 0.715f * c - 0.715f * s;
            float A11 = 0.715f + 0.285f * c + 0.140f * s;
            float A12 = 0.715f - 0.715f * c + 0.715f * s;

            float A20 = 0.072f - 0.072f * c + 0.928f * s;
            float A21 = 0.072f - 0.072f * c - 0.283f * s;
            float A22 = 0.072f + 0.928f * c + 0.072f * s;

            return new float[][] {
                new float[] { A00, A01, A02, 0, 0},
                new float[] { A10, A11, A12, 0, 0},
                new float[] { A20, A21, A22, 0, 0},
                new float[] { 0,     0,   0, 1, 0},
                new float[] { 0,     0,   0, 0, 1}
            };
        }

        /// <summary>
        /// Returns an identity matrix 5x5. Useful to perform operations on, which will then be applied at once to an image
        /// </summary>
        /// <returns></returns>
        public static float[][] GetEmptyColorMatrix() => new float[][] {
                new float[] {1, 0, 0, 0, 0},//red
                new float[] {0, 1, 0, 0, 0},//green
                new float[] {0, 0, 1, 0, 0},//blue
                new float[] {0, 0, 0, 1, 0},//alpha
                new float[] {0, 0, 0, 0, 1}
        };

        /// <summary>
        /// Multiplies two 5x5 matrices together.
        /// Taken from https://www.codeproject.com/Articles/7836/Multiple-Matrices-With-ColorMatrix-in-C
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static float[][] ColorMatrixMultiply(float[][] f1, float[][] f2)
        {
            const int size = 5;
            if (f1.Length != size || f2.Length != size)
                throw new ArgumentException();

            float[][] result = new float[size][];
            for (int d = 0; d < size; d++)
                result[d] = new float[size];

            float[] column = new float[size];
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    column[k] = f1[k][j];
                }
                for (int i = 0; i < size; i++)
                {
                    float[] row = f2[i];
                    float s = 0;
                    for (int k = 0; k < size; k++)
                    {
                        s += row[k] * column[k];
                    }
                    result[i][j] = s;
                }
            }
            return result;
        }

        /// <summary>
        /// Applies a color matrix to a givem Image
        /// </summary>
        /// <param name="img"></param>
        /// <param name="mtx"></param>
        /// <returns></returns>
        public static void ApplyColorMatrix(this Image img, ColorMatrix mtx)
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
        }

        /// <summary>
        /// Adjusts the brightness of an image using a color matrix. brightness is a value between -1 and 1
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static void AdjustBrightness(this Image bmp, float b) =>
            ApplyColorMatrix(bmp, new ColorMatrix(GetBrightnessColorMatrix(b)));

        /// <summary>
        /// Adjusts the saturation of an image using a color matrix. Uses a value between 0 (grayscale) and ~2
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static void AdjustSaturation(this Image bmp, float s) =>
            ApplyColorMatrix(bmp, new ColorMatrix(GetSaturationColorMatrix(s)));
    }
}
