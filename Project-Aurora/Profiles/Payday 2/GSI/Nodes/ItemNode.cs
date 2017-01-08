namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    public class ItemNode : Node<ItemNode>
    {
        public ItemType Type;
        public string ID;
        public int Count;

        internal ItemNode(string JSON) : base(JSON)
        {
            Type = GetEnum<ItemType>("type");
            ID = GetString("id");
            Count = GetInt("count");
        }
    }

    public enum ItemType
    {
        Undefined,
        Equipment,
        Pickup
    }
}
