using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.LeagueOfLegends.GSI.Nodes
{
	public enum ResourceType
	{
		MANA,
		ENERGY,
		NONE,
		SHIELD,
		BATTLEFURY,
		DRAGONFURY,
		RAGE,
		HEAT,
		GNARFURY,
		FEROCITY,
		BLOODWELL,
		WIND,
		AMMO,
		OTHER,
		MAX
	}

    public class StatsNode : Node<StatsNode>
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
		public float CritDamage;
		public float CurrentHealth;
		public float HealthRegenRate;
		public float LifeSteal;
		public float MagicLethality;
		public float MagicPenetrationFlat;
		public float MagicPenetrationPercent;
		public float MagicResist;
		public float MaxHealth;
		public float MoveSpeed;
		public float PhysicalLethality;
		public float ResourceMax;
		public float ResourceRegenRate;
		public ResourceType ResourceType;
		public float ResourceValue;
		public float SpellVamp;
		public float Tenacity;
	}
}
