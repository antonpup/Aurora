using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.StardewValley.GSI.Nodes
{

    public class PlayerNode : AutoJsonNode<PlayerNode>
    {
        public HealthNode Health => NodeFor<HealthNode>("Health");
        public string CurrentLocation;
        public bool IsOutdoor;
        public EnergyNode Energy => NodeFor<EnergyNode>("Energy");

        internal PlayerNode(string json) : base(json){ }
        
        public class HealthNode : AutoJsonNode<HealthNode>
        {
            public int Current;
            public int Max;
            public bool BarActive;

            internal HealthNode(string JSON) : base(JSON) { }
        }

        public class EnergyNode : AutoJsonNode<EnergyNode>
        {
            public int Current;
            public int Max;

            internal EnergyNode(string JSON) : base(JSON) { }
        }
    }
}
