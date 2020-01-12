using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Spotify.GSI.Nodes
{
    public class ColorNode : Node<ColorNode>
    {
        public float Red;
        public float Green;
        public float Blue;

        public ColorNode(string json) : base(json)
        {
            Red = GetFloat("r");
            Green = GetFloat("g");
            Blue = GetFloat("b");
        }
    }
}