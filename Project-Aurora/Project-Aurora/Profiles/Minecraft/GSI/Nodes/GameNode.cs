using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Minecraft.GSI.Nodes {

    public class GameNode : AutoJsonNode<GameNode> {

        [AutoJsonPropertyName("keys")] public MinecraftKeyBinding[] KeyBindings;
        public bool ControlsGuiOpen;
        public bool ChatGuiOpen;

        internal GameNode() : base() { }
        internal GameNode(string json) : base(json) { }
    }
}
