using System.ComponentModel;

namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    public class GameNode : Node
    {
        public readonly GameStates State;

        internal GameNode(string JSON) : base(JSON)
        {
            State = GetEnum<GameStates>("state");
        }
    }

    public enum GameStates
    {
        [Description("Undefined")]
        Undefined,
        [Description("None")]
        None,
        [Description("Pause Menu")]
        Menu_Pause,
        [Description("Ingame lobby")]
        Kit_menu,
        [Description("In-game")]
        Ingame,
        [Description("Card Drop")]
        Loot_menu,
        [Description("Mission failed")]
        Mission_end_success,
        [Description("Mission success")]
        Mission_end_failure
    }
}
