using Aurora.EffectsEngine;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.EliteDangerous.Layers
{
    public class EliteDangerousBackgroundHandlerProperties : LayerHandlerProperties2Color<EliteDangerousBackgroundHandlerProperties>
    {
        public Color? _DefaultColor { get; set; }

        public Color DefaultColor { get { return Logic._DefaultColor ?? _DefaultColor ?? Color.Empty; } }

        public EliteDangerousBackgroundHandlerProperties() : base() { }

        public EliteDangerousBackgroundHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._DefaultColor = Color.FromArgb(60, 0, 0);
        }
    }
    public class EliteDangerousBackgroundLayerHandler : LayerHandler<EliteDangerousBackgroundHandlerProperties>
    {
        public EliteDangerousBackgroundLayerHandler() : base()
        {
            _ID = "EliteDangerousBackground";
        }

        protected override UserControl CreateControl()
        {
            return new Control_EliteDangerousBackgroundLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer bg_layer = new EffectLayer("Elite: Dangerous - Background");

            Color bg_color = this.Properties.DefaultColor;
            bg_layer.Fill(bg_color);

            return bg_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_EliteDangerousBackgroundLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}
