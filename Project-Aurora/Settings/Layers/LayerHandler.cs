using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public class LayerHandler
    {
        public KeySequence AffectedSequence = new KeySequence();

        [JsonIgnore]
        internal UserControl _Control;

        [JsonIgnore]
        public UserControl Control { get { return _Control; } }

        [JsonIgnore]
        internal LayerType _Type;

        [JsonIgnore]
        public LayerType Type { get { return _Type; } }


        public LayerHandler()
        {
        }

        public LayerHandler(LayerHandler other) : base()
        {
            AffectedSequence = other.AffectedSequence;
        }

        public virtual EffectLayer Render(GameState gamestate)
        {
            return new EffectLayer();
        }
    }
}
