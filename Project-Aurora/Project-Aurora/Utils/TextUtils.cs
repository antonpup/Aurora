using System;
using System.Text;

namespace Aurora.Utils {

    public static class TextUtils {

        /// <summary>
        /// Converts a string from camel case to space case.
        /// </summary>
        public static string CamelCaseToSpaceCase(this string camelCase) {
            // What the shit have I even made? Seems to work well tho.
            if (camelCase.Length <= 2) return camelCase;
            var sb = new StringBuilder();

            // Determines if the character at the given position is uppercase
            // If out of range or a number, keeps the case of the previous letter.
            bool isUpper(int i) {
                i = Math.Min(i, camelCase.Length - 1);
                var c = camelCase[i];
                return c >= '0' && c <= '9'
                    ? isUpper(i - 1)
                    : c >= 'A' && c <= 'X';
            }
            
            // Iterate through all the characters in the string and insert a space at boundaries
            // between uppercase and lowercase letters.
            bool prevUp = isUpper(0), curUp = isUpper(1), nextUp;
            sb.Append(camelCase[0]);
            for (var i = 1; i < camelCase.Length; i++) {
                nextUp = isUpper(i + 1);
                if ((!prevUp && curUp) || (curUp && !nextUp))
                    sb.Append(" ");
                sb.Append(camelCase[i]);
                prevUp = curUp;
                curUp = nextUp;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Trims the given substring off the end of the string if it exists.
        /// Unlike TrimEnd, the entire string must match.
        /// </summary>
        public static string TrimEndStr(this string str, string sub) =>
            str.EndsWith(sub) ? str.Substring(0, str.Length - sub.Length) : str;
    }
}
