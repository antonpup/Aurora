using System.ComponentModel;

namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    /// <summary>
    /// Information about the game
    /// </summary>
    public class GameNode : AutoJsonNode<GameNode>
    {
        /// <summary>
        /// The game state
        /// </summary>
        public GameStates State;

        internal GameNode(string JSON) : base(JSON) { }
    }

    /// <summary>
    /// Enum for each game state
    /// </summary>
    public enum GameStates
    {
        /// <summary>
        /// Undefined
        /// </summary>
        [Description("Undefined")]
        Undefined,

        /// <summary>
        /// None
        /// </summary>
        [Description("None")]
        None,

        /// <summary>
        /// In pause menu
        /// </summary>
        [Description("Pause Menu")]
        Menu_Pause,

        /// <summary>
        /// In the in-game lobby
        /// </summary>
        [Description("Ingame lobby")]
        Kit_menu,

        /// <summary>
        /// In-game
        /// </summary>
        [Description("In-game")]
        Ingame,

        /// <summary>
        /// In the card drop screen
        /// </summary>
        [Description("Card Drop")]
        Loot_menu,

        /// <summary>
        /// In the mission failed screen
        /// </summary>
        [Description("Mission failed")]
        Mission_end_success,

        /// <summary>
        /// In the mission success screen
        /// </summary>
        [Description("Mission success")]
        Mission_end_failure
    }
}
