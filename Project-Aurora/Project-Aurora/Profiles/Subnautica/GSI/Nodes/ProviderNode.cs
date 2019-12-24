using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Subnautica.GSI.Nodes {
    public class ProviderNode : Node<ProviderNode> {

        public string Name;
        public int AppID;

        internal ProviderNode(string json) : base(json) {
            Name = GetString("name");
            AppID = GetInt("appid");
        }
    }
}
