using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.RocketLeague
{
    public class PointerData
    {
        public int baseAddress { get; set; }
        public int[] pointers { get; set; }
    }

    public class RocketLeaguePointers
    {
        public PointerData Team;
        public PointerData Orange_score;
        public PointerData Blue_score;
        public PointerData Boost_amout;
    }
}
