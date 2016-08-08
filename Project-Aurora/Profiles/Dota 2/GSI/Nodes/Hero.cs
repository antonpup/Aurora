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
        public readonly int ID;

        /// <summary>
        /// Hero name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Hero level
        /// </summary>
        public readonly int Level;

        /// <summary>
        /// A boolean representing whether the hero is alive
        /// </summary>
        public readonly bool IsAlive;

        /// <summary>
        /// Amount of seconds until the hero respawns
        /// </summary>
        public readonly int SecondsToRespawn;

        /// <summary>
        /// The buyback cost
        /// </summary>
        public readonly int BuybackCost;

        /// <summary>
        /// The buyback cooldown
        /// </summary>
        public readonly int BuybackCooldown;

        /// <summary>
        /// Hero health
        /// </summary>
        public readonly int Health;

        /// <summary>
        /// Hero max health
        /// </summary>
        public readonly int MaxHealth;

        /// <summary>
        /// Hero health percentage
        /// </summary>
        public readonly int HealthPercent;

        /// <summary>
        /// Hero mana
        /// </summary>
        public readonly int Mana;

        /// <summary>
        /// Hero max mana
        /// </summary>
        public readonly int MaxMana;

        /// <summary>
        /// Hero mana percent
        /// </summary>
        public readonly int ManaPercent;

        /// <summary>
        /// A boolean representing whether the hero is silenced
        /// </summary>
        public readonly bool IsSilenced;

        /// <summary>
        /// A boolean representing whether the hero is stunned
        /// </summary>
        public readonly bool IsStunned;

        /// <summary>
        /// A boolean representing whether the hero is disarmed
        /// </summary>
        public readonly bool IsDisarmed;

        /// <summary>
        /// A boolean representing whether the hero is magic immune
        /// </summary>
        public readonly bool IsMagicImmune;

        /// <summary>
        /// A boolean representing whether the hero is hexed
        /// </summary>
        public readonly bool IsHexed;

        /// <summary>
        /// A boolean representing whether the hero is muteds
        /// </summary>
        public readonly bool IsMuted;

        /// <summary>
        /// A boolean representing whether the hero is broken
        /// </summary>
        public readonly bool IsBreak;

        /// <summary>
        /// A boolean representing whether the hero is debuffed
        /// </summary>
        public readonly bool HasDebuff;

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
            HasDebuff = GetBool("has_debuff");
        }
    }
}
