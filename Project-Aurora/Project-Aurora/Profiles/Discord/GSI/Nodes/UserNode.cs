using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Discord.GSI.Nodes {

    public enum DiscordStatus
    {
        Undefined,
        Online,
        Idle,
        DoNotDisturb,
        Invisible
    }


    public class UserNode : Node<UserNode> {

        public long Id = 0;
        public DiscordStatus Status = DiscordStatus.Undefined;
        public bool SelfMute = false;
        public bool SelfDeafen = false;
        public int Mentions = 0;
        public int DirectMessages = 0;


        internal UserNode() : base() { }
        internal UserNode(string json) : base(json) {
            Id = GetLong("id");
            Status = GetEnum<DiscordStatus>("status");
            SelfMute = GetBool("self_mute");
            SelfDeafen = GetBool("self_deafen");
            Mentions = GetInt("mentions");
            DirectMessages = GetInt("direct_messages");
        }
    }
}
