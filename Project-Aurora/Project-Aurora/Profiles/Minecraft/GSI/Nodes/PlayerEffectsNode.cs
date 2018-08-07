using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Minecraft.GSI.Nodes {
    public class PlayerEffectsNode : Node<PlayerEffectsNode> {

        public bool HasAbsorption;
        public bool HasBlindness;
        public bool HasFireResistance;
        public bool HasInvisibility;
        public bool HasNausea;
        public bool HasPoison;
        public bool HasRegeneration;
        public bool HasSlowness;
        public bool HasSpeed;
        public bool HasWither;

        internal PlayerEffectsNode() : base() { }
        internal PlayerEffectsNode(string json) : base(json) {
            HasAbsorption = GetBool("effect.absorption");
            HasBlindness = GetBool("effect.blindness");
            HasFireResistance = GetBool("effect.fireResistance");
            HasInvisibility = GetBool("effect.invisibility");
            HasNausea = GetBool("effect.confusion");
            HasPoison = GetBool("effect.poison");
            HasRegeneration = GetBool("effect.regeneration");
            HasSlowness = GetBool("effect.moveSlowdown");
            HasSpeed = GetBool("effect.moveSpeed");
            HasWither = GetBool("effect.wither");
        }
    }
}
