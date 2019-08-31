using Aurora.Profiles.RocketLeague.GSI.Nodes;
using System;

namespace Aurora.Profiles.RocketLeague.GSI
{
    /// <summary>
    /// A class representing various information relating to Rocket League
    /// </summary>
    public class GameState_RocketLeague : GameState<GameState_RocketLeague>
    {
        private Player_RocketLeague _Player;
        private Match_RocketLeague _Match;

        /// <summary>
        /// Information about the local player
        /// </summary>
        public Player_RocketLeague Player
        {
            get
            {
                if (_Player == null)
                    _Player = new Player_RocketLeague(_ParsedData["player"]?.ToString() ?? "");

                return _Player;
            }
        }

        /// <summary>
        /// Information about the local player
        /// </summary>
        public Match_RocketLeague Match
        {
            get
            {
                if (_Match == null)
                    _Match = new Match_RocketLeague(_ParsedData["match"]?.ToString() ?? "");

                return _Match;
            }
        }

        /// <summary>
        /// Creates a default GameState_RocketLeague instance.
        /// </summary>
        public GameState_RocketLeague() : base()
        {
        }

        /// <summary>
        /// Creates a GameState instance based on the passed json data.
        /// </summary>
        /// <param name="json_data">The passed json data</param>
        public GameState_RocketLeague(string json_data) : base(json_data)
        {
        }

        /// <summary>
        /// A copy constructor, creates a GameState_RocketLeague instance based on the data from the passed GameState instance.
        /// </summary>
        /// <param name="other_state">The passed GameState</param>
        public GameState_RocketLeague(IGameState other_state) : base(other_state)
        {
        }
    }
}
