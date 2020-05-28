using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Profiles.LeagueOfLegends.GSI.Nodes;

namespace Aurora.Profiles.LeagueOfLegends.GSI
{
    public class GameState_LoL : GameState
    {
        private PlayerNode player;
        public PlayerNode Player => player ?? (player = new PlayerNode());

        private MatchNode match;
        public MatchNode Match => match ?? (match = new MatchNode());

        public GameState_LoL() : base()
        {

        }

        public GameState_LoL(string json_data) : base(json_data)
        {

        }
    }
}
