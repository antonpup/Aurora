using System;
using System.Drawing;
using System.Windows.Forms;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Layers.Controls;
using Aurora.Utils;
using Newtonsoft.Json;
using UserControl = System.Windows.Controls.UserControl;

namespace Aurora.Settings.Layers
{
    public class LockColourLayerHandlerProperties : LayerHandlerProperties2Color<LockColourLayerHandlerProperties>
    {
        public LockColourLayerHandlerProperties()
        { }

        public LockColourLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public Keys? _ToggledKey { get; set; }

        [JsonIgnore]
        public Keys ToggledKey => (Logic._ToggledKey ?? _ToggledKey) ?? 0;

        public bool? _Pulse { get; set; }

        [JsonIgnore]
        public bool Pulse => (Logic._Pulse ?? _Pulse) ?? false;

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
        public LockColourLayerHandler() : base("LockColourLayer - Deprecated")
        {
        }

        protected override UserControl CreateControl()
        {
            return new Control_LockColourLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            Color clr;

            if (System.Windows.Forms.Control.IsKeyLocked(Properties.ToggledKey))
            {
                clr = Properties.PrimaryColor;

                if (!Properties.Pulse)
                {
                    EffectLayer.Set(Properties.Sequence, clr);
                    return EffectLayer;
                }

                var d = Math.Pow(Math.Sin(Time.GetMillisecondsSinceEpoch() % 1500L / 1500.0D * Math.PI), 2);
                clr = ColorUtils.MultiplyColorByScalar(clr, d);
            }
            else
                clr = Properties.SecondaryColor;

            EffectLayer.Set(Properties.Sequence, clr);
            return EffectLayer;
        }
    }
}
