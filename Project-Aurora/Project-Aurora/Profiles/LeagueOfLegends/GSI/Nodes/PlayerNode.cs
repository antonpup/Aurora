using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.LeagueOfLegends.GSI.Nodes
{
    public class PlayerNode : Node<PlayerNode>
    {
        public StatsNode Stats = new StatsNode();
        public int Level;
        public float Gold;
    }
}
