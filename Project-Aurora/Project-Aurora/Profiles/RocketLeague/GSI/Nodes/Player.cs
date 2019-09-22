namespace Aurora.Profiles.RocketLeague.GSI.Nodes
{
    /// <summary>
    /// Class representing player information
    /// </summary>
    public class Player_RocketLeague : Node<Player_RocketLeague>
    {
        public int Team = -1;
        public float Boost = -1;
        public int Score = -1;
        public int Goals = -1;
        public int Assists = -1;
        public int Saves = -1;
        public int Shots = -1;

        internal Player_RocketLeague(string json_data) : base(json_data)
        {
            Boost = GetFloat("boost");
            Score = GetInt("score");
            Goals = GetInt("goals");
            Assists = GetInt("assists");
            Saves = GetInt("saves");
            Shots = GetInt("shots");
            Team = GetInt("team");
        }
    }
}
