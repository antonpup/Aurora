using System;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    public class MatchStatsNode : Node
    {
        public readonly int Kills;
        public readonly int Assists;
        public readonly int Deaths;
        public readonly int MVPs;
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
