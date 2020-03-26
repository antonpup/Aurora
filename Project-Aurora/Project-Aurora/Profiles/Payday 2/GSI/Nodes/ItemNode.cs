namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    public class ItemNode : AutoJsonNode<ItemNode>
    {
        public ItemType Type;
        public string ID;
        public int Count;

        internal ItemNode(string JSON) : base(JSON) { }
    }

    public enum ItemType
    {
        Undefined,
        Equipment,
        Pickup
    }
}
