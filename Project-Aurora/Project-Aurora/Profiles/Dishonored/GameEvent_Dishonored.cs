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
using Aurora.Profiles.Dishonored.GSI;
using Aurora.Profiles.Dishonored.GSI.Nodes;

namespace Aurora.Profiles.Dishonored
{
    public class GameEvent_Dishonored : LightEvent
    {
        private bool isInitialized = false;

        //Pointers
        private DishonoredPointers pointers;

        public GameEvent_Dishonored() : base()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = System.IO.Path.Combine(Global.ExecutingDirectory, "Pointers");
            watcher.Changed += DHPointers_Changed;
            watcher.EnableRaisingEvents = true;

            ReloadPointers();
        }

        private void DHPointers_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name.Equals("Dishonored.json") && e.ChangeType == WatcherChangeTypes.Changed)
                ReloadPointers();
        }

        private void ReloadPointers()
        {
            string path = System.IO.Path.Combine(Global.ExecutingDirectory, "Pointers", "Dishonored.json");
            
            if (File.Exists(path))
            {
                try
                {
                    // deserialize JSON directly from a file
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, System.Text.Encoding.Default))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        pointers = (DishonoredPointers)serializer.Deserialize(sr, typeof(DishonoredPointers));
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
            _game_state = new GameState_Dishonored();
        }

        public new bool IsEnabled
        {
            get { return this.Application.Settings.IsEnabled && isInitialized; }
        }

        public override void UpdateLights(EffectFrame frame)
        {

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            DishonoredProfile settings = (DishonoredProfile)this.Application.Profile;

            Process[] process_search = Process.GetProcessesByName("Dishonored");

            if (process_search.Length != 0)
            {
                using (MemoryReader memread = new MemoryReader(process_search[0]))
                {
                    (_game_state as GameState_Dishonored).Player.MaximumHealth = memread.ReadInt(pointers.MaximumHealth.baseAddress, pointers.MaximumHealth.pointers);
                    (_game_state as GameState_Dishonored).Player.CurrentHealth = memread.ReadInt(pointers.CurrentHealth.baseAddress, pointers.CurrentHealth.pointers);
                    (_game_state as GameState_Dishonored).Player.MaximumMana = memread.ReadInt(pointers.MaximumMana.baseAddress, pointers.MaximumMana.pointers);
                    (_game_state as GameState_Dishonored).Player.CurrentMana = memread.ReadInt(pointers.CurrentMana.baseAddress, pointers.CurrentMana.pointers);
                    (_game_state as GameState_Dishonored).Player.ManaPots = memread.ReadInt(pointers.ManaPots.baseAddress, pointers.ManaPots.pointers);
                    (_game_state as GameState_Dishonored).Player.HealthPots = memread.ReadInt(pointers.HealthPots.baseAddress, pointers.HealthPots.pointers);
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
