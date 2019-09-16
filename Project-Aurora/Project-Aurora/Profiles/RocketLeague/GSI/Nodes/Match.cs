namespace Aurora.Profiles.RocketLeague.GSI.Nodes
{
    /// <summary>
    /// Represents the match type in rocketleague
    /// </summary>
    public enum RocketLeagueMatchType
    {
        Replay,
        OnlineGame,
        Freeplay,
        Training,
        Spectate,
        None
    };

    public enum RocketLeagueGameMode
    {
        Soccar,
        Hoops,
        SnowDay,
        Dropshot
    }

    /// <summary>
    /// Class representing match information
    /// </summary>
    public class Match_RocketLeague : Node<Match_RocketLeague>
    {
        /// <summary>
        /// The type the current match is
        /// </summary>
        public int Type;

        /// <summary>
        /// The current mode being played
        /// </summary>
        public int Mode;

        /// <summary>
        /// The Blue team playing in the match
        /// </summary>
        public Team_RocketLeague Blue;

        /// <summary>
        /// The Blue team playing in the match
        /// </summary>
        public Team_RocketLeague Orange;

        /// <summary>
        /// Remaining seconds in the match
        /// </summary>
        public int RemainingSeconds = 0;

        internal Match_RocketLeague(string json_data) : base(json_data)
        {
            Blue = new Team_RocketLeague(_ParsedData["team_0"]?.ToString() ?? "");
            Orange = new Team_RocketLeague(_ParsedData["team_1"]?.ToString() ?? "");
            Type = GetInt("type");
            Mode = GetInt("mode");
            RemainingSeconds = GetInt("time");
        }
    }
}
