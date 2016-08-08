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
        public readonly int Kills;

        /// <summary>
        /// Amount of assists
        /// </summary>
        public readonly int Assists;

        /// <summary>
        /// Amount of deaths
        /// </summary>
        public readonly int Deaths;

        /// <summary>
        /// Amount of MVPs
        /// </summary>
        public readonly int MVPs;

        /// <summary>
        /// The score
        /// </summary>
        public readonly int Score;

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
