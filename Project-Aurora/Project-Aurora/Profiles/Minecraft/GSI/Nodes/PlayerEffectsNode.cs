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
            HasAbsorption = GetBool("absorption");
            HasBlindness = GetBool("blindness");
            HasFireResistance = GetBool("fireResistance");
            HasInvisibility = GetBool("invisibility");
            HasNausea = GetBool("confusion");
            HasPoison = GetBool("poison");
            HasRegeneration = GetBool("regeneration");
            HasSlowness = GetBool("moveSlowdown");
            HasSpeed = GetBool("moveSpeed");
            HasWither = GetBool("wither");
        }
    }
}
