namespace Aurora.Profiles.Minecraft.GSI.Nodes
{
    public class ProviderNode : Node<ProviderNode>
    {

        public string Name;
        public int AppID;

        internal ProviderNode(string json) : base(json)
        {
            Name = GetString("name");
            AppID = GetInt("appid");
        }
    }
}
