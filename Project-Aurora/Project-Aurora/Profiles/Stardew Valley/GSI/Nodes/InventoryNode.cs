using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.StardewValley.GSI.Nodes {

    public class InventoryNode : AutoJsonNode<InventoryNode>
    {
        public SellectedSlotNode SellectedSlot => NodeFor<SellectedSlotNode>("SellectedSlot");

        internal InventoryNode(string json) : base(json) { }
    }
    public class SellectedSlotNode : AutoJsonNode<SellectedSlotNode>
    {
        public int Number;
        public string ItemName;
        public SellectedSlotColorNode CategoryColor => NodeFor<SellectedSlotColorNode>("SellectedSlotColor");

        internal SellectedSlotNode(string JSON) : base(JSON) { }
    }

    public class SellectedSlotColorNode : AutoJsonNode<SellectedSlotColorNode>
    {
        public float Red;
        public float Green;
        public float Blue;
        public float Alpha;

        internal SellectedSlotColorNode(string JSON) : base(JSON) { }
    }
}