using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Subnautica.GSI.Nodes {
    public class WorldNode : Node<WorldNode> {

        public float DayScalar;
        //public bool IsDay;

        internal WorldNode(string json) : base(json) {
            DayScalar = GetFloat("day_scalar");
            /*if (DayScalar >= 0.15 && DayScalar <= 0.89)
                IsDay = true;
            else
                IsDay = false;
                */
        }
    }
}

/* Between ~0.15 and ~0.85, world is fully bright day time
 * Between ~0.85 and 0.9 world transitions from day to night
 * Between ~0.9 and ~0.1 world is fully night time
 * Between ~0.1 and ~0.15 world is transitions from night to day
 */
