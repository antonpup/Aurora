using Aurora.Profiles;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NAudio.CoreAudioApi;
using Aurora.Utils;

namespace Aurora.Profiles
{
    public class GameStateIgnoreAttribute : Attribute
    { }

    public class RangeAttribute : Attribute
    {
        public int Start { get; set; }

        public int End { get; set; }

        public RangeAttribute(int start, int end)
        {
            Start = start;
            End = end;
        }
    }

    /// <summary>
    /// A class representing various information retaining to the game.
    /// </summary>
    public interface IGameState
    {
        /// <summary>
        /// Information about the local system
        /// </summary>
        //LocalPCInformation LocalPCInfo { get; }

        JObject _ParsedData { get; set; }
        string json { get; set; }
        
        string GetNode(string name);
    }

    public class GameState<TSelf> : IGameState where TSelf : GameState<TSelf>
    {
        private static LocalPCInformation _localpcinfo;

        // Holds a cache of the child nodes on this gamestate
        private readonly Dictionary<string, object> childNodes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Information about the local system
        /// </summary>
        public LocalPCInformation LocalPCInfo => _localpcinfo ?? (_localpcinfo = new LocalPCInformation());

        [GameStateIgnore] public JObject _ParsedData { get; set; }
        [GameStateIgnore] public string json { get; set; }

        /// <summary>
        /// Creates a default GameState instance.
        /// </summary>
        public GameState() : base()
        {
            json = "{}";
            _ParsedData = JObject.Parse(json);
        }

        /// <summary>
        /// Creates a GameState instance based on the passed json data.
        /// </summary>
        /// <param name="json_data">The passed json data</param>
        public GameState(string json_data) : base()
        {
            if (String.IsNullOrWhiteSpace(json_data))
                json_data = "{}";

            json = json_data;
            _ParsedData = JObject.Parse(json_data);
        }

        /// <summary>
        /// A copy constructor, creates a GameState instance based on the data from the passed GameState instance.
        /// </summary>
        /// <param name="other_state">The passed GameState</param>
        public GameState(IGameState other_state) : base()
        {
            _ParsedData = other_state._ParsedData;
            json = other_state.json;
        }

        /// <summary>
        /// Gets the JSON for a child node in this GameState.
        /// </summary>
        public string GetNode(string name) =>
            _ParsedData.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var value) ? value.ToString() : "";

        /// <summary>
        /// Use this method to more-easily lazily return the child node of the given name that exists on this AutoNode.
        /// </summary>
        protected TNode NodeFor<TNode>(string name) where TNode : Node<TNode>
            => (TNode)(childNodes.TryGetValue(name, out var n) ? n : (childNodes[name] = Instantiator<TNode, string>.Create(_ParsedData[name]?.ToString() ?? "")));

        /// <summary>
        /// Displays the JSON, representative of the GameState data
        /// </summary>
        /// <returns>JSON String</returns>
        public override string ToString() => json;
    }


    /// <summary>
    /// An empty gamestate with no child nodes.
    /// </summary>
    public class EmptyGameState : GameState<EmptyGameState>
    {
        public EmptyGameState() : base() { }
        public EmptyGameState(IGameState gs) : base(gs) { }
        public EmptyGameState(string json) : base(json) { }
    }
}
