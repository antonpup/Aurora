namespace Aurora.Profiles.Dota_2.GSI.Nodes
{
    /// <summary>
    /// Class representing ability information
    /// </summary>
    public class Ability : Node
    {
        /// <summary>
        /// Ability name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Ability level
        /// </summary>
        public readonly int Level;

        /// <summary>
        /// A boolean representing whether the ability can be casted
        /// </summary>
        public readonly bool CanCast;

        /// <summary>
        /// A boolean representing whether the ability is passives
        /// </summary>
        public readonly bool IsPassive;

        /// <summary>
        /// A boolean representing whether the ability is active
        /// </summary>
        public readonly bool IsActive;

        /// <summary>
        /// Ability cooldown
        /// </summary>
        public readonly int Cooldown;

        /// <summary>
        /// A boolean representing whether the ability is an ultimate
        /// </summary>
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
