namespace Aurora.Profiles.Dota_2.GSI.Nodes
{
    /// <summary>
    /// Class representing item information
    /// </summary>
    public class Item : Node<Item>
    {
        /// <summary>
        /// Item name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The name of the rune cotnained inside this item.
        /// <note type="note">Possible rune names: empty, arcane, bounty, double_damage, haste, illusion, invisibility, regen</note>
        /// </summary>
        public readonly string ContainsRune;

        /// <summary>
        /// A boolean representing whether this item can be casted
        /// </summary>
        public readonly bool CanCast;

        /// <summary>
        /// Item's cooldown
        /// </summary>
        public readonly int Cooldown;

        /// <summary>
        /// A boolean representing whether this item is passive
        /// </summary>
        public readonly bool IsPassive;

        /// <summary>
        /// The amount of charges on this item
        /// </summary>
        public readonly int Charges;

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
