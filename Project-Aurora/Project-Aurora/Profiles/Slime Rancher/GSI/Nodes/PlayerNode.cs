using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Slime_Rancher.GSI.Nodes {
    public class PlayerNode : AutoJsonNode<PlayerNode>
    {
        public CurrentMaxNode Health => NodeFor<CurrentMaxNode>("health");
        public CurrentMaxNode Energy => NodeFor<CurrentMaxNode>("energy");
        public CurrentMaxNode Radiation => NodeFor<CurrentMaxNode>("rad");

        internal PlayerNode(string json) : base(json) { }
    }

    public class CurrentMaxNode : AutoJsonNode<CurrentMaxNode>
    {
        public int Current;
        public int Max;

        internal CurrentMaxNode(string json) : base(json) { }
    }
}
