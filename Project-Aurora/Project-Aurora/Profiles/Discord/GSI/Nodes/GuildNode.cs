using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Discord.GSI.Nodes {
    public class GuildNode : AutoJsonNode<GuildNode> {
        public long Id;
        public string Name;

        internal GuildNode(string json) : base(json) { }
    }
}
