using System.Collections.Generic;
using System.Linq;

namespace Aurora.Profiles.Dota_2.GSI.Nodes
{
    public class Items_Dota2 : Node
    {
        private List<Item> inventory = new List<Item>();
        private List<Item> stash = new List<Item>();

        public int CountInventory { get { return inventory.Count; } }
        public int CountStash { get { return stash.Count; } }

        internal Items_Dota2(string json_data) : base(json_data)
        {
            List<string> slots = _ParsedData.Properties().Select(p => p.Name).ToList();
            foreach (string item_slot in slots)
            {
                if (item_slot.StartsWith("slot"))
                    this.inventory.Add(new Item(_ParsedData[item_slot].ToString()));
                else
                    this.stash.Add(new Item(_ParsedData[item_slot].ToString()));
            }
        }

        /// <summary>
        /// Gets the inventory item in the selected index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Item GetInventoryAt(int index)
        {
            if (index > inventory.Count - 1)
                return new Item("");

            return inventory[index];
        }

        /// <summary>
        /// Gets the stash item in the selected index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Item GetStashAt(int index)
        {
            if (index > stash.Count - 1)
                return new Item("");

            return stash[index];
        }

        /// <summary>
        /// Checks if item exists in the inventory
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool InventoryContains(string itemname)
        {
            foreach(Item inventory_item in this.inventory)
            {
                if (inventory_item.Name == itemname)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if item exists in the stash
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool StashContains(string itemname)
        {
            foreach (Item stash_item in this.stash)
            {
                if (stash_item.Name == itemname)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets index of the item in the inventory
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int InventoryIndexOf(string itemname)
        {
            int index = -1;
            for (int x = 0; x < this.inventory.Count; x++)
            {
                if (this.inventory[x].Name == itemname)
                {
                    return x;
                }
            }

            return index;
        }

        /// <summary>
        /// Gets index of the item in the stash
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int StashIndexOf(string itemname)
        {
            int index = -1;
            for (int x = 0; x < this.stash.Count; x++)
            {
                if (this.stash[x].Name == itemname)
                {
                    return x;
                }
            }

            return index;
        }
    }
}
