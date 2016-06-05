using System.ComponentModel;

namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    public class LevelNode : Node
    {
        public readonly string LevelID;
        public readonly LevelPhase Phase;
        public readonly int NoReturnTime;

        internal LevelNode(string JSON) : base(JSON)
        {
            LevelID = GetString("level_id");
            Phase = GetEnum<LevelPhase>("phase");
            NoReturnTime = (int)GetFloat("no_return_timer");
        }
    }

    public enum LevelPhase
    {
        [Description("Undefined")]
        Undefined,
        [Description("Stealth")]
        Stealth,
        [Description("Loud")]
        Loud,
        [Description("Cptn. Winters")]
        Winters,
        [Description("Assault")]
        Assault,
        [Description("Point of no return")]
        Point_of_no_return
    }
}
