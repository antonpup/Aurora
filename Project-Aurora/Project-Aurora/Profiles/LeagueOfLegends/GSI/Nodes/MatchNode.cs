using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.LeagueOfLegends.GSI.Nodes
{
    public class MatchNode : Node<MatchNode>
    {
        public string GameMode;

        public float GameTime;

        public bool InGame;

        public int DragonsKilled;

        public int TurretsKilled;

        public int BaronsKilled;

        public int HeraldsKilled;
    }
}
