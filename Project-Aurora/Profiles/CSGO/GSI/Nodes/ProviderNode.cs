namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    public class ProviderNode : Node
    {
        public readonly string Name;
        public readonly int AppID;
        public readonly int Version;
        public readonly string SteamID;
        public readonly string TimeStamp;

        internal ProviderNode(string JSON)
            : base(JSON)
        {
            Name = GetString("name");
            AppID = GetInt("appid");
            Version = GetInt("version");
            SteamID = GetString("steamid");
            TimeStamp = GetString("timestamp");
        }
    }
}
