using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    public class PlayerNode : Node
    {
        internal string _SteamID;
        public string SteamID { get { return _SteamID; } }
        public readonly string Name;
        public readonly PlayerTeam Team;
        public readonly string Clan;
        public readonly PlayerActivity Activity;
        public readonly WeaponsNode Weapons;
        public readonly MatchStatsNode MatchStats;
        public readonly PlayerStateNode State;

        internal PlayerNode(string JSON)
            : base(JSON)
        {
            _SteamID = GetString("steamid");
            Name = GetString("name");
            Team = GetEnum<PlayerTeam>("team");
            Clan = GetString("clan");
            State = new PlayerStateNode(_ParsedData?.SelectToken("state")?.ToString() ?? "{}");
            Weapons = new WeaponsNode(_ParsedData?.SelectToken("weapons")?.ToString() ?? "{}");
            MatchStats = new MatchStatsNode(_ParsedData?.SelectToken("match_stats")?.ToString() ?? "{}");
            Activity = GetEnum<PlayerActivity>("activity");
        }
    }

    public enum PlayerActivity
    {
        Undefined,
        Menu,
        Playing,
        /// <summary>
        /// Console is open
        /// </summary>
        TextInput
    }

    public enum PlayerTeam
    {
        Undefined,
        T,
        CT
    }
}
