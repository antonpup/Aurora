using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Spotify.GSI.Nodes
{
    public class ColorThiefNode : Node
    {
        public PaletteNode Palette => NodeFor<PaletteNode>("Palette");

        public ColorThiefNode(string json) : base(json) { }
    }
}
