using Aurora.Utils;
using CSScriptLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Aurora.Profiles {

    /// <summary>
    /// The GameStatePluginManager handles the loading of special GSI plugins and also provides
    /// a collection of parameters that the user can select from the drop downs.
    /// <para>It implements IStringProperty so that it can be passed as the starting object to
    /// the `GameStateUtils._RetrieveGameStateParameter` method for getting plugin states.</para>
    /// </summary>
    public class GameStatePluginManager : IStringProperty {

        /// <summary>Directory that will be scanned for game state plugins.</summary>
        private readonly string PluginDirectory = Path.Combine(Global.ExecutingDirectory, "GameStatePlugins");

        /// <summary>Dictionary that contains the plugin ID mapped to the game state represented by that plugin.</summary>
        private Dictionary<string, IGameState> states { get; } = new Dictionary<string, IGameState>();

        /// <summary>Dictionary that contains the plugin ID mapped to the type of game state for that plugin.</summary>
        private Dictionary<string, ExternalScriptUtils.LoadedTypeWithAttribute<GameStatePluginAttribute>> stateTypes = new Dictionary<string, ExternalScriptUtils.LoadedTypeWithAttribute<GameStatePluginAttribute>>();

        /// <summary>ParameterLookup for all the loaded GSI plugins.</summary>
        public Dictionary<string, Tuple<Type, Type>> PluginParameterLookup { get; private set; } = new Dictionary<string, Tuple<Type, Type>>();

        /// <summary>
        /// Creates a new GameStatePluginManager and loads the GSI plugins.
        /// </summary>
        public GameStatePluginManager() {
            // Create the GameStatePlugins directory if it doesn't exist.
            if (!Directory.Exists(PluginDirectory))
                Directory.CreateDirectory(PluginDirectory);

            var loadedTypes = ExternalScriptUtils.LoadTypesWithAttribute<GameStatePluginAttribute>(PluginDirectory, typeof(GameState<>)) // Load all plugin files from the plugin directory and get all exported types that are GameState<T>s and have GameStatePluginAttributes
                .GroupBy(loaded => loaded.Attribute.ID).Select(group => FilterRepeatedIDs(group)) // Group the types by their defined attribute ID, then filter the groups so that only 1 type per ID is left. This logs any conflicting IDs to console and stops loading of conflicted plugins.
                .Where(TestForConstructors) // Check the GameStates for the correct constructors
                .ToList(); // Finally, cast to a list so that this is immediately executed (instead of once per time used below).

            // Create an initial game state for all the loaded plugins and store the type so we can constructor more when JSON data is received
            foreach (var ltype in loadedTypes) {
                states.Add(ltype.Attribute.ID.ToLowerInvariant(), (IGameState)ltype.Create(null));
                stateTypes.Add(ltype.Attribute.ID.ToLowerInvariant(), ltype);
            }

            // Create the parameter lookup dictionary:
            PluginParameterLookup = loadedTypes // for each loaded GameState
                .SelectMany(gs => GameStateUtils.ReflectGameStateParameters(gs.Type, new[] { "LocalPCInfo" }) // Reflect the possible values of it (apart from the built in "LocalPCInfo") and add all fields to the IEnumerable, having first
                    .Select(kvp => new KeyValuePair<string, Tuple<Type, Type>>($"Plugins/{gs.Attribute.ID}/{kvp.Key}", kvp.Value)) // Appended "Plugins" and the ID of the plugin to the front of the name
                )
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value); // Finally, cast the IEnumerable<KeyValuePair<...>> to a dictionary.
        }

        /// <summary>
        /// Takes a group of loaded types (with their path and attributes) that are grouped by ID and returns the first one in the group.
        /// If there are multiple items in the group, that means there are multiple types with the same ID. In this case, an error is logged
        /// for each type other than the first with a conflicting ID.
        /// </summary>
        private ExternalScriptUtils.LoadedTypeWithAttribute<GameStatePluginAttribute> FilterRepeatedIDs(IGrouping<string, ExternalScriptUtils.LoadedTypeWithAttribute<GameStatePluginAttribute>> group) {
            if (group.Count() > 1)
                foreach (var ltype in group.Skip(1))
                    Global.logger.Error("An error occured while trying to load game state plugin {0}. Exception: A plugin with this ID ('{1}') has already been registered. This plugin will be ignored.", ltype.Path, ltype.Attribute.ID); // If multiple plugins exist with the same ID, log an error to the console for each extra one (otther than the first)
            return group.First();
        }

        /// <summary>
        /// Tries to instantiate the given type with a parameter-less call and a string parameter to ensure that the provided
        /// type has valid constructors to be used with the program.
        /// </summary>
        private static bool TestForConstructors(ExternalScriptUtils.LoadedTypeWithAttribute<GameStatePluginAttribute> ltype) {
            try {
                ltype.Create(null);
                ltype.Create(new[] { "{}" });
                return true;
            } catch {
                Global.logger.Error("An error occured while trying to load game state plugin {0}. Exception: The plugin does not contain the required constructors. Must have a parameter-less constructor and a constructor that takes a string.", ltype.Path);
                return false;
            }
        }

        /// <summary>
        /// Attempts to update a plugin with the given ID with a new JSON game state.
        /// If a plugin with this ID exists, true is returned, else false is returned.
        /// </summary>
        public bool TryUpdateState(string id, string json) {
            if (states.ContainsKey(id)) {
                try {
                    states[id] = (IGameState)stateTypes[id].Create(new[] { json });
                    return true;
                } catch { }
            }
            return false;
        }

        /// <summary>Returns the state of the plugin whose ID is equal to the given name.</summary>
        public object GetValueFromString(string name, object input = null) => states[name.ToLowerInvariant()];

        // Extra methods required to implement IStringProperty. Neither of these should ever be called.
        public void SetValueFromString(string name, object value) => new NotImplementedException();
        public IStringProperty Clone() => throw new NotImplementedException();
    }


    /// <summary>
    /// Attribute that should be applied to GameStates that have been added in by plugins.
    /// </summary>
    public class GameStatePluginAttribute : Attribute {

        /// <summary>The ID of this GameStatePlugin.</summary>
        public string ID { get; private set; }

        public GameStatePluginAttribute(string id) {
            ID = id.Replace(" ", "_").Replace("/", "").Replace("\\", "");
        }
    }
}
