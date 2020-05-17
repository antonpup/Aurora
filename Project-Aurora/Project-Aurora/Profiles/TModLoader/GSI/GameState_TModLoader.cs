using Aurora.Profiles.Generic.GSI.Nodes;
using Aurora.Profiles.TModLoader.GSI.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.TModLoader.GSI {
    public class GameState_TModLoader : GameState {
        public ProviderNode Provider => NodeFor<ProviderNode>("provider");

        public WorldNode World => NodeFor<WorldNode>("world");

        public PlayerNode Player => NodeFor<PlayerNode>("player");

        public GameState_TModLoader() : base() { }

        /// <summary>
        /// Creates a GameState_Terraria instance based on the passed JSON data.
        /// </summary>
        /// <param name="JSONstring"></param>
        public GameState_TModLoader(string JSONstring) : base(JSONstring) { }
    }
}
