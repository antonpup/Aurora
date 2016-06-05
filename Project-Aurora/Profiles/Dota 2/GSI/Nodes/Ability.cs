namespace Aurora.Profiles.Dota_2.GSI.Nodes
{
    public class Ability : Node
    {
        public readonly string Name;
        public readonly int Level;
        public readonly bool CanCast;
        public readonly bool IsPassive;
        public readonly bool IsActive;
        public readonly int Cooldown;
        public readonly bool IsUltimate;

        internal Ability(string json_data) : base(json_data)
        {
            Name = GetString("name");
            Level = GetInt("level");
            CanCast = GetBool("can_cast");
            IsPassive = GetBool("passive");
            IsActive = GetBool("ability_active");
            Cooldown = GetInt("cooldown");
            IsUltimate = GetBool("ultimate");
        }
    }
}
