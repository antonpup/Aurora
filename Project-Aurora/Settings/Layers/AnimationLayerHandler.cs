using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public class AnimationLayerHandlerProperties : LayerHandlerProperties2Color<AnimationLayerHandlerProperties>
    {
        public AnimationMix _AnimationMix { get; set; }

        [JsonIgnore]
        public AnimationMix AnimationMix { get { return Logic._AnimationMix ?? _AnimationMix; } }

        public bool? _forceKeySequence { get; set; }

        [JsonIgnore]
        public bool ForceKeySequence { get { return Logic._forceKeySequence ?? _forceKeySequence ?? false; } }

        public bool? _scaleToKeySequenceBounds { get; set; }

        [JsonIgnore]
        public bool ScaleToKeySequenceBounds { get { return Logic._scaleToKeySequenceBounds ?? _scaleToKeySequenceBounds ?? false; } }

        public float? _AnimationDuration { get; set; }

        [JsonIgnore]
        public float AnimationDuration { get { return Logic._AnimationDuration ?? _AnimationDuration ?? 0.0f; } }

        public int? _AnimationRepeat { get; set; }

        [JsonIgnore]
        public int AnimationRepeat { get { return Logic._AnimationRepeat ?? _AnimationRepeat ?? 0; } }

        public AnimationLayerHandlerProperties() : base() { }

        public AnimationLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._AnimationMix = new AnimationMix();
            this._forceKeySequence = false;
            this._scaleToKeySequenceBounds = false;
            this._AnimationDuration = 0.0f;
            this._AnimationRepeat = 0;
        }
    }

    public class AnimationLayerHandler : LayerHandler<AnimationLayerHandlerProperties>
    {
        private int _playTimes = 0;
        private float _previousAnimationTime = 0.0f;
        private float _currentAnimationTime = 0.0f;

        private float GetAnimationTime()
        {
            long _timeDiff = Utils.Time.GetMillisecondsSinceEpoch() - Global.StartTime;

            return (_timeDiff / 1000.0f) % float.MaxValue;
        }

        public AnimationLayerHandler()
        {
            _ID = "Animation";
        }

        protected override UserControl CreateControl()
        {
            return new Control_AnimationLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {

            _previousAnimationTime = _currentAnimationTime;
            _currentAnimationTime = GetAnimationTime() % Properties.AnimationDuration;

            EffectLayer gradient_layer = new EffectLayer();

            if (Properties.AnimationRepeat > 0)
            {
                if (_playTimes >= Properties.AnimationRepeat)
                    return gradient_layer;

                if (_currentAnimationTime < _previousAnimationTime)
                    _playTimes++;
            }
            else
                _playTimes = 0;

            EffectLayer gradient_layer_temp = new EffectLayer();

            using (Graphics g = gradient_layer_temp.GetGraphics())
            {
                Properties.AnimationMix.Draw(g, _currentAnimationTime);
            }

            Rectangle rect = new Rectangle(0, 0, Effects.canvas_width, Effects.canvas_height);

            if (Properties.ScaleToKeySequenceBounds)
            {
                var region = Properties.Sequence.GetAffectedRegion();
                rect = new Rectangle((int)region.X, (int)region.Y, (int)region.Width, (int)region.Height);
            }

            using (Graphics g = gradient_layer.GetGraphics())
            {
                g.DrawImage(gradient_layer_temp.GetBitmap(), rect, new Rectangle(0, 0, Effects.canvas_width, Effects.canvas_height), GraphicsUnit.Pixel);
            }

            gradient_layer_temp.Dispose();

            if (Properties.ForceKeySequence)
                gradient_layer.OnlyInclude(Properties.Sequence);

            return gradient_layer;
        }
    }
}
