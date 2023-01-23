namespace Aurora.Profiles.Borderlands2.GSI.Nodes
{
    /// <summary>
    /// Class representing player information
    /// </summary>
    public class Player_Borderlands2 : Node
    {
        /// <summary>
        /// Player's boost amount [0.0f, 1.0f]
        /// </summary>
        public float MaximumHealth = 0.0f;
        public float CurrentHealth = 0.0f;
        public float MaximumShield = 0.0f;
        public float CurrentShield = 0.0f;

        internal Player_Borderlands2(string json_data) : base(json_data)
        {

        }
    }
}
