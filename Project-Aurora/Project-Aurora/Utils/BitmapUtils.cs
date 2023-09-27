using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Common.Utils;

namespace Aurora.Utils;

public static class BitmapUtils
{
    //B, G, R
    private static readonly long[] ColorData = {0L, 0L, 0L};
    private static readonly Dictionary<Size, BitmapData> Bitmaps = new();
    // ReSharper disable once CollectionNeverQueried.Local //to keep reference
    private static readonly Dictionary<Size, int[]> BitmapBuffers = new();

    /**
     * Gets average color of region, ignoring transparency
     * NOT thread-safe if optional parameters are null
     */
    public static Color GetRegionColor(Bitmap map, Rectangle rectangle, long[]? color = null)
    {
        var graphicsUnit = GraphicsUnit.Pixel;
        if (rectangle.Width == 0 || rectangle.Height == 0 || !map.GetBounds(ref graphicsUnit).Contains(rectangle))
            return Color.Black;

        color ??= ColorData;
        color[0] = 0L;
        color[1] = 0L;
        color[2] = 0L;

        if (!Bitmaps.TryGetValue(rectangle.Size, out var buff))
        {
            var bitmapBuffer = new int[rectangle.Width * rectangle.Height];
            BitmapBuffers[rectangle.Size] = bitmapBuffer;

            var buffer = Marshal.AllocHGlobal(bitmapBuffer.Length * sizeof(int));
            Marshal.Copy(bitmapBuffer, 0, buffer, bitmapBuffer.Length);
            // Create new bitmap data.
            buff = new BitmapData
            {
                Width = rectangle.Width,
                Height = rectangle.Height,
                PixelFormat = PixelFormat.Format32bppArgb,
                Stride = rectangle.Width * sizeof(int),
                Scan0 = buffer
            };
            
            Bitmaps[rectangle.Size] = buff;
        }

        var srcData = map.LockBits(
            rectangle,
            ImageLockMode.UserInputBuffer | ImageLockMode.ReadOnly,
            PixelFormat.Format32bppRgb, buff);
        var scan0 = srcData.Scan0;

        var rectangleHeight = rectangle.Height;
        var rectangleWidth = rectangle.Width;
        unsafe
        {
            var p = (byte*)(void*)scan0;

            var j = 0;
            for (var y = 0; y < rectangleHeight; y++)
            {
                for (var x = 0; x < rectangleWidth; x++)
                {
                    color[0] += p[j++];
                    color[1] += p[j++];
                    color[2] += p[j++];
                    j++; //not going to use color[3], devices don't need Alpha
                }
            }
        }
        map.UnlockBits(srcData);

        var area = rectangle.Width * rectangle.Height;
        return CommonColorUtils.FastColor(
            (byte) (color[2] / area), (byte) (color[1] / area), (byte) (color[0] / area)
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
        new [] {b, 0, 0, 0, 0},//red
        new [] {0, b, 0, 0, 0},//green
        new [] {0, 0, b, 0, 0},//blue
        new [] {0f, 0, 0, 1, 0},//alpha
        new [] {0f, 0, 0, 0, 1}
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
        var sr = (1 - s) * lumR;
        var sg = (1 - s) * lumG;
        var sb = (1 - s) * lumB;

        return new[] {
            new [] {sr + s, sr,     sr,     0, 0},
            new [] {sg,     sg + s, sg,     0, 0},
            new [] {sb,     sb,     sb + s, 0, 0},
            new [] {0f,      0,      0,     1, 0},
            new [] {0f,      0,      0,     0, 1}
        };
    }

    /// <summary>
    /// Returns a color matrix that when applied to an image rotates its hue.
    /// Taken from https://stackoverflow.com/questions/25856660/rotate-hue-in-c-sharp
    /// </summary>
    /// <param name="d">Degrees (0..360)</param>
    /// <returns></returns>
    public static float[][] GetHueShiftColorMatrix(float d)
    {
        var theta = d / 360 * 2 * (float)Math.PI;
        var c = (float)Math.Cos(theta);
        var s = (float)Math.Sin(theta);

        var a00 = 0.213f + 0.787f * c - 0.213f * s;
        var a01 = 0.213f - 0.213f * c + 0.413f * s;
        var a02 = 0.213f - 0.213f * c - 0.787f * s;

        var a10 = 0.715f - 0.715f * c - 0.715f * s;
        var a11 = 0.715f + 0.285f * c + 0.140f * s;
        var a12 = 0.715f - 0.715f * c + 0.715f * s;

        var a20 = 0.072f - 0.072f * c + 0.928f * s;
        var a21 = 0.072f - 0.072f * c - 0.283f * s;
        var a22 = 0.072f + 0.928f * c + 0.072f * s;

        return new[]
        {
            new [] { a00, a01, a02, 0, 0},
            new [] { a10, a11, a12, 0, 0},
            new [] { a20, a21, a22, 0, 0},
            new [] { 0f,    0,   0, 1, 0},
            new [] { 0f,    0,   0, 0, 1}
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

        var result = new float[size][];
        for (var d = 0; d < size; d++)
            result[d] = new float[size];

        var column = new float[size];
        for (var j = 0; j < size; j++)
        {
            for (var k = 0; k < size; k++)
            {
                column[k] = f1[k][j];
            }
            for (var i = 0; i < size; i++)
            {
                var row = f2[i];
                float s = 0;
                for (var k = 0; k < size; k++)
                {
                    s += row[k] * column[k];
                }
                result[i][j] = s;
            }
        }
        return result;
    }
}