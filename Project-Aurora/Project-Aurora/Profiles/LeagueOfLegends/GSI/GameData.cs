using JsonSubTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.LeagueOfLegends.GSI
{
    //Mostly auto generated with jsonutils.com

    public class _Passive
    {
        public string displayName { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
    }

    public class _Ability
    {
        public int abilityLevel { get; set; }
        public string displayName { get; set; }
        public string id { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
    }

    public class _Abilities
    {
        public _Passive Passive { get; set; }
        public _Ability Q { get; set; }
        public _Ability W { get; set; }
        public _Ability E { get; set; }
        public _Ability R { get; set; }
    }

    public class _ChampionStats
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

    public class _Rune
    {
        public string displayName { get; set; }
        public int id { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
    }

    public class _StatRune
    {
        public int id { get; set; }
        public string rawDescription { get; set; }
    }

    public class _FullRunes
    {
        public IList<_Rune> generalRunes { get; set; }
        public _Rune keystone { get; set; }
        public _Rune primaryRuneTree { get; set; }
        public _Rune secondaryRuneTree { get; set; }
        public IList<_StatRune> statRunes { get; set; }
    }

    public class _ActivePlayer
    {
        public _Abilities abilities { get; set; }
        public _ChampionStats championStats { get; set; }
        public float currentGold { get; set; }
        public _FullRunes fullRunes { get; set; }
        public int level { get; set; }
        public string summonerName { get; set; }
    }

    public class _Item
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

    public class _Runes
    {
        public _Rune keystone { get; set; }
        public _Rune primaryRuneTree { get; set; }
        public _Rune secondaryRuneTree { get; set; }
    }

    public class _Scores
    {
        public int assists { get; set; }
        public int creepScore { get; set; }
        public int deaths { get; set; }
        public int kills { get; set; }
        public float wardScore { get; set; }
    }

    public class _SummonerSpellOne
    {
        public string displayName { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
    }

    public class _SummonerSpellTwo
    {
        public string displayName { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
    }

    public class _SummonerSpells
    {
        public _SummonerSpellOne summonerSpellOne { get; set; }
        public _SummonerSpellTwo summonerSpellTwo { get; set; }
    }

    public class _AllPlayer
    {
        public string championName { get; set; }
        public bool isBot { get; set; }
        public bool isDead { get; set; }
        public IList<_Item> items { get; set; }
        public int level { get; set; }
        public string position { get; set; }
        public string rawChampionName { get; set; }
        public float respawnTimer { get; set; }
        public _Runes runes { get; set; }
        public _Scores scores { get; set; }
        public int skinID { get; set; }
        public string summonerName { get; set; }
        public _SummonerSpells summonerSpells { get; set; }
        public string team { get; set; }
    }

    [JsonConverter(typeof(JsonSubtypes), "EventName")]
    [JsonSubtypes.KnownSubType(typeof(_BaronKillEvent), "BaronKill")]
    [JsonSubtypes.KnownSubType(typeof(_HeraldKillEvent), "HeraldKill")]
    [JsonSubtypes.KnownSubType(typeof(_DragonKillEvent), "DragonKill")]
    [JsonSubtypes.KnownSubType(typeof(_ChampionKillEvent), "ChampionKill")]
    [JsonSubtypes.KnownSubType(typeof(_MultikillEvent), "Multikill")]
    [JsonSubtypes.KnownSubType(typeof(_AceEvent), "Ace")]
    [JsonSubtypes.KnownSubType(typeof(_InhibKillEvent), "InhibKilled")]
    [JsonSubtypes.KnownSubType(typeof(_TurretKillEvent), "TurretKilled")]
    public class _Event
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public float EventTime { get; set; }
    }

    public class _BaronKillEvent : _Event
    {
        public bool Stolen { get; set; }
        public string KillerName { get; set; }
        public string[] Assisters { get; set; }
    }

    public class _HeraldKillEvent : _Event
    {
        public bool Stolen { get; set; }
        public string KillerName { get; set; }
        public string[] Assisters { get; set; }
    }

    public class _DragonKillEvent : _Event
    {
        public string DragonType { get; set; }
        public bool Stolen { get; set; }
        public string KillerName { get; set; }
        public string[] Assisters { get; set; }
    }

    public class _ChampionKillEvent : _Event
    {
        public string KillerName { get; set; }
        public string VictimName { get; set; }
        public string[] Assisters { get; set; }
    }

    public class _MultikillEvent : _Event
    {
        public string KillerName { get; set; }
        public int KillStreak { get; set; }
    }

    public class _AceEvent : _Event
    {
        public string Acer { get; set; }
        public string AcingTeam { get; set; }
    }

    public class _InhibKillEvent : _Event
    {
        public string KillerName { get; set; }
        public string InhibKilled { get; set; }
        public string[] Assisters { get; set; }
    }

    public class _TurretKillEvent : _Event
    {
        public string KillerName { get; set; }
        public string TurretKilled { get; set; }
        public string[] Assisters { get; set; }
    }

    public class _EventList
    {
        public IList<_Event> Events { get; set; }
    }

    public class _GameData
    {
        public string gameMode { get; set; }
        public float gameTime { get; set; }
        public string mapName { get; set; }
        public int mapNumber { get; set; }
        public string mapTerrain { get; set; }
    }

    public class _RootGameData
    {
        public _ActivePlayer activePlayer { get; set; }
        public IList<_AllPlayer> allPlayers { get; set; }
        public _EventList events { get; set; }
        public _GameData gameData { get; set; }
    }
}
