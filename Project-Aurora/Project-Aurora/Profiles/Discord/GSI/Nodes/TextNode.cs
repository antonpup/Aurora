using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Discord.GSI.Nodes {

    public enum DiscordTextType
    {
        Undefined = -1,
        TextChannel = 0,
        DirectMessage = 1,
        GroupChat = 3
    }


    public class TextNode : Node<TextNode> {

        public long Id = 0;
        public string Name;
        public int Type;

        internal TextNode() : base() { }
        internal TextNode(string json) : base(json) {
            Id = GetLong("id");
            Name = GetString("name");
            Type = GetInt("type");
        }
    }
}
