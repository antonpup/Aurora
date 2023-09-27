namespace Common.Utils;

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

    public static bool NearlyEqual(float a, float b, float epsilon) {
        const double minNormal = 2.2250738585072014E-308d;
        float absA = Math.Abs(a);
        float absB = Math.Abs(b);
        float diff = Math.Abs(a - b);

        if (a.Equals(b)) { // shortcut, handles infinities
            return true;
        }
        if (a == 0 || b == 0 || absA + absB < minNormal) {
            // a or b is zero or both are extremely close to it
            // relative error is less meaningful here
            return diff < epsilon * minNormal;
        } // use relative error
        return diff / (absA + absB) < epsilon;
    }

    public static bool NearlyEqual(double a, double b, double epsilon) {
        const double minNormal = 2.2250738585072014E-308d;
        double absA = Math.Abs(a);
        double absB = Math.Abs(b);
        double diff = Math.Abs(a - b);

        if (a.Equals(b)) { // shortcut, handles infinities
            return true;
        }
        if (a == 0 || b == 0 || absA + absB < minNormal) {
            // a or b is zero or both are extremely close to it
            // relative error is less meaningful here
            return diff < epsilon * minNormal;
        } // use relative error
        return diff / (absA + absB) < epsilon;
    }
}