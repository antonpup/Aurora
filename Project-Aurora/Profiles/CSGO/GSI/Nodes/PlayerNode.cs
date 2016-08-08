using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    /// <summary>
    /// Class representing player information
    /// </summary>
    public class PlayerNode : Node
    {
        internal string _SteamID;

        /// <summary>
        /// Player's steam ID
        /// </summary>
        public string SteamID { get { return _SteamID; } }

        /// <summary>
        /// Player's name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Player's team
        /// </summary>
        public readonly PlayerTeam Team;

        /// <summary>
        /// Player's clan tag
        /// </summary>
        public readonly string Clan;

        /// <summary>
        /// Player's current activity state
        /// </summary>
        public readonly PlayerActivity Activity;

        /// <summary>
        /// Player's current weapons
        /// </summary>
        public readonly WeaponsNode Weapons;

        /// <summary>
        /// Player's match statistics
        /// </summary>
        public readonly MatchStatsNode MatchStats;

        /// <summary>
        /// Player's state information
        /// </summary>
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
        Playing,

        /// <summary>
        /// In a console/chat
        /// </summary>
        TextInput
    }

    /// <summary>
    /// Enum for each team
    /// </summary>
    public enum PlayerTeam
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined,

        /// <summary>
        /// Terrorist team
        /// </summary>
        T,

        /// <summary>
        /// Counter-Terrorist team
        /// </summary>
        CT
    }
}
