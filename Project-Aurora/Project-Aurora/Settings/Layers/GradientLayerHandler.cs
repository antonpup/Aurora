using Aurora.EffectsEngine;
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
    public class GradientLayerHandlerProperties : LayerHandlerProperties2Color<GradientLayerHandlerProperties>
    {
        [Overrides.LogicOverridable("Gradient")]
        public LayerEffectConfig _GradientConfig { get; set; }

        [JsonIgnore]
        public LayerEffectConfig GradientConfig { get { return Logic._GradientConfig ?? _GradientConfig; } }

        public GradientLayerHandlerProperties() : base() { }

        public GradientLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._GradientConfig = new LayerEffectConfig(Utils.ColorUtils.GenerateRandomColor(), Utils.ColorUtils.GenerateRandomColor()).SetAnimationType(AnimationType.None);
        }
    }

    public class GradientLayerHandler : LayerHandler<GradientLayerHandlerProperties>
    {
        private EffectLayer temp_layer;

        public GradientLayerHandler()
        {
            _ID = "Gradient";
        }

        protected override UserControl CreateControl()
        {
            return new Control_GradientLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer gradient_layer = new EffectLayer();

            if (Properties.Sequence.type == KeySequenceType.Sequence)
            {
                temp_layer = new EffectLayer("Color Zone Effect", LayerEffects.GradientShift_Custom_Angle, Properties.GradientConfig);

                foreach (var key in Properties.Sequence.keys)
                    gradient_layer.Set(key, Utils.ColorUtils.AddColors(gradient_layer.Get(key), temp_layer.Get(key)));
            }
            else
            {
                float x_pos = (float)Math.Round((Properties.Sequence.freeform.X + Effects.grid_baseline_x) * Effects.editor_to_canvas_width);
                float y_pos = (float)Math.Round((Properties.Sequence.freeform.Y + Effects.grid_baseline_y) * Effects.editor_to_canvas_height);
                float width = (float)Math.Round((double)(Properties.Sequence.freeform.Width * Effects.editor_to_canvas_width));
                float height = (float)Math.Round((double)(Properties.Sequence.freeform.Height * Effects.editor_to_canvas_height));

                if (width < 3) width = 3;
                if (height < 3) height = 3;

                Rectangle rect = new Rectangle((int)x_pos, (int)y_pos, (int)width, (int)height);

                temp_layer = new EffectLayer("Color Zone Effect", LayerEffects.GradientShift_Custom_Angle, Properties.GradientConfig, rect);

                using (Graphics g = gradient_layer.GetGraphics())
                {
                    PointF rotatePoint = new PointF(x_pos + (width / 2.0f), y_pos + (height / 2.0f));

                    Matrix myMatrix = new Matrix();
                    myMatrix.RotateAt(Properties.Sequence.freeform.Angle, rotatePoint, MatrixOrder.Append);

                    g.Transform = myMatrix;
                    g.DrawImage(temp_layer.GetBitmap(), rect, rect, GraphicsUnit.Pixel);
                }
            }

            return gradient_layer;
        }
    }
}
