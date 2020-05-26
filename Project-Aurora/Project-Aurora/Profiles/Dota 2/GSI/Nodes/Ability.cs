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
        public string Name;

        /// <summary>
        /// Ability level
        /// </summary>
        public int Level;

        /// <summary>
        /// A boolean representing whether the ability can be casted
        /// </summary>
        public bool CanCast;

        /// <summary>
        /// A boolean representing whether the ability is passive
        /// </summary>
        public bool IsPassive;

        /// <summary>
        /// A boolean representing whether the ability is active
        /// </summary>
        public bool IsActive;

        /// <summary>
        /// Ability cooldown
        /// </summary>
        public int Cooldown;

        /// <summary>
        /// A boolean representing whether the ability is an ultimate
        /// </summary>
        public bool IsUltimate;

        public Ability() : this("")
        {
        }

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
