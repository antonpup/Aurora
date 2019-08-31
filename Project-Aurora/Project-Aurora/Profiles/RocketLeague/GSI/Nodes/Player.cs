namespace Aurora.Profiles.RocketLeague.GSI.Nodes
{
    /// <summary>
    /// Enum list for each player team
    /// </summary>
    public enum RocketLeagueTeam
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// Blue Team
        /// </summary>
        Blue = 0,

        /// <summary>
        /// Orange Team
        /// </summary>
        Orange = 1,

        /// <summary>
        /// Spectator
        /// </summary>
        Spectator = 2 
    }

    /// <summary>
    /// Class representing player information
    /// </summary>
    public class Player_RocketLeague : Node<Player_RocketLeague>
    {
        public int Team = -1;
        public float Boost = -1;
        public int Score = -1;
        public int Goals = -1;
        public int Assists = -1;
        public int Saves = -1;
        public int Shots = -1;

        internal Player_RocketLeague(string json_data) : base(json_data)
        {
            Boost = GetFloat("boost");
            Score = GetInt("score");
            Goals = GetInt("goals");
            Assists = GetInt("assists");
            Saves = GetInt("saves");
            Shots = GetInt("shots");
            Team = GetInt("team");
        }
    }
}
