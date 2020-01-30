using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.LeagueOfLegends.GSI.Nodes
{
    public class ItemNode : Node<ItemNode>
    {
        public bool CanUse;
        public bool Consumable;
        public int Count;
        public string Name = "";
        public int ItemID;

        public ItemNode()
        {

        }

        public ItemNode(Item item)
        {
            CanUse = item.canUse;
            Consumable = item.consumable;
            Count = item.count;
            Name = item.displayName;
            ItemID = item.itemID;
        }
    }
}
