using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Minecraft.GSI.Nodes {

    public class PlayerNode : AutoJsonNode<PlayerNode> {

        public bool InGame;

        public float Health;
        [AutoJsonPropertyName("maxHealth")] public float HealthMax;
        public float Absorption;
        public float AbsorptionMax = 20;
        public bool IsDead;
        public int Armor;
        public int ArmorMax = 20;

        public int ExperienceLevel;
        public float Experience;
        public float ExperienceMax = 1;

        public int FoodLevel;
        public int FoodLevelmax = 20;
        public float SaturationLevel;
        public float SaturationLevelMax = 20;

        public bool IsSneaking;
        public bool IsRidingHorse;
        public bool IsBurning;
        public bool IsInWater;

        public PlayerEffectsNode PlayerEffects => NodeFor<PlayerEffectsNode>("playerEffects");
        
        internal PlayerNode(string json) : base(json) { }
    }
}
