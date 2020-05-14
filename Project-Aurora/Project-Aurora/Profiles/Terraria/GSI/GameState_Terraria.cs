using Aurora.Profiles.Generic.GSI.Nodes;
using Aurora.Profiles.Terraria.GSI.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Terraria.GSI {
    public class GameState_Terraria : GameState {
        public ProviderNode Provider => NodeFor<ProviderNode>("provider");

        public WorldNode World => NodeFor<WorldNode>("world");

        public PlayerNode Player => NodeFor<PlayerNode>("player");


        public GameState_Terraria() : base() { }
        public GameState_Terraria(string JSONstring) : base(JSONstring) { }
    }
}
