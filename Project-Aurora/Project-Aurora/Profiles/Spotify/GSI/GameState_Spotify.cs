using Aurora.Profiles.Spotify.GSI.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Spotify.GSI
{
    public class GameState_Spotify : GameState<GameState_Spotify>
    {

        private ProviderNode _Provider;
        private PlayerNode _Player;
        private ColorsNode _Colors;

        public ProviderNode Provider => _Provider ?? (_Provider = new ProviderNode(_ParsedData["provider"]?.ToString() ?? ""));

        public PlayerNode Player => _Player ?? (_Player = new PlayerNode(_ParsedData["player"]?.ToString() ?? ""));

        public ColorsNode Colors => _Colors ?? (_Colors = new ColorsNode(_ParsedData["colors"]?.ToString() ?? ""));

        public GameState_Spotify() : base() { }

        public GameState_Spotify(string JSONstring) : base(JSONstring) { }

        public GameState_Spotify(IGameState other) : base(other) { }
    }
}