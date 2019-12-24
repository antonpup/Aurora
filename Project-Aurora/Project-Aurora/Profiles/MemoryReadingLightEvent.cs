using Aurora.EffectsEngine;
using Aurora.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Aurora.Profiles {

    /// <summary>
    /// A base LightEvent (GameEvent) that can be used as a basis for applications that make use of the Aurora.Utils.MemoryReader.
    /// <para>Important: The constructor of any class that extends this one, should call `base` on the constructor.</para>
    /// <para>Supports the 64 bit memory reader and will reload pointer JSON files if they are changed.</para>
    /// </summary>
    /// <typeparam name="TPointers">The type of class that holds all the pointer datas for this game event.</typeparam>
    /// <typeparam name="TGameState">The IGameState type that holds the game state data for this game event. Must have parameterless constructor.</typeparam>
    public abstract class MemoryReadingLightEvent<TPointers, TGameState> : LightEvent where TGameState : IGameState, new() {

        private readonly string pointerFilename;
        private readonly string processName;
        private readonly string processModule;
        private readonly bool needs64bitMemReader;
        protected bool isInitialized = false;

        /// <summary>Main pointer data (parsed from the JSON file).</summary>
        protected TPointers pointers;

        /// <summary>
        /// Constructs a new MemoryReadingLightEvent and sets up the FileSystemWatcher on the target JSON file.
        /// </summary>
        /// <param name="pointerFilename">The name of the JSON file in the pointers directory that should be used as a pointer data source file.</param>
        /// <param name="processName">The target name of the process. Note this is the process NAME, not the exe file.</param>
        /// <param name="processModule">Optional name of a module to target with the memory reader. If not given, the default mainmodule is used.</param>
        /// <param name="needs64bitMemReader">Flag indicating whether or not the MemoryReader should be in 64 bit mode. Defaults to false.</param>
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

        /// <summary>
        /// Loads the pointer data from the target JSON file in the Pointers directory.
        /// </summary>
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

        /// <summary>
        /// Resets the game state to be the default value.
        /// </summary>
        public override void ResetGameState() {
            _game_state = new TGameState();
        }

        /// <summary>
        /// Gets whether or not the application is enabled and if the memory reader has been initialised correctly.
        /// </summary>
        public new bool IsEnabled => Application.Settings.IsEnabled && isInitialized;

        /// <summary>
        /// Updates all the gamestate for this frame. Will search for the given process name and attempt to attach the memory reader.
        /// </summary>
        public override void UpdateTick() {
            // Look for the target process
            Process[] process_search = Process.GetProcessesByName(processName);

            // If it was found, attempt to attach the MemoryReader
            if (process_search.Length != 0)
                // Call a different constructor depending on whether a `processModule` has been provided.
                using (MemoryReader memread = string.IsNullOrWhiteSpace(processModule) ? new MemoryReader(process_search[0], needs64bitMemReader) : new MemoryReader(process_search[0], processModule, needs64bitMemReader))
                    // Call the application-specific method to handle the memory reading
                    UpdateGameState((TGameState)_game_state, memread);
        }

        public override void SetGameState(IGameState new_game_state) { }

        /// <summary>
        /// This method should be overriden to provide application-specific functionality to the LightEvent.
        /// <para>This will be called if the process has been found and after a MemoryReader has been attached to the
        /// process. The MemoryReader is passed to this method.</para>
        /// </summary>
        /// <param name="gameState">The game state of the application. Can also use `_game_state` field, but this one is
        /// cast to the correct type before being passed, saving you from needing to cast it each time you need it.</param>
        /// <param name="reader">The MemoryReader that has been attached to the process.</param>
        public abstract void UpdateGameState(TGameState gameState, MemoryReader reader);
    }
}
