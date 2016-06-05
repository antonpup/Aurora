using System;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    public class PlayerStateNode : Node
    {
        public readonly int Health;
        public readonly int Armor;
        public readonly bool Helmet;
        public readonly int Flashed;
        public readonly int Smoked;
        public readonly int Burning;
        public readonly int Money;
        public readonly int RoundKills;
        public readonly int RoundKillHS;

        internal PlayerStateNode(string JSON)
            : base(JSON)
        {
            Health = GetInt("health");
            Armor = GetInt("armor");
            Helmet = GetBool("helmet");
            Flashed = GetInt("flashed");
            Smoked = GetInt("smoked");
            Burning = GetInt("burning");
            Money = GetInt("money");
            RoundKills = GetInt("round_kills");
            RoundKillHS = GetInt("round_killhs");
        }
    }
}
