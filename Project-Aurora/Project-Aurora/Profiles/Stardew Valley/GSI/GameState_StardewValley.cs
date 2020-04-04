using Aurora.Profiles.Generic.GSI.Nodes;
using Aurora.Profiles.StardewValley.GSI.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.StardewValley.GSI {
    public class GameState_StardewValley : GameState<GameState_StardewValley> {
        public ProviderNode Provider => NodeFor<ProviderNode>("provider");

        public WorldNode World => NodeFor<WorldNode>("world");

        public PlayerNode Player => NodeFor<PlayerNode>("player");
        public JournalNode Journal => NodeFor<JournalNode>("journal");
        public GameStateNode GameState => NodeFor<GameStateNode>("game_state");

        public GameState_StardewValley() : base() { }

        /// <summary>
        /// Creates a GameState_Terraria instance based on the passed JSON data.
        /// </summary>
        /// <param name="JSONstring"></param>
        public GameState_StardewValley(string JSONstring) : base(JSONstring) { }

        /// <summary>
        /// Creates a GameState_Terraria instance based on the data from the passed GameState instance.
        /// </summary>
        public GameState_StardewValley(IGameState other) : base(other) { }
    }
}
