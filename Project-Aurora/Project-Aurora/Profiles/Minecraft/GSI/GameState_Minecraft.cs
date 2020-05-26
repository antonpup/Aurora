using Aurora.Profiles.Generic.GSI.Nodes;
using Aurora.Profiles.Minecraft.GSI.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Minecraft.GSI {

    public class GameState_Minecraft : GameState {

        /// <summary>
        /// Provider node provides information about the data source so that Aurora can update the correct gamestate.
        /// </summary>
        public ProviderNode Provider => NodeFor<ProviderNode>("provider");

        /// <summary>
        /// Player node provides information about the player (e.g. health and hunger).
        /// </summary>
        public GameNode Game => NodeFor<GameNode>("game");

        /// <summary>
        /// World node provides information about the world (e.g. rain intensity and time).
        /// </summary>
        public WorldNode World => NodeFor<WorldNode>("world");

        /// <summary>
        /// Player node provides information about the player (e.g. health and hunger).
        /// </summary>
        public PlayerNode Player => NodeFor<PlayerNode>("player");

        /// <summary>
        /// Creates a default GameState_Minecraft instance.
        /// </summary>
        public GameState_Minecraft() : base() { }

        /// <summary>
        /// Creates a GameState_Minecraft instance based on the passed JSON data.
        /// </summary>
        /// <param name="JSONstring"></param>
        public GameState_Minecraft(string JSONstring) : base(JSONstring) { }
        
    }
}
