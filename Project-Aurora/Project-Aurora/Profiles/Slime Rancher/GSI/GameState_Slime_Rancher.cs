using Aurora.Profiles.Generic.GSI.Nodes;
using Aurora.Profiles.Slime_Rancher.GSI.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Slime_Rancher.GSI {

    public class GameState_Slime_Rancher : GameState {

        public ProviderNode Provider => NodeFor<ProviderNode>("provider");
        public GameStateNode GameState => NodeFor<GameStateNode>("game_state");
        public PlayerNode Player => NodeFor<PlayerNode>("player");
        public VacPackNode VacPack => NodeFor<VacPackNode>("vac_pack");
        public MailNode Mail => NodeFor<MailNode>("mail");
        public WorldNode World => NodeFor<WorldNode>("world");
        public LocationNode Location => NodeFor<LocationNode>("location");

        public GameState_Slime_Rancher() : base() { }

        /// <summary>
        /// Creates a GameState_Slime_Rancher instance based on the passed JSON data.
        /// </summary>
        /// <param name="JSONstring"></param>
        public GameState_Slime_Rancher(string JSONstring) : base(JSONstring) { }
    }
}
