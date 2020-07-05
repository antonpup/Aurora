using Aurora.Profiles;
using Aurora.Profiles.LeagueOfLegends.GSI;
using Aurora.Profiles.LeagueOfLegends.GSI.Nodes;
using Aurora.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Aurora.Profiles.LeagueOfLegends
{
    public class GameEvent_LoL : LightEvent
    {
        private const string URI = "https://127.0.0.1:2999/liveclientdata/allgamedata";

        private readonly HttpClient client = new HttpClient();
        private readonly Timer updateTimer;

        private _RootGameData allGameData;
        private bool updatedOnce = false;

        public GameEvent_LoL() : base()
        {
            //ignore ssl errors
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            updateTimer = new Timer(100);
            updateTimer.Elapsed += UpdateData;
        }

        public override void ResetGameState()
        {
            _game_state = new GameState_LoL();
        }

        public override void OnStart() => updateTimer.Start();

        public override void OnStop()
        {
            updateTimer.Stop();
            client.CancelPendingRequests();
        }

        public override void UpdateTick()
        {
            if (!updatedOnce)
                return;

            var s = _game_state as GameState_LoL;

            if (allGameData == null)
            {
                _game_state = new GameState_LoL();
                return;
            }

            try
            {
                #region Match
                s.Match.InGame = true;
                //s.Match.GameMode = EnumUtils.TryParseOr(allGameData.gameData.gameMode, true, GameMode.Unknown);
                s.Match.GameMode = allGameData.gameData.gameMode;
                s.Match.GameTime = allGameData.gameData.gameTime;
                #endregion

                #region Player

                var ap = allGameData.activePlayer;
                s.Player.Level = ap.level;
                s.Player.Gold = ap.currentGold;
                s.Player.SummonerName = ap.summonerName;

                #region Abilities
                s.Player.Abilities.Q.Level = ap.abilities.Q.abilityLevel;
                s.Player.Abilities.Q.Name = ap.abilities.Q.displayName;
                s.Player.Abilities.W.Level = ap.abilities.W.abilityLevel;
                s.Player.Abilities.W.Name = ap.abilities.W.displayName;
                s.Player.Abilities.E.Level = ap.abilities.E.abilityLevel;
                s.Player.Abilities.E.Name = ap.abilities.E.displayName;
                s.Player.Abilities.R.Level = ap.abilities.R.abilityLevel;
                s.Player.Abilities.R.Name = ap.abilities.R.displayName;
                #endregion

                #region Stats
                s.Player.ChampionStats.AbilityPower = ap.championStats.abilityPower;
                s.Player.ChampionStats.Armor = ap.championStats.armor;
                s.Player.ChampionStats.ArmorPenetrationFlat = ap.championStats.armorPenetrationFlat;
                s.Player.ChampionStats.ArmorPenetrationPercent = ap.championStats.armorPenetrationPercent;
                s.Player.ChampionStats.AttackDamage = ap.championStats.attackDamage;
                s.Player.ChampionStats.AttackRange = ap.championStats.attackRange;
                s.Player.ChampionStats.AttackSpeed = ap.championStats.attackSpeed;
                s.Player.ChampionStats.BonusArmorPenetrationPercent = ap.championStats.bonusArmorPenetrationPercent;
                s.Player.ChampionStats.BonusMagicPenetrationPercent = ap.championStats.bonusMagicPenetrationPercent;
                s.Player.ChampionStats.CooldownReduction = ap.championStats.cooldownReduction;
                s.Player.ChampionStats.CritChance = ap.championStats.critChance;
                s.Player.ChampionStats.CritDamagePercent = ap.championStats.critDamage;
                s.Player.ChampionStats.HealthCurrent = ap.championStats.currentHealth;
                s.Player.ChampionStats.HealthRegenRate = ap.championStats.healthRegenRate;
                s.Player.ChampionStats.LifeSteal = ap.championStats.lifeSteal;
                s.Player.ChampionStats.MagicLethality = ap.championStats.magicLethality;
                s.Player.ChampionStats.MagicPenetrationFlat = ap.championStats.magicPenetrationFlat;
                s.Player.ChampionStats.MagicPenetrationPercent = ap.championStats.magicPenetrationPercent;
                s.Player.ChampionStats.MagicResist = ap.championStats.magicResist;
                s.Player.ChampionStats.HealthMax = ap.championStats.maxHealth;
                s.Player.ChampionStats.MoveSpeed = ap.championStats.moveSpeed;
                s.Player.ChampionStats.PhysicalLethality = ap.championStats.physicalLethality;
                s.Player.ChampionStats.ResourceMax = ap.championStats.resourceMax;
                s.Player.ChampionStats.ResourceRegenRate = ap.championStats.resourceRegenRate;
                s.Player.ChampionStats.ResourceType = EnumUtils.TryParseOr(ap.championStats.resourceType, true, ResourceType.Unknown);
                s.Player.ChampionStats.ResourceCurrent = ap.championStats.resourceValue;
                s.Player.ChampionStats.SpellVamp = ap.championStats.spellVamp;
                s.Player.ChampionStats.Tenacity = ap.championStats.tenacity;
                #endregion

                #region Runes
                //TODO
                #endregion

                #region allPlayer data
                //there's some data in allPlayers about the user that is not contained in activePlayer...
                var p = allGameData.allPlayers.FirstOrDefault(a => a.summonerName == ap.summonerName);
                if (p == null)
                    return;
                //if we can't find it, skip

                s.Player.Champion = EnumUtils.TryParseOr(p.championName.Replace(" ", "").Replace("'", "").Replace(".", ""), true, Champion.None);
                s.Player.SpellD = EnumUtils.TryParseOr(p.summonerSpells.summonerSpellOne.displayName, true, SummonerSpell.Unknown);
                s.Player.SpellF = EnumUtils.TryParseOr(p.summonerSpells.summonerSpellTwo.displayName, true, SummonerSpell.Unknown);
                s.Player.Team = EnumUtils.TryParseOr(p.team, true, Team.Unknown);
                s.Player.Position = EnumUtils.TryParseOr(p.position, true, Position.Unknown);

                s.Player.IsDead = p.isDead;
                s.Player.RespawnTimer = p.respawnTimer;
                s.Player.Kills = p.scores.kills;
                s.Player.Deaths = p.scores.deaths;
                s.Player.Assists = p.scores.assists;
                s.Player.CreepScore = p.scores.creepScore;
                s.Player.WardScore = p.scores.wardScore;
                #endregion

                #region Events
                var drags = allGameData.events.Events.OfType<_DragonKillEvent>();

                s.Match.InfernalDragonsKilled = drags.Count(d => d.DragonType.ToLower() == "fire");
                s.Match.EarthDragonsKilled = drags.Count(d => d.DragonType.ToLower() == "earth");
                s.Match.OceanDragonsKilled = drags.Count(d => d.DragonType.ToLower() == "water");
                s.Match.CloudDragonsKilled = drags.Count(d => d.DragonType.ToLower() == "air");
                s.Match.ElderDragonsKilled = drags.Count(d => d.DragonType.ToLower() == "elder");

                 s.Match.BaronsKilled = allGameData.events.Events.Count(ev => ev is _BaronKillEvent);
                s.Match.HeraldsKilled = allGameData.events.Events.Count(ev => ev is _HeraldKillEvent);
                s.Match.DragonsKilled = allGameData.events.Events.Count(ev => ev is _DragonKillEvent);
                s.Match.TurretsKilled = allGameData.events.Events.Count(ev => ev is _TurretKillEvent);
                s.Match.InhibsKilled = allGameData.events.Events.Count(ev => ev is _InhibKillEvent);
                s.Match.MapTerrain = EnumUtils.TryParseOr(allGameData.gameData.mapTerrain, true, MapTerrain.Unknown);
                #endregion

                #region Items
                s.Player.Items.Slot1 = GetItem(p, 0);
                s.Player.Items.Slot2 = GetItem(p, 1);
                s.Player.Items.Slot3 = GetItem(p, 2);
                s.Player.Items.Slot4 = GetItem(p, 3);
                s.Player.Items.Slot5 = GetItem(p, 4);
                s.Player.Items.Slot6 = GetItem(p, 5);
                s.Player.Items.Trinket = GetItem(p, 6);
                #endregion

                #endregion
            }
            catch (Exception e)
            {
                //Global.logger.Error(e);
            }
        }

        /// <summary>
        /// Tries to find an item from the nth <paramref name="slot"/>. Returns the item if it finds it, or an empty item otherwise.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        private static SlotNode GetItem(_AllPlayer p, int slot)
        {
            var newItem = p.items.FirstOrDefault(item => item.slot == slot);

            if (newItem == null)
                return new SlotNode();
            else
                return new SlotNode(newItem);
        }

        private async void UpdateData(object sender, ElapsedEventArgs e)
        {
            if (!Global.LightingStateManager.RunningProcessMonitor.IsProcessRunning("league of legends.exe"))
            {
                allGameData = null;
                return;
            }


            string jsonData = "";
            try
            {
                using (var res = await client.GetAsync(URI))
                {
                    if (res.IsSuccessStatusCode)
                        jsonData = await res.Content.ReadAsStringAsync();
                }
            }
            catch (Exception exc)
            {
                // Global.logger.Error(exc);
                allGameData = null;
                return;
            }

            if (string.IsNullOrWhiteSpace(jsonData) || jsonData.Contains("error"))
            {
                allGameData = null;
                return;
            }

            allGameData = JsonConvert.DeserializeObject<_RootGameData>(jsonData);
            updatedOnce = true;
        }
    }
}