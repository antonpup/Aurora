using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Spotify.GSI.Nodes
{
    public class PlayerNode : AutoJsonNode<PlayerNode>
    {
        public long Duration = 0;
        public long Progress = 0;
        public bool Mute = false;
        public int Repeat = 0;
        public bool Shuffle = false;
        public bool Heart = false;
        public int Volume = 0;
        public bool Playing = false;

        internal PlayerNode(string json) : base(json) { }
    }
}