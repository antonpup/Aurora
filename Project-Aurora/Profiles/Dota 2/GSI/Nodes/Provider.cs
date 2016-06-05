namespace Aurora.Profiles.Dota_2.GSI.Nodes
{
    public class Provider_Dota2 : Node
    {
        public readonly string Name;
        public readonly int AppID;
        public readonly int Version;
        public readonly string TimeStamp;

        internal Provider_Dota2(string json_data) : base(json_data)
        {
            Name = GetString("name");
            AppID = GetInt("appid");
            Version = GetInt("version");
            TimeStamp = GetString("timestamp");
        }
    }
}
