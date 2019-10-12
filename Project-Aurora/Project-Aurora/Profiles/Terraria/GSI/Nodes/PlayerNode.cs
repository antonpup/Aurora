using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Terraria.GSI.Nodes {
    public enum TerrariaBiome
    {
        None = -1,
        Forest = 0,
        Beach = 1,
        Jungle = 2,
        Snow = 3,
        Desert = 4
    }

    public enum TerrariaDepth
    {
        None = -1,
        Underworld = 0,
        Cavern = 1,
        Underground = 2,
        Overworld = 3,
        Sky = 4
    }

    public class PlayerNode : Node<PlayerNode> {
        public bool InGame;

        #region Stats
        public int Health;
        public int MaxHealth;
        public int Mana;
        public int MaxMana;
        public int Defense;
        #endregion

        #region Location
        public int Depth;
        public TerrariaDepth DepthLayer;
        public int MaxDepth;
        public TerrariaBiome Biome;
        #endregion

        #region Microbiomes
        public bool InGlowshroom;
        public bool InUndergroundDesert;
        public bool InSandstorm;
        public bool InMeteor;
        public bool InDungeon;
        #endregion

        #region Local Events
        public bool InTowerSolar;
        public bool InTowerVortex;
        public bool InTowerNebula;
        public bool InTowerStardust;
        public bool InOldOneArmy;
        #endregion

        #region Local Corruption Status
        public bool InCorruption;
        public bool InCrimson;
        public bool InHalllow;
        #endregion

        internal PlayerNode(string json) : base(json)
        {
            InGame = GetBool("inGame");

            Health = GetInt("health");
            MaxHealth = GetInt("maxHealth");
            Mana = GetInt("mana");
            MaxMana = GetInt("maxMana");
            Defense = GetInt("defense");

            Depth = GetInt("depth");
            DepthLayer = (TerrariaDepth)GetInt("depthLayer");
            MaxDepth = GetInt("maxdepth");
            Biome = (TerrariaBiome)GetInt("biome");

            InGlowshroom = GetBool("zoneGlowshroom");
            InUndergroundDesert = GetBool("zoneUndergroundDesert");
            InSandstorm = GetBool("zoneSandstorm");
            InMeteor = GetBool("zoneMeteor");
            InDungeon = GetBool("zoneDungeon");

            InTowerSolar = GetBool("zoneTowerSolar");
            InTowerVortex = GetBool("zoneTowerVortex");
            InTowerNebula = GetBool("zoneTowerNebula");
            InTowerStardust = GetBool("zoneTowerStardust");
            InOldOneArmy = GetBool("zoneOldOneArmy");

            InCorruption = GetBool("zoneCorrupt");
            InCrimson = GetBool("zoneCrimson");
            InHalllow = GetBool("zoneHoly");
        }
    }
}
