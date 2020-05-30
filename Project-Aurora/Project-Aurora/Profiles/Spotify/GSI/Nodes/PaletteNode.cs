using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Spotify.GSI.Nodes
{
    public class PaletteNode : Node
    {
        public ColorNode Color1 => NodeFor<ColorNode>("color_1");
        public ColorNode Color2 => NodeFor<ColorNode>("color_2");
        public ColorNode Color3 => NodeFor<ColorNode>("color_3");
        public ColorNode Color4 => NodeFor<ColorNode>("color_4");
        public ColorNode Color5 => NodeFor<ColorNode>("color_5");

        public PaletteNode(string json) : base(json){ }
    }
}
