using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.TModLoader.GSI.Nodes
{
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

    public class PlayerNode : AutoJsonNode<PlayerNode>
    {
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
        [AutoJsonPropertyName("zoneGlowshroom")] public bool InGlowshroom;
        [AutoJsonPropertyName("zoneUndergroundDesert")] public bool InUndergroundDesert;
        [AutoJsonPropertyName("zoneSandstorm")] public bool InSandstorm;
        [AutoJsonPropertyName("zoneMeteor")] public bool InMeteor;
        [AutoJsonPropertyName("zoneDungeon")] public bool InDungeon;
        #endregion

        #region Local Events
        [AutoJsonPropertyName("zoneTowerSolar")] public bool InTowerSolar;
        [AutoJsonPropertyName("zoneTowerVortex")] public bool InTowerVortex;
        [AutoJsonPropertyName("zoneTowerNebula")] public bool InTowerNebula;
        [AutoJsonPropertyName("zoneTowerStardust")] public bool InTowerStardust;
        [AutoJsonPropertyName("zoneOldOneArmy")] public bool InOldOneArmy;
        #endregion

        #region Local Corruption Status
        [AutoJsonPropertyName("zoneCorrupt")] public bool InCorruption;
        [AutoJsonPropertyName("zoneCrimson")] public bool InCrimson;
        [AutoJsonPropertyName("zoneHoly")] public bool InHalllow;
        #endregion

        internal PlayerNode(string json) : base(json){ }
    }
}
