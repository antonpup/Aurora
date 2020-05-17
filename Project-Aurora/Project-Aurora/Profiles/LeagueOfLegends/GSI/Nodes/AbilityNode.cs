using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.LeagueOfLegends.GSI.Nodes
{
    public class AbilityNode : Node
    {
        public bool Learned => Level != 0;
        public int Level;
        public string Name = "";

        //TODO: there might be additional useful info to add here such as cooldown
    }
}
