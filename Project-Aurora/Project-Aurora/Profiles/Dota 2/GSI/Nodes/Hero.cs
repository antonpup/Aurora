using Aurora.Nodes;

namespace Aurora.Profiles.Dota_2.GSI.Nodes
{
    /// <summary>
    /// Class representing hero information
    /// </summary>
    public class Hero_Dota2 : Node
    {
        /// <summary>
        /// Hero ID
        /// </summary>
        public int ID;

        /// <summary>
        /// Hero name
        /// </summary>
        public string Name;

        /// <summary>
        /// Hero level
        /// </summary>
        public int Level;

        /// <summary>
        /// A boolean representing whether the hero is alive
        /// </summary>
        public bool IsAlive;

        /// <summary>
        /// Amount of seconds until the hero respawns
        /// </summary>
        public int SecondsToRespawn;

        /// <summary>
        /// The buyback cost
        /// </summary>
        public int BuybackCost;

        /// <summary>
        /// The buyback cooldown
        /// </summary>
        public int BuybackCooldown;

        /// <summary>
        /// Hero health
        /// </summary>
        public int Health;

        /// <summary>
        /// Hero max health
        /// </summary>
        public int MaxHealth;

        /// <summary>
        /// Hero health percentage
        /// </summary>
        public int HealthPercent;

        /// <summary>
        /// Hero mana
        /// </summary>
        public int Mana;

        /// <summary>
        /// Hero max mana
        /// </summary>
        public int MaxMana;

        /// <summary>
        /// Hero mana percent
        /// </summary>
        public int ManaPercent;

        /// <summary>
        /// A boolean representing whether the hero is silenced
        /// </summary>
        public bool IsSilenced;

        /// <summary>
        /// A boolean representing whether the hero is stunned
        /// </summary>
        public bool IsStunned;

        /// <summary>
        /// A boolean representing whether the hero is disarmed
        /// </summary>
        public bool IsDisarmed;

        /// <summary>
        /// A boolean representing whether the hero is magic immune
        /// </summary>
        public bool IsMagicImmune;

        /// <summary>
        /// A boolean representing whether the hero is hexed
        /// </summary>
        public bool IsHexed;

        /// <summary>
        /// A boolean representing whether the hero is muteds
        /// </summary>
        public bool IsMuted;

        /// <summary>
        /// A boolean representing whether the hero is broken
        /// </summary>
        public bool IsBreak;

        /// <summary>
        /// A boolean representing whether the hero has Aghanim's Scepter
        /// </summary>
        public bool HasScepter;

        /// <summary>
        /// A boolean representing whether the hero has Aghanim's Shard
        /// </summary>
        public bool HasShard;

        /// <summary>
        /// A boolean representing whether the hero is under smoke effect
        /// </summary>
        public bool IsSmoked;

        /// <summary>
        /// A boolean representing whether the hero is debuffed
        /// </summary>
        public bool HasDebuff;

        public bool HasTalent1;
        public bool HasTalent2;
        public bool HasTalent3;
        public bool HasTalent4;
        public bool HasTalent5;
        public bool HasTalent6;
        public bool HasTalent7;
        public bool HasTalent8;

        internal Hero_Dota2(string json_data) : base(json_data)
        {
            ID = GetInt("id");
            Name = GetString("name");
            Level = GetInt("level");
            IsAlive = GetBool("alive");
            SecondsToRespawn = GetInt("respawn_seconds");
            BuybackCost = GetInt("buyback_cost");
            BuybackCooldown = GetInt("buyback_cooldown");
            Health = GetInt("health");
            MaxHealth = GetInt("max_health");
            HealthPercent = GetInt("health_percent");
            Mana = GetInt("mana");
            MaxMana = GetInt("max_mana");
            ManaPercent = GetInt("mana_percent");
            IsSilenced = GetBool("silenced");
            IsStunned = GetBool("stunned");
            IsDisarmed = GetBool("disarmed");
            IsMagicImmune = GetBool("magicimmune");
            IsHexed = GetBool("hexed");
            IsMuted = GetBool("muted");
            IsBreak = GetBool("break");
            HasScepter = GetBool("aghanims_scepter");
            HasShard = GetBool("aghanims_shard");
            IsSmoked = GetBool("smoked");
            HasDebuff = GetBool("has_debuff");
            HasTalent1 = GetBool("talen1_1");
            HasTalent2 = GetBool("talen1_2");
            HasTalent3 = GetBool("talen1_3");
            HasTalent4 = GetBool("talen1_4");
            HasTalent5 = GetBool("talen1_5");
            HasTalent6 = GetBool("talen1_6");
            HasTalent7 = GetBool("talen1_7");
            HasTalent8 = GetBool("talen1_8");
        }
    }
}
