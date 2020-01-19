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
        private data gameData;

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
            catch (Exception e)
            {
                Global.logger.Info(e);
                return;
            }

            if (string.IsNullOrWhiteSpace(jsonData) || jsonData.Contains("error"))
                return;

            gameData = JsonConvert.DeserializeObject<data>(jsonData);
            var player = Array.Find(gameData.allplayers, p => p.summonerName == gameData.activePlayer.SummonerName);
            var playerState = (_game_state as GameState_LoL).Player;

            //fill in data here
            #region ugly stats setting
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
            #endregion

            playerState.Gold = gameData.activePlayer.CurrentGold;
            playerState.Level = gameData.activePlayer.Level;
            playerState.SummonerName = gameData.activePlayer.SummonerName;
            playerState.IsDead = player.isDead;

            if (Enum.TryParse<Champion>(player.championName.Replace(" ","").Replace("'","").Replace(".", ""), true, out var c))
                playerState.Champion = c;

            if (Enum.TryParse<Team>(player.team, true, out var t))
                playerState.Team = t;

            if (Enum.TryParse<SummonerSpell>(player.summonerSpells.summonerSpellOne.displayname, out var ss1))
                playerState.SpellD = ss1;

            if (Enum.TryParse<SummonerSpell>(player.summonerSpells.summonerSpellTwo.displayname, out var ss2))
                playerState.SpellF = ss2;

            playerState.Assists = player.scores.assists;
            playerState.Kills = player.scores.kills;
            playerState.Deaths = player.scores.deaths;
            playerState.WardScore = player.scores.wardScore;
            playerState.CreepScore = player.scores.creepScore;
            playerState.RespawnTimer = player.respawnTimer;
            
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

        private struct data
        {
            public activeplayer activePlayer;
            public playersdata[] allplayers;
            public lolevents events;
            public gamedata gameData;
        }
        private struct playersdata
        {
            public string championName;
            public bool isBot;
            public bool isDead;
            public item[] items;

            public int level;

            public float respawnTimer;

            public stats scores;

            public string summonerName;
            public spells summonerSpells;

            public string team;
        }
        private struct stats
        {
            public int assists;
            public int creepScore;
            public int deaths;
            public int kills;
            public float wardScore;
        }
        private struct spells
        {
            public spell summonerSpellOne;
            public spell summonerSpellTwo;
        }
        private struct spell
        {
            public string displayname;
        }
        private struct item
        {
            public bool canUse;
            public bool consumable;
            public string displayName;
            public int slot;
        }
        private struct activeplayer
        {
            public abilities abilities;
            public championStats championStats;
            public float CurrentGold;
            public int Level;
            public string SummonerName;
        }
        private struct championStats
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
        private struct ability
        {
            public int abilityLevel;
            public string displayName;
            public string id;
        }
        private struct abilities
        {
            public ability passive;
            public ability q;
            public ability w;
            public ability e;
            public ability r;
        }
        private struct lolevent
        {
            public int eventID;
            public string eventname;
            public float eventtime;
        }
        private struct lolevents
        {
            public lolevent[] events;
        }
        private struct gamedata
        {
            public string gamemode;
            public float gameTime;
        }
    }
}
