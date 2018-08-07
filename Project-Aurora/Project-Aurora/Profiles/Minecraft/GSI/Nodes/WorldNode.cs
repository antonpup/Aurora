using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Minecraft.GSI.Nodes {
    public class WorldNode : Node<WorldNode> {

        public long WorldTime;
        public bool IsDayTime;
        public bool IsRaining;
        public float RainStrength;

        internal WorldNode(string json) : base(json) {
            WorldTime = GetLong("worldTime");
            IsDayTime = GetBool("isDayTime");
            IsRaining = GetBool("isRaining");
            RainStrength = GetFloat("rainStrength");
        }
    }
}
