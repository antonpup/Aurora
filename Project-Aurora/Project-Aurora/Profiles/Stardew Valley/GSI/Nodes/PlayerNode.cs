using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.StardewValley.GSI.Nodes
{

    public class PlayerNode : AutoJsonNode<PlayerNode>
    {
        public enum Locations
        {
            Unknown = -1,
            Farm,
            Beach,
            AnimalHouse,
            SlimeHutch,
            Shed,
            LibraryMuseum,
            AdventureGuild,
            Woods,
            Railroad,
            Summit,
            Forest,
            ShopLocation,
            SeedShop,
            FishShop,
            BathHousePool,
            FarmHouse,
            Cabin,
            Club,
            BusStop,
            CommunityCenter,
            Desert,
            FarmCave,
            JojaMart,
            MineShaft,
            Mountain,
            Sewer,
            WizardHouse,
            Town,
            Cellar,
            Submarine,
            MermaidHouse,
            BeachNightMarket,
            MovieTheater,
            ManorHouse,
            AbandonedJojaMart,
            Mine
        }

        public HealthNode Health => NodeFor<HealthNode>("Health");
        public string LocationName;
        public Locations Location;
        public bool IsOutdoor;
        public EnergyNode Energy => NodeFor<EnergyNode>("Energy");

        internal PlayerNode(string json) : base(json){ }
        
        public class HealthNode : AutoJsonNode<HealthNode>
        {
            public int Current;
            public int Max;
            public bool BarActive;

            internal HealthNode(string JSON) : base(JSON) { }
        }

        public class EnergyNode : AutoJsonNode<EnergyNode>
        {
            public int Current;
            public int Max;

            internal EnergyNode(string JSON) : base(JSON) { }
        }
    }
}
