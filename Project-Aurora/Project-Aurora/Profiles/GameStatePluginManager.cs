using Aurora.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        private Dictionary<string, Type> stateTypes = new Dictionary<string, Type>();

        /// <summary>ParameterLookup for all the loaded GSI plugins.</summary>
        public Dictionary<string, Tuple<Type, Type>> PluginParameterLookup { get; private set; }

        /// <summary>
        /// Creates a new GameStatePluginManager and loads the GSI plugins.
        /// </summary>
        public GameStatePluginManager() {
            // Load the plugins from disk (TODO)
            var loadedTypes = new[] { (type: typeof(TestGameState), attr: new GameStatePluginAttribute("Test Game State")), (type: typeof(Minecraft.GSI.GameState_Minecraft), attr: new GameStatePluginAttribute("Test Minecraft State")) };

            // Create an initial game state for all the loaded plugins and store the type so we can constructor more when JSON data is received
            foreach (var (type, attr) in loadedTypes) {
                states.Add(attr.ID, (IGameState)Activator.CreateInstance(type));
                stateTypes.Add(attr.ID, type);
            }

            // Create the parameter lookup dictionary:
            PluginParameterLookup = loadedTypes // for each loaded GameState
                .SelectMany(gs => GameStateUtils.ReflectGameStateParameters(gs.type) // Reflect the possible values of it and add all fields to the IEnumerable, having first
                    .Select(kvp => new KeyValuePair<string, Tuple<Type, Type>>($"Plugins/{gs.attr.ID}/{kvp.Key}", kvp.Value)) // Appended "Plugins" and the ID of the plugin to the front of the name
                )
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value); // Finally, cast the IEnumerable<KeyValuePair<...>> to a dictionary.
        }

        /// <summary>
        /// Attempts to update a plugin with the given ID with a new JSON game state.
        /// If a plugin with this ID exists, true is returned, else false is returned.
        /// </summary>
        public bool TryUpdateState(string id, string json) {
            if (states.ContainsKey(id)) {
                states[id] = (IGameState)Activator.CreateInstance(stateTypes[id], json);
                return true;
            }
            return false;
        }

        /// <summary>Returns the state of the plugin whose ID is equal to the given name.</summary>
        public object GetValueFromString(string name, object input = null) { ((Minecraft.GSI.GameState_Minecraft)states["Minecraft"]).Player.Health = (((Minecraft.GSI.GameState_Minecraft)states["Minecraft"]).Player.Health + 1) % 10; return states[name]; }

        // Extra methods required to implement IStringProperty. Neither of these should ever be called.
        public void SetValueFromString(string name, object value) => new NotImplementedException();
        public IStringProperty Clone() => throw new NotImplementedException();
    }


    public class TestGameState : GameState<TestGameState> {
        public int TestField;
        public string TestField2;
        public double TestField3;
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
