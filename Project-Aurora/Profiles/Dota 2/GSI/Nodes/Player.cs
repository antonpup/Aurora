namespace Aurora.Profiles.Dota_2.GSI.Nodes
{
    /// <summary>
    /// Enum for various player activities
    /// </summary>
    public enum PlayerActivity
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined,

        /// <summary>
        /// In a menu
        /// </summary>
        Menu,

        /// <summary>
        /// In a game
        /// </summary>
        Playing
    }

    /// <summary>
    /// Class representing player information
    /// </summary>
    public class Player_Dota2 : Node
    {
        /// <summary>
        /// Player's steam ID
        /// </summary>
        public readonly string SteamID;

        /// <summary>
        /// Player's name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Player's current activity state
        /// </summary>
        public readonly PlayerActivity Activity;

        /// <summary>
        /// Player's amount of kills
        /// </summary>
        public readonly int Kills;

        /// <summary>
        /// Player's amount of deaths
        /// </summary>
        public readonly int Deaths;

        /// <summary>
        /// Player's amount of assists
        /// </summary>
        public readonly int Assists;

        /// <summary>
        /// Player's amount of last hits
        /// </summary>
        public readonly int LastHits;

        /// <summary>
        /// Player's amount of denies
        /// </summary>
        public readonly int Denies;

        /// <summary>
        /// Player's killstreak
        /// </summary>
        public readonly int KillStreak;

        /// <summary>
        /// Player's team
        /// </summary>
        public readonly PlayerTeam Team;

        /// <summary>
        /// Player's amount of gold
        /// </summary>
        public readonly int Gold;

        /// <summary>
        /// Player's amount of reliable gold
        /// </summary>
        public readonly int GoldReliable;

        /// <summary>
        /// Player's amount of unreliable gold
        /// </summary>
        public readonly int GoldUnreliable;

        /// <summary>
        /// PLayer's gold per minute
        /// </summary>
        public readonly int GoldPerMinute;

        /// <summary>
        /// Player's experience per minute
        /// </summary>
        public readonly int ExperiencePerMinute;

        internal Player_Dota2(string json_data) : base(json_data)
        {
            SteamID = GetString("steamid");
            Name = GetString("name");
            Activity = GetEnum<PlayerActivity>("activity");
            Kills = GetInt("kills");
            Deaths = GetInt("deaths");
            Assists = GetInt("assists");
            LastHits = GetInt("last_hits");
            Denies = GetInt("denies");
            KillStreak = GetInt("kill_streak");
            Team = GetEnum<PlayerTeam>("team_name");
            Gold = GetInt("gold");
            GoldReliable = GetInt("gold_reliable");
            GoldUnreliable = GetInt("gold_unreliable");
            GoldPerMinute = GetInt("gpm");
            ExperiencePerMinute = GetInt("xpm");
        }
    }
}
