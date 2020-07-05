using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Discord.GSI.Nodes {
    public enum DiscordVoiceType
    {
        Undefined = -1,
        Call = 1,
        VoiceChannel = 2,
    }

    public class VoiceNode : AutoJsonNode<VoiceNode> {
        public long Id = 0;
        public string Name;
        public DiscordVoiceType Type;

        internal VoiceNode(string json) : base(json) { }
    }
}
