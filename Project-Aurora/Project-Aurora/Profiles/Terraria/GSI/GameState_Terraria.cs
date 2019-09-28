using Aurora.Profiles.Terraria.GSI.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Terraria.GSI {
    public class GameState_Terraria : GameState<GameState_Terraria> {
        private ProviderNode _Provider;
        private WorldNode _World;
        private PlayerNode _Player;

        public ProviderNode Provider => _Provider ?? (_Provider = new ProviderNode(_ParsedData["provider"]?.ToString() ?? ""));

        public WorldNode World => _World ?? (_World = new WorldNode(_ParsedData["world"]?.ToString() ?? ""));

        public PlayerNode Player => _Player ?? (_Player = new PlayerNode(_ParsedData["player"]?.ToString() ?? ""));

        public GameState_Terraria() : base() { }

        /// <summary>
        /// Creates a GameState_Terraria instance based on the passed JSON data.
        /// </summary>
        /// <param name="JSONstring"></param>
        public GameState_Terraria(string JSONstring) : base(JSONstring) { }

        /// <summary>
        /// Creates a GameState_Terraria instance based on the data from the passed GameState instance.
        /// </summary>
        public GameState_Terraria(IGameState other) : base(other) { }
    }
}
