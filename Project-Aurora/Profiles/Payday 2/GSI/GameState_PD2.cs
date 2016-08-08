using Aurora.Profiles.Payday_2.GSI.Nodes;
using Newtonsoft.Json.Linq;
using System;

namespace Aurora.Profiles.Payday_2.GSI
{
    /// <summary>
    /// A class representing various information retaining to Payday 2
    /// </summary>
    public class GameState_PD2 : GameState
    {
        private ProviderNode _Provider;
        private LobbyNode _Lobby;
        private LevelNode _Level;
        private PlayersNode _Players;
        private GameNode _Game;
        private GameState_PD2 _Previously;

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
        /// Information about the game lobby
        /// </summary>
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

        /// <summary>
        /// Information about the game level
        /// </summary>
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

        /// <summary>
        /// Information about players in the lobby
        /// </summary>
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

        /// <summary>
        /// Information about the game
        /// </summary>
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

        /// <summary>
        ///  A previous GameState
        /// </summary>
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

        /// <summary>
        /// Creates a default GameState_PD2 instance.
        /// </summary>
        public GameState_PD2()
        {
            json = "{}";
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json);
        }

        /// <summary>
        /// Creates a GameState_PD2 instance based on the passed json data.
        /// </summary>
        /// <param name="JSONstring">The passed json data</param>
        public GameState_PD2(string JSONstring)
        {
            if (String.IsNullOrWhiteSpace(JSONstring))
                JSONstring = "{}";

            json = JSONstring;
            _ParsedData = JObject.Parse(JSONstring);
        }

        /// <summary>
        /// A copy constructor, creates a GameState_CSGO instance based on the data from the passed GameState instance.
        /// </summary>
        /// <param name="other_state">The passed GameState</param>
        public GameState_PD2(GameState other_state) : base(other_state)
        {
        }
    }
}
