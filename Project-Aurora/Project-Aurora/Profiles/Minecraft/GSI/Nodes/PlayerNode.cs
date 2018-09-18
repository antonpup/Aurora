using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Minecraft.GSI.Nodes {

    public class PlayerNode : Node<PlayerNode> {

        public bool InGame;

        public float Health;
        public float HealthMax;
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
        public PlayerEffectsNode PlayerEffects {
            get {
                _playerEffects = _playerEffects ?? new PlayerEffectsNode(_ParsedData["playerEffects"]?.ToString() ?? "");
                return _playerEffects;
            }
        }
        
        internal PlayerNode(string json) : base(json) {
            InGame = GetBool("inGame");

            Health = GetFloat("health");
            HealthMax = GetFloat("maxHealth");
            Absorption = GetFloat("absorption");
            IsDead = GetBool("isDead");
            Armor = GetInt("armor");

            ExperienceLevel = GetInt("experienceLevel");
            Experience = GetFloat("experience");

            FoodLevel = GetInt("foodLevel");
            SaturationLevel = GetFloat("saturationLevel");

            IsSneaking = GetBool("isSneaking");
            IsRidingHorse = GetBool("isRidingHorse");
            IsBurning = GetBool("isBurning");
            IsInWater = GetBool("isInWater");
        }

    }
}
