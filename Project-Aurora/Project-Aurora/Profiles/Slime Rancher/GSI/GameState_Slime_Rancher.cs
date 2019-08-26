using Aurora.Profiles.Slime_Rancher.GSI.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Slime_Rancher.GSI {

    public class GameState_Slime_Rancher : GameState<GameState_Slime_Rancher> {

        private ProviderNode _Provider;
        private GameStateNode _GameState;
        private PlayerNode _Player;
        private VacPackNode _VacPack;
        private MailNode _Mail;
        private WorldNode _World;
        private LocationNode _Location;


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
        /// Game State node provides information about the GameState (InMenu/loading/InGame).
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
        /// Player node provides information about the player (e.g. health and hunger).
        /// </summary>
        public PlayerNode Player
        {
            get
            {
                if (_Player == null)
                    _Player = new PlayerNode(_ParsedData["player"]?.ToString() ?? "");
                return _Player;
            }
        }
        
        /// <summary>
        /// Vac Pack node provides information about the Inventory.
        /// </summary>
        public VacPackNode VacPack
        {
            get
            {
                if (_VacPack == null)
                    _VacPack = new VacPackNode(_ParsedData["vac_pack"]?.ToString() ?? "");
                return _VacPack;
            }
        }

        /// <summary>
        /// Mail node provides information about the Mails.
        /// </summary>
        public MailNode Mail {
            get {
                if (_Mail == null)
                    _Mail = new MailNode(_ParsedData["mail"]?.ToString() ?? "");
                return _Mail;
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
        /// Location node provides information about the Location (e.g. InRanch).
        /// </summary>
        public LocationNode Location {
            get {
                if (_Location == null)
                    _Location = new LocationNode(_ParsedData["location"]?.ToString() ?? "");
                return _Location;
            }
        }

        /// <summary>
        /// Creates a default GameState_Slime_Rancher instance.
        /// </summary>
        public GameState_Slime_Rancher() : base() { }

        /// <summary>
        /// Creates a GameState_Slime_Rancher instance based on the passed JSON data.
        /// </summary>
        /// <param name="JSONstring"></param>
        public GameState_Slime_Rancher(string JSONstring) : base(JSONstring) { }

        /// <summary>
        /// Creates a GameState_Slime_Rancher instance based on the data from the passed GameState instance.
        /// </summary>
        public GameState_Slime_Rancher(IGameState other) : base(other) { }
        
    }
}
