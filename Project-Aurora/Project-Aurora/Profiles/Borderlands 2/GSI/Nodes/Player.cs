namespace Aurora.Profiles.Borderlands2.GSI.Nodes
{
    /// <summary>
    /// Class representing player information
    /// </summary>
    public class Player_Borderlands2 : Node<Player_Borderlands2>
    {
        /// <summary>
        /// Player's boost amount [0.0f, 1.0f]
        /// </summary>
        public float maximumHealth = 0.0f;
        public float currentHealth = 0.0f;
        public float maximumShield = 0.0f;
        public float currentShield = 0.0f;

        internal Player_Borderlands2(string json_data) : base(json_data)
        {

        }
    }
}
