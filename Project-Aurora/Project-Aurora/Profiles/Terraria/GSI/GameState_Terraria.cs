using Aurora.Profiles.Generic.GSI.Nodes;
using Aurora.Profiles.Terraria.GSI.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Terraria.GSI {
    public class GameState_Terraria : GameState<GameState_Terraria> {
        public ProviderNode Provider => NodeFor<ProviderNode>("provider");

        public WorldNode World => NodeFor<WorldNode>("world");

        public PlayerNode Player => NodeFor<PlayerNode>("player");

        public GameState_Terraria() : base() { }

        /// <summary>
        /// Creates a GameState_Terraria instance based on the passed JSON data.
        /// </summary>
        /// <param name="JSONstring"></param>
        public GameState_Terraria(string JSONstring) : base(JSONstring) { }

        /// <summary>
        /// Creates a GameState_Terraria instance based on the data from the passed GameState instance.
        /// </summary>
        public GameState_Terraria(IGameState other) : base(other) { }
    }
}
