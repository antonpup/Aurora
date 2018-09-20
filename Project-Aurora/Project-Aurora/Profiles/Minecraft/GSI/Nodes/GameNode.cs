using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Minecraft.GSI.Nodes {

    public class GameNode : Node<GameNode> {

        public MinecraftKeyBinding[] KeyBindings;
        public bool ControlsGuiOpen;

        internal GameNode() : base() { }
        internal GameNode(string json) : base(json) {
            KeyBindings = GetArray<MinecraftKeyBinding>("keys");
            ControlsGuiOpen = GetBool("controlsGuiOpen");
        }
    }
}
