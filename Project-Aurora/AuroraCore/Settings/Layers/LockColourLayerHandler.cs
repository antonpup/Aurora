using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Utils;
using Newtonsoft.Json;
using System;
using System.Drawing;
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

    [Obsolete("This layer is obselete and has been replaced by the Overrides system.")]
    public class LockColourLayerHandler : LayerHandler<LockColourLayerHandlerProperties>
    {
        public LockColourLayerHandler() : base()
        {
            _ID = "LockColor";
        }
        public override EffectLayer Render(IGameState gamestate)
        {
            Color clr;

            if (System.Windows.Forms.Control.IsKeyLocked(Properties.ToggledKey))
            {
                clr = Properties.PrimaryColor;

                if (Properties.Pulse)
                {
                    double d = Math.Pow(Math.Sin(((Utils.TimeUtils.GetMillisecondsSinceEpoch() % 1500L) / 1500.0D) * Math.PI), 2);
                    clr = ColorUtils.MultiplyColorByScalar(clr, d);
                }
            }
            else
                clr = Properties.SecondaryColor;

            return new EffectLayer().Set(Properties.Sequence, clr);
        }
    }
}
