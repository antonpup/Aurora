using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Discord.GSI.Nodes {
    public class GuildNode : Node<GuildNode> {

        public long Id = 0;
        public string Name = String.Empty;

        internal GuildNode() : base() { }
        internal GuildNode(string json) : base(json) {
            Id = GetLong("id");
            Name = GetString("name");
        }
    }
}
