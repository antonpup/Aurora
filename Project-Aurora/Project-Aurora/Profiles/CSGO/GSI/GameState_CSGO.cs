using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Aurora.Profiles;
using Aurora.Profiles.CSGO.GSI.Nodes;

namespace Aurora.Profiles.CSGO.GSI
{
    /// <summary>
    /// A class representing various information retaining to Game State Integration of Counter-Strike: Global Offensive
    /// </summary>
    public class GameState_CSGO : GameState
    {
        private ProviderNode _Provider;
        private MapNode _Map;
        private RoundNode _Round;
        private PlayerNode _Player;
        private AllPlayersNode _AllPlayers;
        private GameState_CSGO _Previously;
        private GameState_CSGO _Added;
        private AuthNode _Auth;

        /// <summary>
        /// Information about the provider of this GameState
        /// </summary>
        public ProviderNode Provider
        {
            get
            {
                if (_Provider == null)
                {
                    _Provider = new ProviderNode(_ParsedData["provider"]?.ToString() ?? "");
                }

                return _Provider;
            }
        }

        /// <summary>
        /// Information about the current map
        /// </summary>
        public MapNode Map
        {
            get
            {
                if (_Map == null)
                {
                    _Map = new MapNode(_ParsedData["map"]?.ToString() ?? "");
                }

                return _Map;
            }
        }

        /// <summary>
        /// Information about the current round
        /// </summary>
        public RoundNode Round
        {
            get
            {
                if (_Round == null)
                {
                    _Round = new RoundNode(_ParsedData["round"]?.ToString() ?? "");
                }

                return _Round;
            }
        }

        /// <summary>
        /// Information about the current player
        /// </summary>
        public PlayerNode Player
        {
            get
            {
                if (_Player == null)
                {
                    _Player = new PlayerNode(_ParsedData["player"]?.ToString() ?? "");
                }

                return _Player;
            }
        }

        /// <summary>
        /// Information about all players in the lobby
        /// </summary>
        public AllPlayersNode AllPlayers
        {
            get
            {
                if (_AllPlayers == null)
                {
                    _AllPlayers = new AllPlayersNode(_ParsedData["allplayers"]?.ToString() ?? "");
                }

                return _AllPlayers;
            }
        }

        /// <summary>
        /// A previous GameState
        /// </summary>
        public GameState_CSGO Previously
        {
            get
            {
                if (_Previously == null)
                {
                    _Previously = new GameState_CSGO(_ParsedData["previously"]?.ToString() ?? "");
                }

                return _Previously;
            }
        }

        /// <summary>
        /// A GameState with only added information
        /// </summary>
        public GameState_CSGO Added
        {
            get
            {
                if (_Added == null)
                {
                    _Added = new GameState_CSGO(_ParsedData["added"]?.ToString() ?? "");
                }
                return _Added;
            }
        }

        /// <summary>
        /// Information about GSI authentication
        /// </summary>
        public AuthNode Auth
        {
            get
            {
                if(_Auth == null)
                {
                    _Auth = new AuthNode(_ParsedData["auth"]?.ToString() ?? "");
                }

                return _Auth;
            }
        }


        public GameState_CSGO() : base() { }
        public GameState_CSGO(string json_data) : base(json_data) { }
    }
}
