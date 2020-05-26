using Aurora.Profiles.Dishonored.GSI.Nodes;

namespace Aurora.Profiles.Dishonored.GSI
{
    /// <summary>
    /// A class representing various information relating to Dishonored
    /// </summary>
    public class GameState_Dishonored : GameState
    {
        private Player_Dishonored player;

        /// <summary>
        /// Information about the local player
        /// </summary>
        public Player_Dishonored Player
        {
            get
            {
                if (player == null)
                    player = new Player_Dishonored("");

                return player;
            }
        }


        public GameState_Dishonored() : base() { }
        public GameState_Dishonored(string json_data) : base(json_data) { }
    }
}
