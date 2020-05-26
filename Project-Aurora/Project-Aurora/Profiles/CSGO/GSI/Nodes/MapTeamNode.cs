using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    /// <summary>
    /// Class representing team information
    /// </summary>
    public class MapTeamNode : Node
    {
        /// <summary>
        /// Team score
        /// </summary>
        public int Score;

        /// <summary>
        /// Remaining Timeouts
        /// </summary>
        public int TimeoutsRemaining;

        internal MapTeamNode(string JSON)
            : base(JSON)
        {
            Score = GetInt("score");
            TimeoutsRemaining = GetInt("timeouts_remaining");
        }
    }
}
