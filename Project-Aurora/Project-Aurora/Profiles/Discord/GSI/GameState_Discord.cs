using Aurora.Profiles.Discord.GSI.Nodes;
using Aurora.Profiles.Generic.GSI.Nodes;

namespace Aurora.Profiles.Discord.GSI
{
    public class GameState_Discord : GameState
    {
        public ProviderNode Provider => NodeFor<ProviderNode>("provider");

        public UserNode User => NodeFor<UserNode>("user");

        public GuildNode Guild => NodeFor<GuildNode>("guild");

        public TextNode Text => NodeFor<TextNode>("text");

        public VoiceNode Voice => NodeFor<VoiceNode>("voice");


        public GameState_Discord() : base() { }
        public GameState_Discord(string JSONstring) : base(JSONstring) { }
    }
}
