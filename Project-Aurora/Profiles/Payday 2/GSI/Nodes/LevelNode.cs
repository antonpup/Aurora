using System.ComponentModel;

namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    /// <summary>
    /// Information about the level
    /// </summary>
    public class LevelNode : Node
    {
        /// <summary>
        /// Level ID
        /// </summary>
        public readonly string LevelID;

        /// <summary>
        /// Level phase
        /// </summary>
        public readonly LevelPhase Phase;

        /// <summary>
        /// Counter for point of no return
        /// </summary>
        public readonly int NoReturnTime;

        internal LevelNode(string JSON) : base(JSON)
        {
            LevelID = GetString("level_id");
            Phase = GetEnum<LevelPhase>("phase");
            NoReturnTime = (int)GetFloat("no_return_timer");
        }
    }

    /// <summary>
    /// Enum for each level phase
    /// </summary>
    public enum LevelPhase
    {
        /// <summary>
        /// Undefined
        /// </summary>
        [Description("Undefined")]
        Undefined,

        /// <summary>
        /// Stealth
        /// </summary>
        [Description("Stealth")]
        Stealth,

        /// <summary>
        /// Loud
        /// </summary>
        [Description("Loud")]
        Loud,

        /// <summary>
        /// Captain Winterss
        /// </summary>
        [Description("Cptn. Winters")]
        Winters,

        /// <summary>
        /// Assault
        /// </summary>
        [Description("Assault")]
        Assault,

        /// <summary>
        /// Point of no Return
        /// </summary>
        [Description("Point of no return")]
        Point_of_no_return
    }
}
