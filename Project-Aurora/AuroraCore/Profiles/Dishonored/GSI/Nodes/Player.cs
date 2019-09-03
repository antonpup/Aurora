namespace Aurora.Profiles.Dishonored.GSI.Nodes
{
    /// <summary>
    /// Class representing player information
    /// </summary>
    public class Player_Dishonored : Node<Player_Dishonored>
    {
        /// <summary>
        /// Player's boost amount [0.0f, 1.0f]
        /// </summary>
        public int ManaPots = 0;
        public int HealthPots = 0;
        public int CurrentHealth = 0;
        public int MaximumHealth = 0;
        public int CurrentMana = 0;
        public int MaximumMana = 0;

        internal Player_Dishonored(string json_data) : base(json_data)
        {

        }
    }
}
