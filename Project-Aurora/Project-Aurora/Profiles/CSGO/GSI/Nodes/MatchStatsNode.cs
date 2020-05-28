using System;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    /// <summary>
    /// Class representing various player statistics
    /// </summary>
    public class MatchStatsNode : Node
    {
        /// <summary>
        /// Amount of kills
        /// </summary>
        public int Kills;

        /// <summary>
        /// Amount of assists
        /// </summary>
        public int Assists;

        /// <summary>
        /// Amount of deaths
        /// </summary>
        public int Deaths;

        /// <summary>
        /// Amount of MVPs
        /// </summary>
        public int MVPs;

        /// <summary>
        /// The score
        /// </summary>
        public int Score;

        internal MatchStatsNode(string JSON)
            : base(JSON)
        {
            Kills = GetInt("kills");
            Assists = GetInt("assists");
            Deaths = GetInt("deaths");
            MVPs = GetInt("mvps");
            Score = GetInt("score");
        }
    }
}
