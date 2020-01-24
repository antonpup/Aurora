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

namespace Aurora.Settings.Layers {
    public class RadialLayerProperties : LayerHandlerProperties<RadialLayerProperties> {

        public SegmentedRadialBrushFactory _Brush { get; set; }
        [JsonIgnore] public SegmentedRadialBrushFactory Brush => _Brush ?? Logic._Brush ?? new SegmentedRadialBrushFactory(new[] { Color.Magenta, Color.Yellow, Color.Cyan }) { EasingAmount = 3 };

        public RadialLayerProperties() : base() { }
        public RadialLayerProperties(bool empty = false) : base(empty) { }

        public override void Default() {
            base.Default();
        }
    }

    public class RadialLayerHandler : LayerHandler<RadialLayerProperties> {

        public RadialLayerHandler() {
            _ID = "Radial";
        }

        protected override UserControl CreateControl() => new Control_RadialLayer(this);

        public override EffectLayer Render(IGameState gamestate) {
            var area = Properties.Sequence.GetAffectedRegion();
            var brush = Properties.Brush.GetBrush(area, keepAspectRatio: false);
            return new EffectLayer().Set(Properties.Sequence, brush);
        }
    }
}
