using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.LeagueOfLegends.GSI.Nodes
{
    enum Champion
    {

    }
    public class PlayerNode : Node<PlayerNode>
    {
        public StatsNode ChampionStats = new StatsNode();
        public int Level;
        public float CurrentGold;
        public string SummonerName;
        public bool IsDead;
    }
}
