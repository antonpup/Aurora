using Aurora.Profiles.CloneHero.GSI.Nodes;
using System;

namespace Aurora.Profiles.CloneHero.GSI
{
    /// <summary>
    /// A class representing various information relating to CloneHero
    /// </summary>
    public class GameState_CloneHero : GameState
    {
        private Player_CloneHero player;

        /// <summary>
        /// Information about the local player
        /// </summary>
        public Player_CloneHero Player
        {
            get
            {
                if (player == null)
                    player = new Player_CloneHero("");

                return player;
            }
        }

        /// <summary>
        /// Creates a default GameState_CloneHero instance.
        /// </summary>
        public GameState_CloneHero() : base()
        {
        }

        /// <summary>
        /// Creates a GameState instance based on the passed json data.
        /// </summary>
        /// <param name="json_data">The passed json data</param>
        public GameState_CloneHero(string json_data) : base(json_data)
        {
        }
    }
}
