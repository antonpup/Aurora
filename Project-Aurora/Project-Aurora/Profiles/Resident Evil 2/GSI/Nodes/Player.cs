namespace Aurora.Profiles.ResidentEvil2.GSI.Nodes
{
    /// <summary>
    /// Class representing player information
    /// </summary>
    public class Player_ResidentEvil2 : Node
    {
        public enum PlayerStatus
        {
            /// <summary>
            /// Not in-game
            /// </summary>
            OffGame = -1, 
            
            /// <summary>
            /// Dead
            /// </summary>
            Dead = 0,

            /// <summary>
            /// Danger
            /// </summary>
            Danger = 1,

            /// <summary>
            /// Caution
            /// </summary>
            Caution = 2,

            /// <summary>
            /// Lite Fine
            /// </summary>
            LiteFine = 3,

            /// <summary>
            /// Fine
            /// </summary>
            Fine = 4
        }

        /// <summary>
        /// Player's maximum health, currently 1200 in all modes.
        /// </summary>
        public int MaximumHealth = 0; // max health 0 means not in-game

        /// <summary>
        /// Player's health amount [0, 1200].
        /// </summary>
        public int CurrentHealth = 0;

        /// <summary>
        /// Player's status (e.g., Fine, Caution, Danger, etc.). OffGame means player is not in-game.
        /// </summary>
        public PlayerStatus Status = PlayerStatus.OffGame;

        /// <summary>
        /// Player's poison status. Poison status is separate from health status.
        /// </summary>
        public bool Poison = false;

        /// <summary>
        /// The dynamic difficulty of the game. Changes as player kills enemies or gets hurt.
        /// </summary>
        public int Rank = 0;

        internal Player_ResidentEvil2(string json_data) : base(json_data)
        {

        }
    }
}
