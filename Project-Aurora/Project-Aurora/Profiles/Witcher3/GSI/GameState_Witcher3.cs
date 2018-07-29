using Aurora.Profiles.Witcher3.GSI.Nodes;
using System;

namespace Aurora.Profiles.Witcher3.GSI
{
    /// <summary>
    /// A class representing various information retaining to Game State Integration of Witcher 3
    /// </summary>
    public class GameState_Witcher3 : GameState<GameState_Witcher3>
    {
        private Player_Witcher3 player;

        public Player_Witcher3 Player
        {
            get
            {
                if (player == null)
                    player = new Player_Witcher3();

                return player;
            }
        }

        /// <summary>
        /// Creates a default GameState_Witcher3 instance.
        /// </summary>
        public GameState_Witcher3() : base()
        {
        }

        /// <summary>
        /// Creates a GameState instance based on the passed json data.
        /// </summary>
        /// <param name="json_data">The passed json data</param>
        public GameState_Witcher3(string json_data) : base(json_data)
        {
        }

        /// <summary>
        /// A copy constructor, creates a GameState_Witcher3 instance based on the data from the passed GameState instance.
        /// </summary>
        /// <param name="other_state">The passed GameState</param>
        public GameState_Witcher3(IGameState other_state) : base(other_state)
        {
        }
    }
}
