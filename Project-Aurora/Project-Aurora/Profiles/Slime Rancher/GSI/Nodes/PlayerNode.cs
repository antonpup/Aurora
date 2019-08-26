using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Slime_Rancher.GSI.Nodes {
    public class PlayerNode : Node<PlayerNode> {

        public HealthNode Health;
        public EnergyNode Energy;
        public RadNode Radiation;

        internal PlayerNode(string json) : base(json) {
            
            Health = new HealthNode(_ParsedData["health"]?.ToString() ?? "");
            Energy = new EnergyNode(_ParsedData["energy"]?.ToString() ?? "");
            Radiation = new RadNode(_ParsedData["rad"]?.ToString() ?? "");
        }

    }

    public class HealthNode : Node<HealthNode>
    {
        public int Current;
        public int Max;

        internal HealthNode(string json) : base(json)
        {
            Current = GetInt("current");
            Max = GetInt("max");
        }
    }

    public class EnergyNode : Node<EnergyNode>
    {
        public int Current;
        public int Max;

        internal EnergyNode(string json) : base(json)
        {
            Current = GetInt("current");
            Max = GetInt("max");
        }
    }

    public class RadNode : Node<RadNode>
    {
        public int Current;
        public int Max;

        internal RadNode(string json) : base(json)
        {
            Current = GetInt("current");
            Max = GetInt("max");
        }
    }
}
