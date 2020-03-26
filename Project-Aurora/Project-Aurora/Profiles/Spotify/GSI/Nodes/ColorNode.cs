using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Spotify.GSI.Nodes
{
    public class ColorNode : AutoJsonNode<ColorNode>
    {
        [AutoJsonPropertyName("r")]
        public float Red;
        [AutoJsonPropertyName("g")]
        public float Green;
        [AutoJsonPropertyName("b")]
        public float Blue;

        public ColorNode(string json) : base(json) { }
    }
}