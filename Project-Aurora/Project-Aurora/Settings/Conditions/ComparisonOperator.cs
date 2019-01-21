using System.ComponentModel;

namespace Aurora.Settings.Conditions {
    /// <summary>
    /// Enum listing various logic operators for the numeric condition.
    /// </summary>
    public enum ComparisonOperator {
        [Description("=")] EQ,
        [Description("≠")] NEQ,
        [Description("<")] LT,
        [Description("≤")] LTE,
        [Description(">")] GT,
        [Description("≥")] GTE
    }
}
