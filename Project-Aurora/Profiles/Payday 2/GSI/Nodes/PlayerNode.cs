using System.ComponentModel;

namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    public class PlayerNode : Node
    {
        public readonly string Name;
        public readonly string Character;
        public readonly int Level;
        public readonly int Rank;
        public readonly PlayerState State;
        public readonly HealthNode Health;
        public readonly ArmorNode Armor;
        public readonly WeaponsNode Weapons;
        public readonly int DownTime;
        public readonly float SuspicionAmount;
        public readonly float FlashAmount;
        public readonly bool IsLocalPlayer;
        public readonly bool IsSwanSong;

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

    public class HealthNode : Node
    {
        public readonly float Current;
        public readonly float Max;
        public readonly int Revives;

        internal HealthNode(string JSON) : base(JSON)
        {
            Current = GetFloat("current");
            Max = GetFloat("total");
            Revives = GetInt("revives");
        }
    }

    public class ArmorNode : Node
    {
        public readonly float Max;
        public readonly float Current;
        public readonly float Total;

        internal ArmorNode(string JSON) : base(JSON)
        {
            Max = GetFloat("max");
            Current = GetFloat("current");
            Total = GetFloat("total");
        }
    }

    public enum PlayerState
    {
        [Description("Undefined")]
        Undefined,
        [Description("Standard")]
        Standard,
        [Description("Mask Off")]
        Mask_Off,
        [Description("Carrying a bag")]
        Carry,
        [Description("Using a bipod")]
        Bipod,
        [Description("Parachuting 1")]
        Jerry1,
        [Description("Parachuting 2")]
        Jerry2,
        [Description("Tased")]
        Tased,
        [Description("Clean")]
        Clean,
        [Description("Civilian")]
        Civilian,
        [Description("Bleeding out")]
        Bleed_out,
        [Description("Fatal injury")]
        Fatal,
        [Description("Incapacitated")]
        Incapacitated,
        [Description("Arrested")]
        Arrested
    }

}
