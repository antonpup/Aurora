using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.LeagueOfLegends.GSI.Nodes
{
    public class InventoryNode : Node<InventoryNode>
    {
        public ItemNode Slot1 = new ItemNode();
        public ItemNode Slot2 = new ItemNode();
        public ItemNode Slot3 = new ItemNode();
        public ItemNode Slot4 = new ItemNode();
        public ItemNode Slot5 = new ItemNode();
        public ItemNode Slot6 = new ItemNode();
        public ItemNode Trinket = new ItemNode();
    }
}
