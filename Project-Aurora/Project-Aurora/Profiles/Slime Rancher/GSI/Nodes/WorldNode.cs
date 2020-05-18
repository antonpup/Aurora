using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Slime_Rancher.GSI.Nodes {
    public class WorldNode : Node {

        public TimeNode Time;
        public bool Paused;

        internal WorldNode(string json) : base(json) {
            Time = new TimeNode(_ParsedData["time"]?.ToString() ?? "");
            Paused = GetBool("paused");
        }
    }

    public class TimeNode : Node
    {
        public int Hour;
        public int Min;

        internal TimeNode(string json) : base(json)
        {
            this.Hour = GetInt("hour");
            this.Min = GetInt("min");
        }
    }
}
