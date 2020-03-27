using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils {

    /// <summary>
    /// Static class that provides additional maths functions for Aurora.
    /// </summary>
    public static class MathUtils {

        /// <summary>Constrains the given value 'v' so that it is within 'min' and 'max'.</summary>
        public static int Clamp(int v, int min, int max) => Math.Min(Math.Max(v, min), max);

        /// <summary>Constrains the given value 'v' so that it is within 'min' and 'max'.</summary>
        public static float Clamp(float v, float min, float max) => Math.Min(Math.Max(v, min), max);

        /// <summary>Constrains the given value 'v' so that it is within 'min' and 'max'.</summary>
        public static double Clamp(double v, double min, double max) => Math.Min(Math.Max(v, min), max);

        public static float[][] MatrixMultiply(float[][] f1, float[][] f2)
        {
            float[][] X = new float[5][];
            for (int d = 0; d < 5; d++)
                X[d] = new float[5];
            int size = 5;
            float[] column = new float[5];
            for (int j = 0; j < 5; j++)
            {
                for (int k = 0; k < 5; k++)
                {
                    column[k] = f1[k][j];
                }
                for (int i = 0; i < 5; i++)
                {
                    float[] row = f2[i];
                    float s = 0;
                    for (int k = 0; k < size; k++)
                    {
                        s += row[k] * column[k];
                    }
                    X[i][j] = s;
                }
            }
            return X;
        }
    }
}
