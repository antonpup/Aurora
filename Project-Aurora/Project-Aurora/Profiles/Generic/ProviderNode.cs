using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Generic.GSI.Nodes {
    
    public class ProviderNode : AutoJsonNode<ProviderNode> {

        public string Name;
        public int AppID;

        internal ProviderNode(string json) : base(json) { }
    }
}
