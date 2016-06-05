namespace Aurora.Profiles.Dota_2.GSI.Nodes
{
    public class Hero_Dota2 : Node
    {
        public readonly int ID;
        public readonly string Name;
        public readonly int Level;
        public readonly bool IsAlive;
        public readonly int SecondsToRespawn;
        public readonly int BuybackCost;
        public readonly int BuybackCooldown;
        public readonly int Health;
        public readonly int MaxHealth;
        public readonly int HealthPercent;
        public readonly int Mana;
        public readonly int MaxMana;
        public readonly int ManaPercent;
        public readonly bool IsSilenced;
        public readonly bool IsStunned;
        public readonly bool IsDisarmed;
        public readonly bool IsMagicImmune;
        public readonly bool IsHexed;
        public readonly bool IsMuted;
        public readonly bool IsBreak;
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
