using System;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    /// <summary>
    /// Class representing various player states
    /// </summary>
    public class PlayerStateNode : Node
    {
        /// <summary>
        /// Player's health
        /// </summary>
        public int Health;

        /// <summary>
        /// Player's armor
        /// </summary>
        public int Armor;

        /// <summary>
        /// Boolean representing whether or not the player has a helmet
        /// </summary>
        public bool Helmet;

        /// <summary>
        /// Player's flash amount
        /// </summary>
        public int Flashed;

        /// <summary>
        /// Player's smoked amount
        /// </summary>
        public int Smoked;

        /// <summary>
        /// Player's burning amount
        /// </summary>
        public int Burning;

        /// <summary>
        /// Player's current money
        /// </summary>
        public int Money;

        /// <summary>
        /// Player's current round kills
        /// </summary>
        public int RoundKills;

        /// <summary>
        /// Player's current round kills (headshots only)
        /// </summary>
        public int RoundKillHS;

        /// <summary>
        /// Value of equipment
        /// </summary>
        public int EquipValue;

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
            EquipValue = GetInt("equip_value");
        }
    }
}
