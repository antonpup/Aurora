using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Slime_Rancher.GSI.Nodes
{
    public class LocationNode : Node<LocationNode>
    {
        public BiomeNode In;

        internal LocationNode(string json) : base(json)
        {
            In = new BiomeNode(json);
        }

    }

    public class BiomeNode : Node<BiomeNode>
    {
        public bool None;
        public bool TheRanch;
        public bool TheDryReef;
        public bool TheIndigoQuarry;
        public bool TheMossBlanket;
        public bool TheGlassDesert;
        public bool TheSlimeSea;
        public bool TheAncientRuins;
        public bool TheAncientRuinsCourtyard;
        public bool TheWilds;
        public bool OgdensRetreat;
        public bool NimbleValley;
        public bool MochisManor;
        public bool TheSlimeulation;
        public bool ViktorsWorkshop;

        internal BiomeNode(string json) : base(json)
        {
            SRZone Location = (SRZone)GetInt("zone");

            this.None = Location == SRZone.NONE;
            this.TheRanch = Location == SRZone.RANCH;
            this.TheDryReef = Location == SRZone.REEF;
            this.TheIndigoQuarry = Location == SRZone.QUARRY;
            this.TheMossBlanket = Location == SRZone.MOSS;
            this.TheGlassDesert = Location == SRZone.DESERT;
            this.TheSlimeSea = Location == SRZone.SEA;
            this.TheAncientRuins = Location == SRZone.RUINS;
            this.TheAncientRuinsCourtyard = Location == SRZone.RUINS_TRANSITION;
            this.TheWilds = Location == SRZone.WILDS;
            this.OgdensRetreat = Location == SRZone.OGDEN_RANCH;
            this.NimbleValley = Location == SRZone.VALLEY;
            this.MochisManor = Location == SRZone.MOCHI_RANCH;
            this.TheSlimeulation = Location == SRZone.SLIMULATIONS;
            this.ViktorsWorkshop = Location == SRZone.VIKTOR_LAB;
        }
    }

}

enum SRZone
{
    NONE = -1,
    RANCH = 0,
    REEF = 1,
    QUARRY = 2,
    MOSS = 3,
    DESERT = 4,
    SEA = 5,
    RUINS = 7,
    RUINS_TRANSITION = 8,
    WILDS = 9,
    OGDEN_RANCH = 10,
    VALLEY = 11,
    MOCHI_RANCH = 12,
    SLIMULATIONS = 13,
    VIKTOR_LAB = 14
}

