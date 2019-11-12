
namespace Aurora.Profiles.RocketLeague.GSI.Nodes
{
    public enum RLPlaylist
    {
        Undefined = -1,
        Duel = 1,
        Doubles = 2,
        Standard = 3,
        Chaos = 4,
        PrivateMatch = 6,
        OfflineSeason = 7,
        OfflineSplitscreen = 8,
        Training = 9,
        RankedDuel = 10,
        RankedDoubles = 11,
        RankedSoloStandard = 12,
        RankedStandard = 13,
        MutatorMashup = 14,
        Snowday = 15,
        Rocketlabs = 16,
        Hoops = 17,
        Rumble = 18,
        Workshop = 19,
        TrainingEditor = 20,
        CustomTraining = 21,
        Tournament = 22,
        Dropshot = 23,
        RankedHoops = 27,
        RankedRumble = 28,
        RankedDropshot = 29,
        RankedSnowday = 30
    }

    /// <summary>
    /// Class representing match information
    /// </summary>
    public class Match_RocketLeague : Node<Match_RocketLeague>
    {
        /// <summary>
        /// The current mode being played
        /// </summary>
        public RLPlaylist Playlist = RLPlaylist.Undefined;

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
            Playlist = (RLPlaylist)GetInt("playlist");
            RemainingSeconds = GetInt("time");
        }

        public int TotalGoals { 
            get
            {
                if (Blue.Goals == -1 || Orange.Goals == -1)
                    return -1;
                return Blue.Goals + Orange.Goals;
            }
        }
    }
}
