using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.LeagueOfLegends.GSI.Nodes
{
    public enum ResourceType
    {
        Unknown = -1,
        None = 0,
        Mana,
        Energy,
        Shield,
        Battlefury,
        Dragonfury,
        Rage,
        Heat,
        Gnarfury,
        Ferocity,
        Bloodwell,
        Wind,
        Ammo,
        Other,
        Max
    }

    public class StatsNode : Node
    {
        public float AbilityPower;
        public float Armor;
        public float ArmorPenetrationFlat;
        public float ArmorPenetrationPercent;
        public float AttackDamage;
        public float AttackRange;
        public float AttackSpeed;
        public float BonusArmorPenetrationPercent;
        public float BonusMagicPenetrationPercent;
        public float CooldownReduction;
        public float CritChance;
        public float CritDamagePercent;
        public float HealthCurrent;
        public float HealthMax;
        public float HealthRegenRate;
        public float LifeSteal;
        public float MagicLethality;
        public float MagicPenetrationFlat;
        public float MagicPenetrationPercent;
        public float MagicResist;
        public float MoveSpeed;
        public float PhysicalLethality;
        public float ResourceCurrent;
        public float ResourceMax;
        public float ResourceRegenRate;
        public ResourceType ResourceType;
        public float SpellVamp;
        public float Tenacity;
    }
}
