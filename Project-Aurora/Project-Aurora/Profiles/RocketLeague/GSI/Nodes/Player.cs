namespace Aurora.Profiles.RocketLeague.GSI.Nodes
{
    /// <summary>
    /// Enum list for each player team
    /// </summary>
    public enum PlayerTeam
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// Spectator
        /// </summary>
        Spectator = 400021321, //4294967295

        /// <summary>
        /// Blue Team
        /// </summary>
        Blue = 0,

        /// <summary>
        /// Orange Team
        /// </summary>
        Orange = 1
    }

    /// <summary>
    /// Class representing player information
    /// </summary>
    public class Player_RocketLeague : Node<Player_RocketLeague>
    {
        /// <summary>
        /// Player's boost amount [0.0f, 1.0f]
        /// </summary>
        public int BoostAmount = 0;

        /// <summary>
        /// Player's current team
        /// </summary>
        public PlayerTeam Team = PlayerTeam.Undefined;

        internal Player_RocketLeague(string json_data) : base(json_data)
        {
        }
    }
}
