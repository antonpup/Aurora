using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Minecraft.GSI.Nodes {

    public class PlayerNode : AutoNode<PlayerNode> {

        public bool InGame;

        public float Health;
        public float MaxHealth;
        public float HealthMax => MaxHealth; // "HealthMax" was the original name, but when converting to an AutoNode, the name does not match the JSON, so the "MaxHealth" was added to match JSON but this was kept so as not to break existing profiles.
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

        private PlayerEffectsNode _playerEffects;
        public PlayerEffectsNode PlayerEffects => _playerEffects ?? (_playerEffects = new PlayerEffectsNode(GetString("playerEffects")));

        internal PlayerNode() : base() { }
        internal PlayerNode(string json) : base(json) { }
    }
}
