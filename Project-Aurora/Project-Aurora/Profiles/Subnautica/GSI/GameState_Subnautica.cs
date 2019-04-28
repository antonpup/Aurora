using Aurora.Profiles.Subnautica.GSI.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Subnautica.GSI {

    public class GameState_Subnautica : GameState<GameState_Subnautica> {

        private ProviderNode _Provider;
        private GameStateNode _GameState;
        private NotificationNode _Notification;
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
        /// Game node provides information about the GameState (InMenu/loading/InGame) source so that Aurora can update the correct gamestate.
        /// </summary>
        public GameStateNode GameState
        {
            get
            {
                if (_GameState == null)
                    _GameState = new GameStateNode(_ParsedData["game_state"]?.ToString() ?? "");
                return _GameState;
            }
        }

        /// <summary>
        /// Notification node provides information about the Notifications (e.g. Log and Inventory Tab in the PDA).
        /// </summary>
        public NotificationNode Notification {
            get {
                if (_Notification == null)
                    _Notification = new NotificationNode(_ParsedData["notification"]?.ToString() ?? "");
                return _Notification;
            }
        }

        /// <summary>
        /// World node provides information about the world (e.g. time).
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
        /// Creates a default GameState_Subnautica instance.
        /// </summary>
        public GameState_Subnautica() : base() { }

        /// <summary>
        /// Creates a GameState_Subnautica instance based on the passed JSON data.
        /// </summary>
        /// <param name="JSONstring"></param>
        public GameState_Subnautica(string JSONstring) : base(JSONstring) { }

        /// <summary>
        /// Creates a GameState_Subnautica instance based on the data from the passed GameState instance.
        /// </summary>
        public GameState_Subnautica(IGameState other) : base(other) { }
        
    }
}
