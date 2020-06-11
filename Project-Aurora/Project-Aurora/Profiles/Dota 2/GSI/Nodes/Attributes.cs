namespace Aurora.Profiles.Dota_2.GSI.Nodes
{
    /// <summary>
    /// Class representing ability attributes
    /// </summary>
    public class Attributes : Node
    {
        /// <summary>
        /// Amount of levels to spend
        /// </summary>
        public int Level;

        internal Attributes(string json_data) : base(json_data)
        {
            Level = GetInt("level");
        }
    }
}
