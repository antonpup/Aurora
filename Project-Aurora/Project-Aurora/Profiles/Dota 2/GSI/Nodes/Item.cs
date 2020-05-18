namespace Aurora.Profiles.Dota_2.GSI.Nodes
{
    /// <summary>
    /// Class representing item information
    /// </summary>
    public class Item : Node
    {
        /// <summary>
        /// Item name
        /// </summary>
        public string Name;

        /// <summary>
        /// The name of the rune cotnained inside this item.
        /// <note type="note">Possible rune names: empty, arcane, bounty, double_damage, haste, illusion, invisibility, regen</note>
        /// </summary>
        public string ContainsRune;

        /// <summary>
        /// A boolean representing whether this item can be casted
        /// </summary>
        public bool CanCast;

        /// <summary>
        /// Item's cooldown
        /// </summary>
        public int Cooldown;

        /// <summary>
        /// A boolean representing whether this item is passive
        /// </summary>
        public bool IsPassive;

        /// <summary>
        /// The amount of charges on this item
        /// </summary>
        public int Charges;

        public Item() : this("")
        {
        }

        internal Item(string json_data) : base(json_data)
        {
            Name = GetString("name");
            ContainsRune = GetString("contains_rune");
            CanCast = GetBool("can_cast");
            Cooldown = GetInt("cooldown");
            IsPassive = GetBool("passive");
            Charges = GetInt("charges");
        }
    }
}
