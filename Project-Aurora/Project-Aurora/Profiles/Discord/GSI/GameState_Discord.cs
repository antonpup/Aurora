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
        public ProviderNode Provider => NodeFor<ProviderNode>("provider");

        public UserNode User => NodeFor<UserNode>("user");

        public GuildNode Guild => NodeFor<GuildNode>("guild");

        public TextNode Text => NodeFor<TextNode>("text");

        public VoiceNode Voice => NodeFor<VoiceNode>("voice");

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
