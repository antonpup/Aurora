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
        public string Status = "";
        public bool SelfMute = false;
        public bool SelfDeafen = false;
        public bool Mentions = false;
        public bool UnreadMessages = false;
   
        internal UserNode() : base() { }
        internal UserNode(string json) : base(json) {
            Id = GetLong("id");
            Status = GetString("status");
            SelfMute = GetBool("self_mute");
            SelfDeafen = GetBool("self_deafen");
            Mentions = GetBool("mentions");
            UnreadMessages = GetBool("unread_messages");
        }
    }
}
