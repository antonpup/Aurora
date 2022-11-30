using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Aurora.Settings.Layers.Controls;

namespace Aurora.Settings.Layers {

    public class RadialLayerProperties : LayerHandlerProperties<RadialLayerProperties> {

        private static readonly SegmentedRadialBrushFactory defaultFactory = new(new Utils.ColorStopCollection(
            new[] { Color.Red, Color.Orange, Color.Yellow, Color.Lime, Color.Cyan, Color.Blue, Color.Purple, Color.Red }));

        public SegmentedRadialBrushFactory _Brush { get; set; }
        [JsonIgnore] public SegmentedRadialBrushFactory Brush => Logic._Brush ?? _Brush ?? defaultFactory;

        // Number of degrees per second the brush rotates at.
        [LogicOverridable("Animation Speed")] public int? _AnimationSpeed { get; set; }
        [JsonIgnore] public int AnimationSpeed => Logic._AnimationSpeed ?? _AnimationSpeed ?? 60;

        public RadialLayerProperties() : base() { }
        public RadialLayerProperties(bool empty = false) : base(empty) { }

        public override void Default() {
            base.Default();
            _Sequence = new KeySequence(Effects.WholeCanvasFreeForm);
            _Brush = (SegmentedRadialBrushFactory)defaultFactory.Clone();
            _AnimationSpeed = 60;
        }
    }

    public class RadialLayerHandler : LayerHandler<RadialLayerProperties> {
        private readonly Stopwatch _sw = new();
        private float _angle;
        private bool invalidated;

        protected override UserControl CreateControl() => new Control_RadialLayer(this);

        public RadialLayerHandler(): base("RadialLayer")
        {
            Properties.PropertyChanged += PropertiesChanged;
        }

        private void PropertiesChanged(object sender, PropertyChangedEventArgs e)
        {
            invalidated = true;
        }

        public override EffectLayer Render(IGameState gamestate) {
            if (invalidated)
            {
                EffectLayer.Clear();
                invalidated = false;
            }
            
            // Calculate delta time
            var dt = _sw.Elapsed.TotalSeconds;
            _sw.Restart();

            // Update angle
            _angle = (_angle + (float)(dt * Properties.AnimationSpeed)) % 360;

            var area = Properties.Sequence.GetAffectedRegion();
            var brush = Properties.Brush.GetBrush(area, _angle);
            return EffectLayer.Set(Properties.Sequence, brush);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                EffectLayer?.Dispose();
                Properties.PropertyChanged -= PropertiesChanged;
            }
        }

        public sealed override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
