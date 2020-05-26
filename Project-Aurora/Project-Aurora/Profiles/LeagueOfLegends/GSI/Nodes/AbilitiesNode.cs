using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.LeagueOfLegends.GSI.Nodes
{
    public class AbilitiesNode : Node
    {
        public AbilityNode Q = new AbilityNode();

        public AbilityNode W = new AbilityNode();

        public AbilityNode E = new AbilityNode();

        public AbilityNode R = new AbilityNode();

        //TODO: if there is anything useful for the passive later, add it here
    }
}
