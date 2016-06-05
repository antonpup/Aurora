using Aurora.Profiles.Payday_2.GSI.Nodes;
using Newtonsoft.Json.Linq;
using System;

namespace Aurora.Profiles.Payday_2.GSI
{
    public class GameState_PD2 : GameState
    {
        private ProviderNode _Provider;
        private LobbyNode _Lobby;
        private LevelNode _Level;
        private PlayersNode _Players;
        private GameNode _Game;
        private GameState_PD2 _Previously;

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

        public LobbyNode Lobby
        {
            get
            {
                if (_Lobby == null)
                {
                    _Lobby = new LobbyNode(_ParsedData["lobby"]?.ToString() ?? "");
                }

                return _Lobby;
            }
        }

        public LevelNode Level
        {
            get
            {
                if (_Level == null)
                {
                    _Level = new LevelNode(_ParsedData["level"]?.ToString() ?? "");
                }

                return _Level;
            }
        }

        public PlayersNode Players
        {
            get
            {
                if (_Players == null)
                {
                    _Players = new PlayersNode(_ParsedData["players"]?.ToString() ?? "");
                }

                return _Players;
            }
        }

        public GameNode Game
        {
            get
            {
                if (_Game == null)
                {
                    _Game = new GameNode(_ParsedData["game"]?.ToString() ?? "");
                }

                return _Game;
            }
        }

        public GameState_PD2 Previously
        {
            get
            {
                if (_Previously == null)
                {
                    _Previously = new GameState_PD2(_ParsedData["previous"]?.ToString() ?? "");
                }

                return _Previously;
            }
        }


        public GameState_PD2()
        {
            json = "{}";
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json);
        }

        public GameState_PD2(string JSONstring)
        {
            if (String.IsNullOrWhiteSpace(JSONstring))
                JSONstring = "{}";

            json = JSONstring;
            _ParsedData = JObject.Parse(JSONstring);
        }

        public GameState_PD2(GameState other_state) : base(other_state)
        {
        }
    }
}
