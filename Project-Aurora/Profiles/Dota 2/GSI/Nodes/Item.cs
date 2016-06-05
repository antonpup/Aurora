namespace Aurora.Profiles.Dota_2.GSI.Nodes
{
    public class Item : Node
    {
        public readonly string Name;
        public readonly string ContainsRune;
        public readonly bool CanCast;
        public readonly int Cooldown;
        public readonly bool IsPassive;
        public readonly int Charges;


        internal Item(string json_data) : base(json_data)
        {
            Name = GetString("name");
            ContainsRune = GetString("contains_rune"); //double_damage, haste, illusion, invisibility, regeneration, arcane, bounty, empty
            CanCast = GetBool("can_cast");
            Cooldown = GetInt("cooldown");
            IsPassive = GetBool("passive");
            Charges = GetInt("charges");
        }
    }
}
