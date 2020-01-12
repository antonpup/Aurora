using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Spotify.GSI.Nodes
{
    public class ColorsNode : Node<ColorsNode>
    {
        public ColorNode Desaturated;
        public ColorNode LightVibrant;
        public ColorNode Prominent;
        public ColorNode Vibrant;
        public ColorNode VibrantNonAlarming;

        public ColorsNode(string json) : base(json)
        {
            Desaturated = new ColorNode(_ParsedData["desaturated"]?.ToString() ?? "");
            LightVibrant = new ColorNode(_ParsedData["light_vibrant"]?.ToString() ?? "");
            Prominent = new ColorNode(_ParsedData["prominent"]?.ToString() ?? "");
            Vibrant = new ColorNode(_ParsedData["vibrant"]?.ToString() ?? "");
            VibrantNonAlarming = new ColorNode(_ParsedData["vibrant_non_alarming"]?.ToString() ?? "");
        }
    }
}
