namespace Aurora.Profiles.RocketLeague.GSI.Nodes
{
    /// <summary>
    /// Class representing player information
    /// </summary>
    public class Player_RocketLeague : AutoJsonNode<Player_RocketLeague>
    {
        /// <summary>
        /// The Index of the team the player is on
        /// </summary>
        public int Team = -1;

        /// <summary>
        /// Amount of boost (0-1)
        /// </summary>
        public float Boost = -1;

        /// <summary>
        /// Number of points the player has on the scoreboard
        /// </summary>
        public int Score = -1;

        /// <summary>
        /// Number of goals the player scored
        /// </summary>
        public int Goals = -1;

        /// <summary>
        /// Number of assists the player has
        /// </summary>
        public int Assists = -1;

        /// <summary>
        /// Number of saves the player has
        /// </summary>
        public int Saves = -1;

        /// <summary>
        /// Number of shots the player has
        /// </summary>
        public int Shots = -1;

        internal Player_RocketLeague(string json_data) : base(json_data) { }
    }
}
