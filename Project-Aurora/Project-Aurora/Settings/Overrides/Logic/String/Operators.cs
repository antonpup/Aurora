using System.ComponentModel;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Enum listing various logic operators for the string comparison.
    /// </summary>
    public enum StringComparisonOperator {
        [Description("Equals")] Equal,
        [Description("Doesn't equal")] NotEqual,
        [Description("Alphabetical Before")] Before,
        [Description("Alphabetical After")] After,

        [Description("Equal length")] EqualLength,
        [Description("Shorter than")] ShorterThan,
        [Description("Longer than")] LongerThan,

        [Description("Starts with")] StartsWith,
        [Description("Ends with")] EndsWith,
        [Description("Contains")] Contains
    }
}
