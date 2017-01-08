using System.ComponentModel;

namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    /// <summary>
    /// Information about a player
    /// </summary>
    public class PlayerNode : Node<PlayerNode>
    {
        /// <summary>
        /// Player name
        /// </summary>
        public string Name;

        /// <summary>
        /// Player character
        /// </summary>
        public string Character;

        /// <summary>
        /// Player level
        /// </summary>
        public int Level;

        /// <summary>
        /// Player infamy rank
        /// </summary>
        public int Rank;

        /// <summary>
        /// Player state
        /// </summary>
        public PlayerState State;

        /// <summary>
        /// Player health information
        /// </summary>
        public HealthNode Health;

        /// <summary>
        /// Player armor information
        /// </summary>
        public ArmorNode Armor;

        /// <summary>
        /// Player weapons information
        /// </summary>
        public WeaponsNode Weapons;

        /// <summary>
        /// The time left on downed timer
        /// </summary>
        public int DownTime;

        /// <summary>
        /// The suspicion amount [0.0f - 1.0f]
        /// </summary>
        public float SuspicionAmount;

        /// <summary>
        /// The flashed amount [0.0f - 1.0f]
        /// </summary>
        public float FlashAmount;

        /// <summary>
        /// A boolean representing if this is the local player
        /// </summary>
        public bool IsLocalPlayer;

        /// <summary>
        /// A boolean representing if this player is in swan song
        /// </summary>
        public bool IsSwanSong;

        internal PlayerNode(string JSON) : base(JSON)
        {
            Name = GetString("name");
            Character = GetString("character");
            Level = GetInt("level");
            Rank = GetInt("rank");
            State = GetEnum<PlayerState>("state");
            Health = new HealthNode(_ParsedData["health"]?.ToString() ?? "");
            Armor = new ArmorNode(_ParsedData["armor"]?.ToString() ?? "");
            Weapons = new WeaponsNode(_ParsedData["weapons"]?.ToString() ?? "");
            DownTime = GetInt("down_time");
            SuspicionAmount = GetFloat("suspicion");
            FlashAmount = GetFloat("flashbang_amount");
            IsLocalPlayer = GetBool("is_local");
            IsSwanSong = GetBool("is_swansong");
        }
    }

    /// <summary>
    /// Information about player's health
    /// </summary>
    public class HealthNode : Node<HealthNode>
    {
        /// <summary>
        /// Current health amount
        /// </summary>
        public float Current;

        /// <summary>
        /// Maximum health amount
        /// </summary>
        public float Max;

        /// <summary>
        /// Number of revives left
        /// </summary>
        public int Revives;

        internal HealthNode(string JSON) : base(JSON)
        {
            Current = GetFloat("current");
            Max = GetFloat("total");
            Revives = GetInt("revives");
        }
    }

    /// <summary>
    /// Information about player's armor
    /// </summary>
    public class ArmorNode : Node<ArmorNode>
    {
        /// <summary>
        /// Maximum amount of armor
        /// </summary>
        public float Max;

        /// <summary>
        /// Current amount of armor
        /// </summary>
        public float Current;

        /// <summary>
        /// Total amount of armor
        /// </summary>
        public float Total;

        internal ArmorNode(string JSON) : base(JSON)
        {
            Max = GetFloat("max");
            Current = GetFloat("current");
            Total = GetFloat("total");
        }
    }

    /// <summary>
    /// Enum for each player state
    /// </summary>
    public enum PlayerState
    {
        /// <summary>
        /// Undefined
        /// </summary>
        [Description("Undefined")]
        Undefined,

        /// <summary>
        /// Standard state (mask is on)
        /// </summary>
        [Description("Standard")]
        Standard,

        /// <summary>
        /// Mask is off state
        /// </summary>
        [Description("Mask Off")]
        Mask_Off,

        /// <summary>
        /// Player is carrying a bag
        /// </summary>
        [Description("Carrying a bag")]
        Carry,

        /// <summary>
        /// Player is using a bipod
        /// </summary>
        [Description("Using a bipod")]
        Bipod,

        /// <summary>
        /// Player is parachiting, type 1
        /// </summary>
        [Description("Parachuting 1")]
        Jerry1,

        /// <summary>
        /// Player is parachiting, type 2
        /// </summary>
        [Description("Parachuting 2")]
        Jerry2,

        /// <summary>
        /// Player is being tased
        /// </summary>
        [Description("Tased")]
        Tased,

        /// <summary>
        /// Player is clean
        /// </summary>
        [Description("Clean")]
        Clean,

        /// <summary>
        /// Player is a civilian
        /// </summary>
        [Description("Civilian")]
        Civilian,

        /// <summary>
        /// Player is bleeding out
        /// </summary>
        [Description("Bleeding out")]
        Bleed_out,

        /// <summary>
        /// PLayer is completely downed
        /// </summary>
        [Description("Fatal injury")]
        Fatal,

        /// <summary>
        /// Player is incapacitated
        /// </summary>
        [Description("Incapacitated")]
        Incapacitated,

        /// <summary>
        /// Player is in custody
        /// </summary>
        [Description("Arrested")]
        Arrested
    }

}
