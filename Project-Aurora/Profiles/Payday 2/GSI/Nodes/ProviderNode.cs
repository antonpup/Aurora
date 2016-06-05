namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    public class ProviderNode : Node
    {
        public readonly string Name;
        public readonly int AppID;
        public readonly int Version;
        public readonly string SteamID;
        public readonly float TimeStamp;
        public readonly int LocalID;

        internal ProviderNode(string JSON)
            : base(JSON)
        {
            Name = GetString("name");
            AppID = GetInt("appid");
            Version = GetInt("version");
            SteamID = GetString("steamid");
            TimeStamp = GetFloat("timestamp");
            LocalID = GetInt("local_peer_id");
        }
    }
}
