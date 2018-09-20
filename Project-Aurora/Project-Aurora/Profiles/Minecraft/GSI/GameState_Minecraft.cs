using Aurora.Profiles.Minecraft.GSI.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Minecraft.GSI {

    public class GameState_Minecraft : GameState<GameState_Minecraft> {

        private ProviderNode _Provider;
        private GameNode _Game;
        private WorldNode _World;
        private PlayerNode _Player;

        /// <summary>
        /// Provider node provides information about the data source so that Aurora can update the correct gamestate.
        /// </summary>
        public ProviderNode Provider {
            get {
                if (_Provider == null)
                    _Provider = new ProviderNode(_ParsedData["provider"]?.ToString() ?? "");
                return _Provider;
            }
        }

        /// <summary>
        /// Player node provides information about the player (e.g. health and hunger).
        /// </summary>
        public GameNode Game {
            get {
                if (_Game == null)
                    _Game = new GameNode(_ParsedData["game"]?.ToString() ?? "");
                return _Game;
            }
        }

        /// <summary>
        /// World node provides information about the world (e.g. rain intensity and time).
        /// </summary>
        public WorldNode World {
            get {
                if (_World == null)
                    _World = new WorldNode(_ParsedData["world"]?.ToString() ?? "");
                return _World;
            }
        }

        /// <summary>
        /// Player node provides information about the player (e.g. health and hunger).
        /// </summary>
        public PlayerNode Player {
            get {
                if (_Player == null)
                    _Player = new PlayerNode(_ParsedData["player"]?.ToString() ?? "");
                return _Player;
            }
        }

        /// <summary>
        /// Creates a default GameState_Minecraft instance.
        /// </summary>
        public GameState_Minecraft() : base() { }

        /// <summary>
        /// Creates a GameState_Minecraft instance based on the passed JSON data.
        /// </summary>
        /// <param name="JSONstring"></param>
        public GameState_Minecraft(string JSONstring) : base(JSONstring) { }

        /// <summary>
        /// Creates a GameState_Minecraft instance based on the data from the passed GameState instance.
        /// </summary>
        public GameState_Minecraft(IGameState other) : base(other) { }
        
    }
}
