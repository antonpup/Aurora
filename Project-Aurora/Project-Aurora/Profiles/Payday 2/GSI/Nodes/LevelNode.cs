using System.ComponentModel;

namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    /// <summary>
    /// Information about the level
    /// </summary>
    public class LevelNode : AutoJsonNode<LevelNode>
    {
        /// <summary>
        /// Level ID
        /// </summary>
        [AutoJsonPropertyName("level_id")]
        public string LevelID;

        /// <summary>
        /// Level phase
        /// </summary>
        public LevelPhase Phase;

        /// <summary>
        /// Counter for point of no return
        /// </summary>
        [AutoJsonPropertyName("no_return_timer")]
        public int NoReturnTime;

        internal LevelNode(string JSON) : base(JSON) { }
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
