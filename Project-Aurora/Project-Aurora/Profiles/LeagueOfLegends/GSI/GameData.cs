using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.LeagueOfLegends.GSI
{
    //Mostly auto generated with jsonutils.com

    public class Passive
    {
        public string displayName { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
    }

    public class Ability
    {
        public int abilityLevel { get; set; }
        public string displayName { get; set; }
        public string id { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
    }

    public class Abilities
    {
        public Passive Passive { get; set; }
        public Ability Q { get; set; }
        public Ability W { get; set; }
        public Ability E { get; set; }
        public Ability R { get; set; }
    }

    public class ChampionStats
    {
        public float abilityPower { get; set; }
        public float armor { get; set; }
        public float armorPenetrationFlat { get; set; }
        public float armorPenetrationPercent { get; set; }
        public float attackDamage { get; set; }
        public float attackRange { get; set; }
        public float attackSpeed { get; set; }
        public float bonusArmorPenetrationPercent { get; set; }
        public float bonusMagicPenetrationPercent { get; set; }
        public float cooldownReduction { get; set; }
        public float critChance { get; set; }
        public float critDamage { get; set; }
        public float currentHealth { get; set; }
        public float healthRegenRate { get; set; }
        public float lifeSteal { get; set; }
        public float magicLethality { get; set; }
        public float magicPenetrationFlat { get; set; }
        public float magicPenetrationPercent { get; set; }
        public float magicResist { get; set; }
        public float maxHealth { get; set; }
        public float moveSpeed { get; set; }
        public float physicalLethality { get; set; }
        public float resourceMax { get; set; }
        public float resourceRegenRate { get; set; }
        public string resourceType { get; set; }
        public float resourceValue { get; set; }
        public float spellVamp { get; set; }
        public float tenacity { get; set; }
    }

    public class Rune
    {
        public string displayName { get; set; }
        public int id { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
    }

    public class StatRune
    {
        public int id { get; set; }
        public string rawDescription { get; set; }
    }

    public class FullRunes
    {
        public IList<Rune> generalRunes { get; set; }
        public Rune keystone { get; set; }
        public Rune primaryRuneTree { get; set; }
        public Rune secondaryRuneTree { get; set; }
        public IList<StatRune> statRunes { get; set; }
    }

    public class ActivePlayer
    {
        public Abilities abilities { get; set; }
        public ChampionStats championStats { get; set; }
        public float currentGold { get; set; }
        public FullRunes fullRunes { get; set; }
        public int level { get; set; }
        public string summonerName { get; set; }
    }

    public class Item
    {
        public bool canUse { get; set; }
        public bool consumable { get; set; }
        public int count { get; set; }
        public string displayName { get; set; }
        public int itemID { get; set; }
        public int price { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
        public int slot { get; set; }
    }

    public class Runes
    {
        public Rune keystone { get; set; }
        public Rune primaryRuneTree { get; set; }
        public Rune secondaryRuneTree { get; set; }
    }

    public class Scores
    {
        public int assists { get; set; }
        public int creepScore { get; set; }
        public int deaths { get; set; }
        public int kills { get; set; }
        public float wardScore { get; set; }
    }

    public class SummonerSpellOne
    {
        public string displayName { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
    }

    public class SummonerSpellTwo
    {
        public string displayName { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
    }

    public class SummonerSpells
    {
        public SummonerSpellOne summonerSpellOne { get; set; }
        public SummonerSpellTwo summonerSpellTwo { get; set; }
    }

    public class AllPlayer
    {
        public string championName { get; set; }
        public bool isBot { get; set; }
        public bool isDead { get; set; }
        public IList<Item> items { get; set; }
        public int level { get; set; }
        public string position { get; set; }
        public string rawChampionName { get; set; }
        public float respawnTimer { get; set; }
        public Runes runes { get; set; }
        public Scores scores { get; set; }
        public int skinID { get; set; }
        public string summonerName { get; set; }
        public SummonerSpells summonerSpells { get; set; }
        public string team { get; set; }
    }

    public class Event
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public float EventTime { get; set; }
    }

    public class EventList
    {
        public IList<Event> Events { get; set; }
    }

    public class GameData
    {
        public string gameMode { get; set; }
        public float gameTime { get; set; }
        public string mapName { get; set; }
        public int mapNumber { get; set; }
        public string mapTerrain { get; set; }
    }

    public class RootGameData
    {
        public ActivePlayer activePlayer { get; set; }
        public IList<AllPlayer> allPlayers { get; set; }
        public EventList events { get; set; }
        public GameData gameData { get; set; }
    }
}
