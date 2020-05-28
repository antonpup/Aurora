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
        /// Observer Slot
        /// </summary>
        public int ObserverSlot;

        /// <summary>
        /// Player's name
        /// </summary>
        public string Name;

        /// <summary>
        /// Player's team
        /// </summary>
        public PlayerTeam Team;

        /// <summary>
        /// Player's clan tag
        /// </summary>
        public string Clan;

        /// <summary>
        /// Player's current activity state
        /// </summary>
        public PlayerActivity Activity;

        /// <summary>
        /// Player's current weapons
        /// </summary>
        public WeaponsNode Weapons;

        /// <summary>
        /// Player's match statistics
        /// </summary>
        public MatchStatsNode MatchStats;

        /// <summary>
        /// Player's state information
        /// </summary>
        public PlayerStateNode State;

        internal PlayerNode(string JSON)
            : base(JSON)
        {
            _SteamID = GetString("steamid");
            ObserverSlot = GetInt("observer_slot");
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
