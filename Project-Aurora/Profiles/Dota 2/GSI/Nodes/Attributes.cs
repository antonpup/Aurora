namespace Aurora.Profiles.Dota_2.GSI.Nodes
{
    public class Attributes : Node
    {
        public readonly int Level;

        internal Attributes(string json_data) : base(json_data)
        {
            Level = GetInt("level");
        }
    }
}
