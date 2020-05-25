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
        TheDryReef = 1,
        TheIndigoQuarry = 2,
        TheMossBlanket = 3,
        TheGlassDesert = 4,
        TheSlimeSea = 5,
        TheAncientRuins = 7,
        TheAncientRuinsCourtyard = 8,
        TheWilds = 9,
        OgdensRetreat = 10,
        NimbleValley = 11,
        MochisManor = 12,
        TheSlimeulation = 13,
        ViktorsWorkshop = 14
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
        public bool TheDryReef => Location == SRZone.TheDryReef;
        public bool TheIndigoQuarry => Location == SRZone.TheIndigoQuarry;
        public bool TheMossBlanket => Location == SRZone.TheMossBlanket;
        public bool TheGlassDesert => Location == SRZone.TheGlassDesert;
        public bool TheSlimeSea => Location == SRZone.TheSlimeSea;
        public bool TheAncientRuins => Location == SRZone.TheAncientRuins;
        public bool TheAncientRuinsCourtyard => Location == SRZone.TheAncientRuinsCourtyard;
        public bool TheWilds => Location == SRZone.TheWilds;
        public bool OgdensRetreat => Location == SRZone.OgdensRetreat;
        public bool NimbleValley => Location == SRZone.NimbleValley;
        public bool MochisManor => Location == SRZone.MochisManor;
        public bool TheSlimeulation => Location == SRZone.TheSlimeulation;
        public bool ViktorsWorkshop => Location == SRZone.ViktorsWorkshop;

        internal BiomeNode(string json) : base(json) { }
    }

}

