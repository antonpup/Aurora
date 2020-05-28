using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Spotify.GSI.Nodes
{
    public class ColorsNode : Node
    {
        public ColorNode Desaturated => NodeFor<ColorNode>("desaturated");
        public ColorNode LightVibrant => NodeFor<ColorNode>("light_vibrant");
        public ColorNode Prominent => NodeFor<ColorNode>("prominent");
        public ColorNode Vibrant => NodeFor<ColorNode>("vibrant");
        public ColorNode VibrantNonAlarming => NodeFor<ColorNode>("vibrant_non_alarming");

        public ColorThiefNode ColorThief => NodeFor<ColorThiefNode>("colorThief");

        public ColorsNode(string json) : base(json) { }
    }
}
