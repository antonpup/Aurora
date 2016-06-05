using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    public class ItemsNode : Node
    {
        private List<ItemNode> _Items = new List<ItemNode>();

        public int Count { get { return _Items.Count; } }

        internal ItemsNode(string JSON) : base(JSON)
        {
            foreach (JToken jt in _ParsedData.Children())
            {
                _Items.Add(new ItemNode(jt.First.ToString()));
            }
        }

        /// <summary>
        /// Gets the weapon with index &lt;index&gt;
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ItemNode this[int index]
        {
            get
            {
                if (index > _Items.Count - 1)
                {
                    return new ItemNode("");
                }

                return _Items[index];
            }
        }
    }
}
