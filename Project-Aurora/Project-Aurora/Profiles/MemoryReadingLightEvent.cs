using Aurora.EffectsEngine;
using Aurora.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Aurora.Profiles {

    public abstract class MemoryReadingLightEvent<TPointers, TGameState> : LightEvent where TGameState : IGameState, new() {

        private readonly string pointerFilename;
        private readonly string processName;
        private readonly string processModule;
        private readonly bool needs64bitMemReader;
        protected bool isInitialized = false; 
        protected TPointers pointers;

        public MemoryReadingLightEvent(string pointerFilename, string processName, string processModule = "", bool needs64bitMemReader = false) : base() {
            // Field initialisation
            this.pointerFilename = pointerFilename;
            this.processName = processName;
            this.processModule = processModule;
            this.needs64bitMemReader = needs64bitMemReader;

            // Create file system watcher to watch for changes in the pointer directory
            FileSystemWatcher watcher = new FileSystemWatcher {
                Path = Path.Combine(Global.ExecutingDirectory, "Pointers"),
                EnableRaisingEvents = true
            };
            watcher.Changed += (sender, e) => {
                // If the file changed was the one we are looking for (e.g. "RocketLeague.json"), then reload the pointers
                if (e.Name.Equals(pointerFilename) && e.ChangeType == WatcherChangeTypes.Changed)
                    ReloadPointers();
            };

            // Perform an initial pointer load
            ReloadPointers();
        }

        private void ReloadPointers() {
            string path = Path.Combine(Global.ExecutingDirectory, "Pointers", pointerFilename);

            if (File.Exists(path)) {
                try {
                    // Deserialize JSON directly from a file
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, System.Text.Encoding.Default)) {
                        JsonSerializer serializer = new JsonSerializer();
                        pointers = (TPointers)serializer.Deserialize(sr, typeof(TPointers));
                    }
                } catch (Exception exc) {
                    Global.logger.Error(exc.Message);
                    isInitialized = false;
                }

                isInitialized = true;
            } else {
                isInitialized = false;
            }
        }

        public override void ResetGameState() {
            _game_state = new TGameState();
        }

        public new bool IsEnabled => Application.Settings.IsEnabled && isInitialized;

        public override void UpdateLights(EffectFrame frame) {

            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            Process[] process_search = Process.GetProcessesByName(processName);

            if (process_search.Length != 0)
                using (MemoryReader memread = string.IsNullOrWhiteSpace(processModule) ? new MemoryReader(process_search[0], needs64bitMemReader) : new MemoryReader(process_search[0], processModule, needs64bitMemReader))
                    UpdateGameState((TGameState)_game_state, memread);

            foreach (var layer in Application.Profile.Layers.Reverse())
                if (layer.Enabled && layer.LogicPass)
                    layers.Enqueue(layer.Render(_game_state));

            //Scripts
            Application.UpdateEffectScripts(layers);

            frame.AddLayers(layers.ToArray());
        }

        public override void SetGameState(IGameState new_game_state) { }

        public abstract void UpdateGameState(TGameState gameState, MemoryReader reader);
    }
}
