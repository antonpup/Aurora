using Aurora.Profiles.Discord.GSI.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Discord.GSI {

    public class GameState_Discord : GameState<GameState_Discord> {

        private ProviderNode _Provider;
        private UserNode _User;
        private GuildNode _Guild;
        private TextNode _Text;
        private VoiceNode _Voice;

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

        public UserNode User
        {
            get
            {
                if (_User == null)
                    _User = new UserNode(_ParsedData["user"]?.ToString() ?? "");
                return _User;
            }
        }

        public GuildNode Guild
        {
            get
            {
                if (_Guild == null)
                    _Guild = new GuildNode(_ParsedData["guild"]?.ToString() ?? "");
                return _Guild;
            }
        }
        public TextNode Text {
            get {
                if (_Text == null)
                    _Text = new TextNode(_ParsedData["text"]?.ToString() ?? "");
                return _Text;
            }
        }

        public VoiceNode Voice {
            get {
                if (_Voice == null)
                    _Voice = new VoiceNode(_ParsedData["voice"]?.ToString() ?? "");
                return _Voice;
            }
        }

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
