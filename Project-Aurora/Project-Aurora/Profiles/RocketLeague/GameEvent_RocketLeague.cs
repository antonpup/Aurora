using System;
using Aurora.EffectsEngine;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using Aurora.Utils;
using System.Drawing.Drawing2D;
using Aurora.Settings;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using Aurora.Profiles.RocketLeague.GSI;
using Aurora.Profiles.RocketLeague.GSI.Nodes;

namespace Aurora.Profiles.RocketLeague
{
    public class GameEvent_RocketLeague : LightEvent
    {
        private bool isInitialized = false;

        //Pointers
        private RocketLeaguePointers pointers;

        public GameEvent_RocketLeague() : base()
        {

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = System.IO.Path.Combine(Global.ExecutingDirectory, "Pointers");
            watcher.Changed += RLPointers_Changed;
            watcher.EnableRaisingEvents = true;

            ReloadPointers();
        }

        private void RLPointers_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name.Equals("RocketLeague.json") && e.ChangeType == WatcherChangeTypes.Changed)
                ReloadPointers();
        }

        private void ReloadPointers()
        {
            string path = System.IO.Path.Combine(Global.ExecutingDirectory, "Pointers", "RocketLeague.json");

            if (File.Exists(path))
            {
                try
                {
                    // deserialize JSON directly from a file
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, System.Text.Encoding.Default))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        pointers = (RocketLeaguePointers)serializer.Deserialize(sr, typeof(RocketLeaguePointers));
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.Error(exc.Message);
                    isInitialized = false;
                }

                isInitialized = true;
            }
            else
            {
                isInitialized = false;
            }
        }

        public override void ResetGameState()
        {
            _game_state = new GameState_RocketLeague();
        }

        public new bool IsEnabled
        {
            get { return this.Application.Settings.IsEnabled && isInitialized; }
        }

        public override void UpdateLights(EffectFrame frame)
        {

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            RocketLeagueProfile settings = (RocketLeagueProfile)this.Application.Profile;

            Process[] process_search = Process.GetProcessesByName("RocketLeague");

            if (process_search.Length != 0)
            {
                using (MemoryReader memread = new MemoryReader(process_search[0]))
                {
                    PlayerTeam parsed_team = PlayerTeam.Undefined;
                    if(Enum.TryParse<PlayerTeam>(memread.ReadInt(pointers.Team.baseAddress, pointers.Team.pointers).ToString(), out parsed_team))
                        (_game_state as GameState_RocketLeague).Player.Team = parsed_team;


                    // Goal explosion preperation
                    ( _game_state as GameState_RocketLeague ).Match.YourTeam_LastScore = parsed_team == PlayerTeam.Blue ? ( _game_state as GameState_RocketLeague ).Match.BlueTeam_Score 
                                                                                                          : ( _game_state as GameState_RocketLeague ).Match.OrangeTeam_Score;
                    (_game_state as GameState_RocketLeague).Match.EnemyTeam_LastScore = parsed_team == PlayerTeam.Orange ? (_game_state as GameState_RocketLeague).Match.BlueTeam_Score
                                                                                                                              : (_game_state as GameState_RocketLeague).Match.OrangeTeam_Score;
                    
                    ( _game_state as GameState_RocketLeague).Match.OrangeTeam_Score = memread.ReadInt(pointers.Orange_score.baseAddress, pointers.Orange_score.pointers);
                    (_game_state as GameState_RocketLeague).Match.BlueTeam_Score = memread.ReadInt(pointers.Blue_score.baseAddress, pointers.Blue_score.pointers);
                    (_game_state as GameState_RocketLeague).Player.BoostAmount = memread.ReadInt(pointers.Boost_amount.baseAddress, pointers.Boost_amount.pointers);
                }
            }

            foreach (var layer in this.Application.Profile.Layers.Reverse().ToArray())
            {
                if (layer.Enabled && layer.LogicPass)
                    layers.Enqueue(layer.Render(_game_state));
            }

            //Scripts
            this.Application.UpdateEffectScripts(layers);

            frame.AddLayers(layers.ToArray());
        }

        public override void SetGameState(IGameState new_game_state)
        {
            //UpdateLights(frame);
        }
    }
}
