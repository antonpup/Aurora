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

        public GameState_CSGO()
        {
            json = "{}";
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json);
        }

        public GameState_CSGO(string JSONstring)
        {
            if (String.IsNullOrWhiteSpace(JSONstring))
                JSONstring = "{}";

            json = JSONstring;
            _ParsedData = JObject.Parse(JSONstring);
        }

        public GameState_CSGO(GameState other_state) : base(other_state)
        {
        }
    }
}
