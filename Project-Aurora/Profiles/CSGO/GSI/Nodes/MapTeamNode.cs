using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    public class MapTeamNode : Node
    {
        public readonly int Score;

        internal MapTeamNode(string JSON)
            : base(JSON)
        {
            Score = GetInt("score");
        }
    }
}
