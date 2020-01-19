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
                var res = client.GetAsync(URI).Result;
                if (res.StatusCode == HttpStatusCode.OK)
                    jsonData = res.Content.ReadAsStringAsync().Result;
            }
            catch(Exception e)
            {
                Global.logger.Info(e);
                return;
            }

            if (string.IsNullOrWhiteSpace(jsonData) || jsonData.Contains("error"))
                return;

            gameData = JsonConvert.DeserializeObject<data>(jsonData);
            var player = Array.Find(gameData.allplayers, p => p.summonerName == gameData.activePlayer.SummonerName);

            //fill in data here
            //(_game_state as GameState_LoL).Player.ChampionStats = gameData.activePlayer.championStats;
            (_game_state as GameState_LoL).Player.CurrentGold = gameData.activePlayer.CurrentGold;
            (_game_state as GameState_LoL).Player.Level = gameData.activePlayer.Level;
            (_game_state as GameState_LoL).Player.SummonerName = gameData.activePlayer.SummonerName;
            (_game_state as GameState_LoL).Player.IsDead = player.isDead;
           // (_game_state as GameState_LoL).Player.IsDead = player.championName;
        }

        public override void ResetGameState()
        {
            _game_state = new GameState_LoL();
        }

        public GameEvent_LoL() : base()
        {
            //ignore ssl errors
            client.Timeout = TimeSpan.FromSeconds(10);
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }
    }

    public struct data
    {
        public activeplayer activePlayer;
        public playersdata[] allplayers;
        public lolevents events;
        public gamedata gameData;
    }
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
    public struct playersdata
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
    public struct stats
    {
        public int assists;
        public int creepScore;
        public int deaths;
        public int kills;
        public float wardScore;
    }
    public struct spells
    {
		public spell summonerSpellOne;
		public spell summonerSpellTwo;
    }
    public struct spell
    {
        public string displayname;
    }
    public struct item
    {
        public bool canUse;
        public bool consumable;
        public string displayName;
        public int slot;
    }
    public struct activeplayer
    {
        public abilities abilities;
        public championStats championStats;
        public float CurrentGold;
        public int Level;
        public string SummonerName;
    }
    public struct championStats
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
    public struct ability
    {
        public int abilityLevel;
        public string displayName;
        public string id;
    }
    public struct abilities
    {
        public ability passive;
        public ability q;
        public ability w;
        public ability e;
        public ability r;
    }
    public struct lolevent {
        public int eventID;
        public string eventname;
        public float eventtime;
    }
    public struct lolevents
    {
        public lolevent[] events;
    }
    public struct gamedata
    {
        public string gamemode;
        public float gameTime;
    }
}
