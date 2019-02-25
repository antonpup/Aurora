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
using Aurora.Profiles.ResidentEvil2.GSI;
using Aurora.Profiles.ResidentEvil2.GSI.Nodes;

namespace Aurora.Profiles.ResidentEvil2
{
    public class GameEvent_ResidentEvil2 : LightEvent
    {
        private bool isInitialized = false;

        //Pointers
        private ResidentEvil2Pointers pointers;

        public GameEvent_ResidentEvil2() : base()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = System.IO.Path.Combine(Global.ExecutingDirectory, "Pointers");
            watcher.Changed += RLPointers_Changed;
            watcher.EnableRaisingEvents = true;

            ReloadPointers();
        }

        private void RLPointers_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name.Equals("ResidentEvil2.json") && e.ChangeType == WatcherChangeTypes.Changed)
                ReloadPointers();
        }

        private void ReloadPointers()
        {
            string path = System.IO.Path.Combine(Global.ExecutingDirectory, "Pointers", "ResidentEvil2.json");
            
            if (File.Exists(path))
            {
                try
                {
                    // deserialize JSON directly from a file
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, System.Text.Encoding.Default))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        pointers = (ResidentEvil2Pointers)serializer.Deserialize(sr, typeof(ResidentEvil2Pointers));
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
            _game_state = new GameState_ResidentEvil2();
        }

        public new bool IsEnabled
        {
            get { return this.Application.Settings.IsEnabled && isInitialized; }
        }

        public override void UpdateLights(EffectFrame frame)
        {

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            ResidentEvil2Profile settings = (ResidentEvil2Profile)this.Application.Profile;

            Process[] process_search = Process.GetProcessesByName("re2");

            if (process_search.Length != 0)
            {
                using (MemoryReader memread = new MemoryReader(process_search[0],false))
                {
                    (_game_state as GameState_ResidentEvil2).Player.MaximumHealth = memread.ReadInt(pointers.HealthMaximum.baseAddress, pointers.HealthMaximum.pointers);
                    (_game_state as GameState_ResidentEvil2).Player.CurrentHealth = memread.ReadInt(pointers.HealthCurrent.baseAddress, pointers.HealthCurrent.pointers);
                    (_game_state as GameState_ResidentEvil2).Player.Poison = memread.ReadInt(pointers.PlayerPoisoned.baseAddress, pointers.PlayerPoisoned.pointers) == 1;
                    (_game_state as GameState_ResidentEvil2).Player.Rank = memread.ReadInt(pointers.RankCurrent.baseAddress, pointers.RankCurrent.pointers);

                    int maxHealth = (_game_state as GameState_ResidentEvil2).Player.MaximumHealth;
                    int currentHealth = (_game_state as GameState_ResidentEvil2).Player.CurrentHealth;

                    if (maxHealth == 0) // not in-game
                    {
                        (_game_state as GameState_ResidentEvil2).Player.Status = Player_ResidentEvil2.PlayerStatus.OffGame;
                        (_game_state as GameState_ResidentEvil2).Player.Rank = 0;
                    }
                    else if (currentHealth == 1200) (_game_state as GameState_ResidentEvil2).Player.Status = Player_ResidentEvil2.PlayerStatus.Fine;
                    else if (currentHealth > 800) (_game_state as GameState_ResidentEvil2).Player.Status = Player_ResidentEvil2.PlayerStatus.LiteFine;
                    else if (currentHealth > 360) (_game_state as GameState_ResidentEvil2).Player.Status = Player_ResidentEvil2.PlayerStatus.Caution;
                    else if (currentHealth > 0) (_game_state as GameState_ResidentEvil2).Player.Status = Player_ResidentEvil2.PlayerStatus.Danger;
                    else (_game_state as GameState_ResidentEvil2).Player.Status = Player_ResidentEvil2.PlayerStatus.Dead;

                    if ((_game_state as GameState_ResidentEvil2).Player.Status == Player_ResidentEvil2.PlayerStatus.Dead)
                    {
                        (_game_state as GameState_ResidentEvil2).Player.Poison = false;
                        (_game_state as GameState_ResidentEvil2).Player.Rank = 0;
                    }
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
