using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Drawing;
using Aurora.Utils;
using System.Windows.Forms;

namespace Aurora.Settings.Layers
{
    public class LockColourLayerHandlerProperties : LayerHandlerProperties2Color<LockColourLayerHandlerProperties>
    {
        public LockColourLayerHandlerProperties() : base() { }

        public LockColourLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public Keys? _ToggledKey { get; set; }

        [JsonIgnore]
        public Keys ToggledKey { get { return (Logic._ToggledKey ?? _ToggledKey) ?? (Keys)0; } }

        public bool? _Pulse { get; set; }

        [JsonIgnore]
        public bool Pulse { get { return (Logic._Pulse ?? _Pulse) ?? false; } }

        public override void Default()
        {
            base.Default();
            _ToggledKey = Keys.CapsLock;
            _Pulse = false;
            _SecondaryColor = Color.FromArgb(0, 0, 0, 0);
        }
    }

    public class LockColourLayerHandler : LayerHandler<LockColourLayerHandlerProperties>
    {
        public LockColourLayerHandler() : base()
        {
            _Control = new Control_LockColourLayer(this);

            _Type = LayerType.LockColor;
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer layer = new EffectLayer();

            Color clr;

            if (System.Windows.Forms.Control.IsKeyLocked(Properties.ToggledKey))
            {
                clr = Properties.PrimaryColor;

                if (Properties.Pulse)
                {
                    double d = Math.Pow(Math.Sin(((Utils.Time.GetMillisecondsSinceEpoch() % 1500L) / 1500.0D) * Math.PI), 2);
                    clr = ColorUtils.MultiplyColorByScalar(clr, d);
                }
            }
            else
                clr = Properties.SecondaryColor;

            layer.Set(Properties.Sequence, clr);
            //layer.Set(KeyUtils.GetDeviceKey(Properties.ToggledKey), clr);

            return layer;
        }
    }
}
