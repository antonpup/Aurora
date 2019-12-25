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
        public DiscordStatus Status;
        public bool SelfMute;
        public bool SelfDeafen;
        public bool Mentions;
        public bool UnreadMessages;

        internal UserNode(string json) : base(json) {
            Id = GetLong("id");
            Status = GetStatus(GetString("status"));
            SelfMute = GetBool("self_mute");
            SelfDeafen = GetBool("self_deafen");
            Mentions = GetBool("mentions");
            UnreadMessages = GetBool("unread_messages");
        }

        private static DiscordStatus GetStatus(string status)
        {
            switch (status)
            {
                case "online":
                    return DiscordStatus.Online;
                case "dnd":
                    return DiscordStatus.DoNotDisturb;
                case "invisible":
                    return DiscordStatus.Invisible;
                case "idle":
                    return DiscordStatus.Idle;
                default:
                    return DiscordStatus.Undefined;
            }
        }
    }
}
