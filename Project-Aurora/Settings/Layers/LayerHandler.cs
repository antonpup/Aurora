using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        public UserControl Control
        {
            get
            {
                return _Control;
            }
        }

        [JsonIgnore]
        internal LayerType _Type;

        [JsonIgnore]
        public LayerType Type { get { return _Type; } }

        public Color PrimaryColor { get; set; }


        public LayerHandler()
        {
        }

        public LayerHandler(LayerHandler other) : base()
        {
            AffectedSequence = other.AffectedSequence;
        }

        public virtual EffectLayer Render(IGameState gamestate)
        {
            return new EffectLayer();
        }

        public virtual void SetProfile(ProfileManager profile)
        {

        }
    }
}
