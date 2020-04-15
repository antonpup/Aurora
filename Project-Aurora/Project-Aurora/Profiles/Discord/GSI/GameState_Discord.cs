using Aurora.Profiles.Discord.GSI.Nodes;
using Aurora.Profiles.Generic.GSI.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Discord.GSI
{
    public class GameState_Discord : GameState<GameState_Discord>
    {
        private ProviderNode _Provider;
        private UserNode _User;
        private GuildNode _Guild;
        private TextNode _Text;
        private VoiceNode _Voice;

        /// <summary>
        /// Provider node provides information about the data source so that Aurora can update the correct gamestate.
        /// </summary>
        public ProviderNode Provider => _Provider ?? (_Provider = new ProviderNode(_ParsedData["provider"]?.ToString() ?? ""));

        public UserNode User => _User ?? (_User = new UserNode(_ParsedData["user"]?.ToString() ?? ""));

        public GuildNode Guild => _Guild ?? (_Guild = new GuildNode(_ParsedData["guild"]?.ToString() ?? ""));

        public TextNode Text => _Text ?? (_Text = new TextNode(_ParsedData["text"]?.ToString() ?? ""));

        public VoiceNode Voice => _Voice ?? (_Voice = new VoiceNode(_ParsedData["voice"]?.ToString() ?? ""));

        /// <summary>
        /// Creates a default GameState_Discord instance.
        /// </summary>
        public GameState_Discord() : base() { }

        /// <summary>
        /// Creates a GameState_Discord instance based on the passed JSON data.
        /// </summary>
        /// <param name="JSONstring"></param>
        public GameState_Discord(string JSONstring) : base(JSONstring) { }

        /// <summary>
        /// Creates a GameState_Discord instance based on the data from the passed GameState instance.
        /// </summary>
        public GameState_Discord(IGameState other) : base(other) { }

    }
}
