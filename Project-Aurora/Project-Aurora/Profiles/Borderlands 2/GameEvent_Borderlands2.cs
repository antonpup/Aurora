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
using Aurora.Profiles.Borderlands2.GSI;
using Aurora.Profiles.Borderlands2.GSI.Nodes;

namespace Aurora.Profiles.Borderlands2
{
    public class GameEvent_Borderlands2 : LightEvent
    {
        private bool isInitialized = false;

        //Pointers
        private Borderlands2Pointers pointers;

        public GameEvent_Borderlands2() : base()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = System.IO.Path.Combine(Global.ExecutingDirectory, "Pointers");
            watcher.Changed += RLPointers_Changed;
            watcher.EnableRaisingEvents = true;

            ReloadPointers();
        }

        private void RLPointers_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name.Equals("Borderlands2.json") && e.ChangeType == WatcherChangeTypes.Changed)
                ReloadPointers();
        }

        private void ReloadPointers()
        {
            string path = System.IO.Path.Combine(Global.ExecutingDirectory, "Pointers", "Borderlands2.json");
            
            if (File.Exists(path))
            {
                try
                {
                    // deserialize JSON directly from a file
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, System.Text.Encoding.Default))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        pointers = (Borderlands2Pointers)serializer.Deserialize(sr, typeof(Borderlands2Pointers));
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
            _game_state = new GameState_Borderlands2();
        }

        public new bool IsEnabled
        {
            get { return this.Application.Settings.IsEnabled && isInitialized; }
        }

        public override void UpdateLights(EffectFrame frame)
        {

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            Borderlands2Profile settings = (Borderlands2Profile)this.Application.Profile;

            Process[] process_search = Process.GetProcessesByName("Borderlands2");

            if (process_search.Length != 0)
            {
                using (MemoryReader memread = new MemoryReader(process_search[0]))
                {
                    (_game_state as GameState_Borderlands2).Player.MaximumHealth = memread.ReadFloat(pointers.Health_maximum.baseAddress, pointers.Health_maximum.pointers);
                    (_game_state as GameState_Borderlands2).Player.CurrentHealth = memread.ReadFloat(pointers.Health_current.baseAddress, pointers.Health_current.pointers);
                    (_game_state as GameState_Borderlands2).Player.MaximumShield = memread.ReadFloat(pointers.Shield_maximum.baseAddress, pointers.Shield_maximum.pointers);
                    (_game_state as GameState_Borderlands2).Player.CurrentShield = memread.ReadFloat(pointers.Shield_current.baseAddress, pointers.Shield_current.pointers);
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
