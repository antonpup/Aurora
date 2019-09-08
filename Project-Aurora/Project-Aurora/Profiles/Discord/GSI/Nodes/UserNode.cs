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
        public int Status = -1;
        public bool SelfMute = false;
        public bool SelfDeafen = false;
        public bool Mentions = false;
        public bool DirectMessages = false;
        public bool ReceivingCall = false;
   
        internal UserNode() : base() { }
        internal UserNode(string json) : base(json) {
            Id = GetLong("id");
            Status = GetInt("status");
            SelfMute = GetBool("self_mute");
            SelfDeafen = GetBool("self_deafen");
            Mentions = GetBool("mentions");
            DirectMessages = GetBool("direct_messages");
            ReceivingCall = GetBool("being_called");
        }
    }
}
