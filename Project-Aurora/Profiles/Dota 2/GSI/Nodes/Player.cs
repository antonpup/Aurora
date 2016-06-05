namespace Aurora.Profiles.Dota_2.GSI.Nodes
{
    public enum PlayerActivity
    {
        Undefined,
        Menu,
        Playing
    }

    public class Player_Dota2 : Node
    {
        public readonly string SteamID;
        public readonly string Name;
        public readonly PlayerActivity Activity;
        public readonly int Kills;
        public readonly int Deaths;
        public readonly int Assists;
        public readonly int LastHits;
        public readonly int Denies;
        public readonly int KillStreak;
        public readonly PlayerTeam Team;
        public readonly int Gold;
        public readonly int GoldReliable;
        public readonly int GoldUnreliable;
        public readonly int GoldPerMinute;
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
