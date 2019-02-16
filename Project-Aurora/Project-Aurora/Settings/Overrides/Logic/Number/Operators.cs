using System.ComponentModel;

namespace Aurora.Settings.Overrides.Logic {
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

    /// <summary>
    /// Enum listing various mathematical operators for the numeric expressions.
    /// </summary>
    public enum MathsOperator {
        [Description("+")] Add,
        [Description("-")] Sub,
        [Description("×")] Mul,
        [Description("÷")] Div,
        [Description("Mod")] Mod
    }
}
