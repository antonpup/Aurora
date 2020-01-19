using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Aurora.Profiles;
using System.Net;
using Aurora.Profiles.LeagueOfLegends.GSI;
using Aurora.Profiles.LeagueOfLegends.GSI.Nodes;
using Newtonsoft.Json.Linq;

namespace Aurora.Profiles.LeagueOfLegends
{
    public class GameEvent_LoL : LightEvent
    {
        private readonly HttpClient client = new HttpClient();
        private const string URI = "https://localhost:2999/liveclientdata/allgamedata";
        private GameData gameData;

        public override void UpdateTick()
        {
            if (!Global.LightingStateManager.RunningProcessMonitor.IsProcessRunning("league of legends.exe"))
                return;

            string jsonData = "";
            try
            {
                using (var res = client.GetAsync(URI).Result)
                {
                    if (res.IsSuccessStatusCode)
                        jsonData = res.Content.ReadAsStringAsync().Result;
                }
            }
            catch
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(jsonData) || jsonData.Contains("error"))
                return;

            gameData = JsonConvert.DeserializeObject<GameData>(jsonData);
            var player = Array.Find(gameData.allplayers, p => p.summonerName == gameData.activePlayer.SummonerName);
            var playerState = (_game_state as GameState_LoL).Player;

            #region ActivePlayer stats
            playerState.ChampionStats.AbilityPower = gameData.activePlayer.championStats.AbilityPower;
            playerState.ChampionStats.Armor = gameData.activePlayer.championStats.Armor;
            playerState.ChampionStats.ArmorPenetrationFlat = gameData.activePlayer.championStats.ArmorPenetrationFlat;
            playerState.ChampionStats.ArmorPenetrationPercent = gameData.activePlayer.championStats.ArmorPenetrationPercent;
            playerState.ChampionStats.AttackDamage = gameData.activePlayer.championStats.AttackDamage;
            playerState.ChampionStats.AttackRange = gameData.activePlayer.championStats.AttackRange;
            playerState.ChampionStats.AttackSpeed = gameData.activePlayer.championStats.AttackSpeed;
            playerState.ChampionStats.BonusArmorPenetrationPercent = gameData.activePlayer.championStats.BonusArmorPenetrationPercent;
            playerState.ChampionStats.BonusMagicPenetrationPercent = gameData.activePlayer.championStats.BonusMagicPenetrationPercent;
            playerState.ChampionStats.CooldownReduction = gameData.activePlayer.championStats.CooldownReduction;
            playerState.ChampionStats.CritChance = gameData.activePlayer.championStats.CritChance;
            playerState.ChampionStats.CritDamagePercent = gameData.activePlayer.championStats.CritDamage;
            playerState.ChampionStats.HealthCurrent = gameData.activePlayer.championStats.CurrentHealth;
            playerState.ChampionStats.HealthRegenRate = gameData.activePlayer.championStats.HealthRegenRate;
            playerState.ChampionStats.LifeSteal = gameData.activePlayer.championStats.LifeSteal;
            playerState.ChampionStats.MagicLethality = gameData.activePlayer.championStats.MagicLethality;
            playerState.ChampionStats.MagicPenetrationFlat = gameData.activePlayer.championStats.MagicPenetrationFlat;
            playerState.ChampionStats.MagicPenetrationPercent = gameData.activePlayer.championStats.MagicPenetrationPercent;
            playerState.ChampionStats.MagicResist = gameData.activePlayer.championStats.MagicResist;
            playerState.ChampionStats.HealthMax = gameData.activePlayer.championStats.MaxHealth;
            playerState.ChampionStats.MoveSpeed = gameData.activePlayer.championStats.MoveSpeed;
            playerState.ChampionStats.PhysicalLethality = gameData.activePlayer.championStats.PhysicalLethality;
            playerState.ChampionStats.ResourceMax = gameData.activePlayer.championStats.ResourceMax;
            playerState.ChampionStats.ResourceRegenRate = gameData.activePlayer.championStats.ResourceRegenRate;
            playerState.ChampionStats.ResourceType = gameData.activePlayer.championStats.ResourceType;
            playerState.ChampionStats.ResourceCurrent = gameData.activePlayer.championStats.ResourceValue;
            playerState.ChampionStats.SpellVamp = gameData.activePlayer.championStats.SpellVamp;
            playerState.ChampionStats.Tenacity = gameData.activePlayer.championStats.Tenacity;
            playerState.Gold = gameData.activePlayer.CurrentGold;
            playerState.Level = gameData.activePlayer.Level;
            playerState.SummonerName = gameData.activePlayer.SummonerName;
            #endregion

            #region player stats
            playerState.Assists = player.scores.assists;
            playerState.Kills = player.scores.kills;
            playerState.Deaths = player.scores.deaths;
            playerState.WardScore = player.scores.wardScore;
            playerState.CreepScore = player.scores.creepScore;
            playerState.RespawnTimer = player.respawnTimer;
            playerState.IsDead = player.isDead;
            #endregion

            #region Enum parsing
            if (Enum.TryParse<Champion>(player.championName.Replace(" ", "").Replace("'", "").Replace(".", ""), true, out var c))
                playerState.Champion = c;

            if (Enum.TryParse<Team>(player.team, true, out var t))
                playerState.Team = t;

            if (Enum.TryParse<SummonerSpell>(player.summonerSpells.summonerSpellOne.displayname, out var ss1))
                playerState.SpellD.Spell = ss1;

            if (Enum.TryParse<SummonerSpell>(player.summonerSpells.summonerSpellTwo.displayname, out var ss2))
                playerState.SpellF.Spell = ss2;
            #endregion

        }

        public override void ResetGameState()
        {
            _game_state = new GameState_LoL();
        }

        public GameEvent_LoL() : base()
        {
            //ignore ssl errors
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }

        #region Structs
        private struct GameData
        {
            public ActivePlayer activePlayer;
            public Player[] allplayers;
            public LolEvents events;
            public Match gameData;
        }
        private struct Player
        {
            public string championName;
            public bool isBot;
            public bool isDead;
            public Item[] items;

            public int level;

            public float respawnTimer;

            public Stats scores;

            public string summonerName;
            public Spells summonerSpells;

            public string team;
        }
        private struct Stats
        {
            public int assists;
            public int creepScore;
            public int deaths;
            public int kills;
            public float wardScore;
        }
        private struct Spells
        {
            public Spell summonerSpellOne;
            public Spell summonerSpellTwo;
        }
        private struct Spell
        {
            public string displayname;
        }
        private struct Item
        {
            public bool canUse;
            public bool consumable;
            public string displayName;
            public int slot;
        }
        private struct ActivePlayer
        {
            public Abilities abilities;
            public ChampionStats championStats;
            public float CurrentGold;
            public int Level;
            public string SummonerName;
        }
        private struct ChampionStats
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
            public GSI.Nodes.ResourceType ResourceType;
            public float ResourceValue;
            public float SpellVamp;
            public float Tenacity;
        }
        private struct Ability
        {
            public int abilityLevel;
            public string displayName;
            public string id;
        }
        private struct Abilities
        {
            public Ability passive;
            public Ability q;
            public Ability w;
            public Ability e;
            public Ability r;
        }
        private struct LolEvent
        {
            public int eventID;
            public string eventname;
            public float eventtime;
        }
        private struct LolEvents
        {
            public LolEvent[] events;
        }
        private struct Match
        {
            public string gamemode;
            public float gameTime;
        }
        #endregion
    }
}