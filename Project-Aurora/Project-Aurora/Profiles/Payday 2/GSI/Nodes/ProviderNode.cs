namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    /// <summary>
    /// Information about the provider of this GameState
    /// </summary>
    public class ProviderNode : AutoJsonNode<ProviderNode>
    {
        /// <summary>
        /// Game name
        /// </summary>
        public string Name;

        /// <summary>
        /// Game's Steam AppID
        /// </summary>
        public int AppID;

        /// <summary>
        /// Game's version
        /// </summary>
        public int Version;

        /// <summary>
        /// Local player's Steam ID
        /// </summary>
        public string SteamID;

        /// <summary>
        /// Current timestamp
        /// </summary>
        public float TimeStamp;

        /// <summary>
        /// Index ID of the local player
        /// </summary>
        [AutoJsonPropertyName("local_peer_id")]
        public int LocalID;

        internal ProviderNode(string JSON) : base(JSON) { }
    }
}
