using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Minecraft.GSI.Nodes {
    public class PlayerEffectsNode : AutoJsonNode<PlayerEffectsNode> {

        [AutoJsonPropertyName("absorption")] public bool HasAbsorption;
        [AutoJsonPropertyName("blindness")] public bool HasBlindness;
        [AutoJsonPropertyName("fireResistance")] public bool HasFireResistance;
        [AutoJsonPropertyName("invisibility")] public bool HasInvisibility;
        [AutoJsonPropertyName("confusion")] public bool HasNausea;
        [AutoJsonPropertyName("poison")] public bool HasPoison;
        [AutoJsonPropertyName("regeneration")] public bool HasRegeneration;
        [AutoJsonPropertyName("moveSlowdown")] public bool HasSlowness;
        [AutoJsonPropertyName("moveSpeed")] public bool HasSpeed;
        [AutoJsonPropertyName("wither")] public bool HasWither;

        internal PlayerEffectsNode() : base() { }
        internal PlayerEffectsNode(string json) : base(json) { }
    }
}
