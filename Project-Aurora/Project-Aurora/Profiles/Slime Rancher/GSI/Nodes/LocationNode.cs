using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Slime_Rancher.GSI.Nodes
{
    public enum SRZone
    {
        None = -1,
        TheRanch = 0,
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

    public class LocationNode : AutoJsonNode<LocationNode>
    {
        public SRZone Location => In.Location; //legacy
        [AutoJsonPropertyName("zone_name")]
        public string ZoneName;
        [AutoJsonIgnore]
        public BiomeNode In; //legacy


        internal LocationNode(string json) : base(json)
        {
            In = new BiomeNode(json); //legacy
        }
    }

    public class BiomeNode : AutoJsonNode<BiomeNode>
    {
        [GameStateIgnore]
        public SRZone Location => (SRZone)GetInt("zone");

        public bool None => Location == SRZone.None;
        public bool TheRanch => Location == SRZone.TheRanch;
        public bool TheDryReef => Location == SRZone.REEF;
        public bool TheIndigoQuarry => Location == SRZone.QUARRY;
        public bool TheMossBlanket => Location == SRZone.MOSS;
        public bool TheGlassDesert => Location == SRZone.DESERT;
        public bool TheSlimeSea => Location == SRZone.SEA;
        public bool TheAncientRuins => Location == SRZone.RUINS;
        public bool TheAncientRuinsCourtyard => Location == SRZone.RUINS_TRANSITION;
        public bool TheWilds => Location == SRZone.WILDS;
        public bool OgdensRetreat => Location == SRZone.OGDEN_RANCH;
        public bool NimbleValley => Location == SRZone.VALLEY;
        public bool MochisManor => Location == SRZone.MOCHI_RANCH;
        public bool TheSlimeulation => Location == SRZone.SLIMULATIONS;
        public bool ViktorsWorkshop => Location == SRZone.VIKTOR_LAB;

        internal BiomeNode(string json) : base(json) { }
    }

}

