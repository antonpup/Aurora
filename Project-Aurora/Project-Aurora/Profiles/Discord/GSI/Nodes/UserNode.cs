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

    public class UserNode : AutoJsonNode<UserNode> {
        public long Id = 0;
        [AutoJsonIgnore] public DiscordStatus Status;
        [AutoJsonPropertyName("self_mute")] public bool SelfMute;
        [AutoJsonPropertyName("self_deafen")] public bool SelfDeafen;
        public bool Mentions;
        [AutoJsonPropertyName("unread_messages")] public bool UnreadMessages;
        [AutoJsonPropertyName("being_called")] public bool BeingCalled;

        internal UserNode(string json) : base(json) {
            Status = GetStatus(GetString("status"));
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
