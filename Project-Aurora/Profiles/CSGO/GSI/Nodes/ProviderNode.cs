namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    /// <summary>
    /// Information about the provider of this GameState
    /// </summary>
    public class ProviderNode : Node
    {
        /// <summary>
        /// Game name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Game's Steam AppID
        /// </summary>
        public readonly int AppID;

        /// <summary>
        /// Game's version
        /// </summary>
        public readonly int Version;

        /// <summary>
        /// Local player's Steam ID
        /// </summary>
        public readonly string SteamID;

        /// <summary>
        /// Current timestamp
        /// </summary>
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
