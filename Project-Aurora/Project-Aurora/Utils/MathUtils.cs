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
    }
}
