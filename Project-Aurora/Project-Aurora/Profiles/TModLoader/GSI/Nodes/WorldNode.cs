using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.TModLoader.GSI.Nodes {
    public enum TerrariaBoss
    {
        None = -1,
        KingSlime = 0,
        EyeOfCthulu = 1,
        EaterOfWorlds = 2,
        BrainOfCthulu = 3,
        QueenBee = 4,
        Skeletron = 5,
        WallOfFlesh = 6,
        Twins = 7,
        Destroyer = 8,
        SkeletronPrime = 9,
        Plantera = 10,
        Golem = 11,
        DukeFishron = 12,
        LunaticCultist = 13,
        MoonLord = 14
    }

    public class WorldNode : AutoJsonNode<WorldNode> {
        public double Time;
        public bool Raining;
        public bool HardMode;
        public bool ExpertMode;
        public bool Eclipse;
        public bool BloodMoon;
        public bool PumpkinMoon;
        public bool SnowMoon;
        public bool SlimeRain;
        public TerrariaBoss Boss;

        internal WorldNode(string json) : base(json) {}
    }
}