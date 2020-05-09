using Aurora.Profiles.Dishonored.GSI.Nodes;
using System;

namespace Aurora.Profiles.Dishonored.GSI
{
    /// <summary>
    /// A class representing various information relating to Dishonored
    /// </summary>
    public class GameState_Dishonored : GameState<GameState_Dishonored>
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

        /// <summary>
        /// Creates a default GameState_Dishonored instance.
        /// </summary>
        public GameState_Dishonored() : base()
        {
        }

        /// <summary>
        /// Creates a GameState instance based on the passed json data.
        /// </summary>
        /// <param name="json_data">The passed json data</param>
        public GameState_Dishonored(string json_data) : base(json_data)
        {
        }
    }
}
