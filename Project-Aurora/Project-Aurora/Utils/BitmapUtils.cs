using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Aurora.Utils
{
    public static class BitmapUtils
    {

        public static Color GetRegionColor(Bitmap map, Rectangle rectangle)
        {
            if (rectangle.IsEmpty)
                return Color.Black;

            //B, G, R, A
            var color = new[] {0L, 0L, 0L, 0L}; //array because SIMD optimizations

            var srcData = map.LockBits(
                rectangle,
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            var stride = srcData.Stride;
            var scan0 = srcData.Scan0;

            unsafe
            {
                var p = (byte*)(void*)scan0;

                for (var y = 0; y < rectangle.Height; y++)
                {
                    var i = y * stride;
                    for (var x = 0; x < rectangle.Width; x++)
                    {
                        var j = i + x * 4;
                        color[0] += p[j];
                        color[1] += p[j + 1];
                        color[2] += p[j + 2];
                        color[3] += p[j + 3];
                    }
                }
            }
            map.UnlockBits(srcData);

            //each color value is rounded to the nearest integer
            //because for bytes each increment is significant
            //https://stackoverflow.com/a/17974/13320838
            var area = rectangle.Width * rectangle.Height;
            return Color.FromArgb(
                (int)((color[0] - 1) / area + 1) |
                (int)((color[1] - 1) / area + 1 << 8) |
                (int)((color[2] - 1) / area + 1 << 16) |
                (int)((color[3] - 1) / area + 1 << 24)
            );
        }

        /// <summary>
        /// Returns a color matrix that when applied to an image alters its brightness.
        /// Taken from https://docs.rainmeter.net/tips/colormatrix-guide/
        /// </summary>
        /// <param name="b">Brightness (0..4)</param>
        /// <returns></returns>
        public static float[][] GetBrightnessColorMatrix(float b) => new[]
        {
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
        public static float[][] GetEmptyColorMatrix() => new [] {
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
    }
}
