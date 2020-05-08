using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public class ImageLayerHandlerProperties : LayerHandlerProperties2Color<ImageLayerHandlerProperties>
    {
        public string _ImagePath { get; set; }

        [JsonIgnore]
        public string ImagePath { get { return Logic._ImagePath ?? _ImagePath; } }

        public ImageLayerHandlerProperties() : base() { }

        public ImageLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._ImagePath = "";
        }
    }

    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("_SecondaryColor")]
    public class ImageLayerHandler : LayerHandler<ImageLayerHandlerProperties>
    {
        private EffectLayer temp_layer;
        private System.Drawing.Image _loaded_image = null;
        private string _loaded_image_path = "";

        protected override UserControl CreateControl()
        {
            return new Control_ImageLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer image_layer = new EffectLayer();

            if (!String.IsNullOrWhiteSpace(Properties.ImagePath))
            {

                if (!_loaded_image_path.Equals(Properties.ImagePath))
                {
                    //Not loaded, load it!
                    if (!File.Exists(Properties.ImagePath))
                        throw new FileNotFoundException("Could not find file specified for layer: " + Properties.ImagePath);

                    _loaded_image = new Bitmap(Properties.ImagePath);
                    _loaded_image_path = Properties.ImagePath;

                    if (Properties.ImagePath.EndsWith(".gif") && ImageAnimator.CanAnimate(_loaded_image))
                        ImageAnimator.Animate(_loaded_image, null);
                }

                if (Properties.ImagePath.EndsWith(".gif") && ImageAnimator.CanAnimate(_loaded_image))
                    ImageAnimator.UpdateFrames(_loaded_image);

                temp_layer = new EffectLayer("Temp Image Render");

                if (Properties.Sequence.type == KeySequenceType.Sequence)
                {
                    using (Graphics g = temp_layer.GetGraphics())
                    {
                        g.DrawImage(_loaded_image, new RectangleF(0, 0, Effects.canvas_width, Effects.canvas_height), new RectangleF(0, 0, _loaded_image.Width, _loaded_image.Height), GraphicsUnit.Pixel);
                    }

                    foreach (var key in Properties.Sequence.keys)
                        image_layer.Set(key, Utils.ColorUtils.AddColors(image_layer.Get(key), temp_layer.Get(key)));
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

                    using (Graphics g = temp_layer.GetGraphics())
                    {
                        g.DrawImage(_loaded_image, rect, new RectangleF(0, 0, _loaded_image.Width, _loaded_image.Height), GraphicsUnit.Pixel);
                    }

                    using (Graphics g = image_layer.GetGraphics())
                    {
                        PointF rotatePoint = new PointF(x_pos + (width / 2.0f), y_pos + (height / 2.0f));

                        Matrix myMatrix = new Matrix();
                        myMatrix.RotateAt(Properties.Sequence.freeform.Angle, rotatePoint, MatrixOrder.Append);

                        g.Transform = myMatrix;
                        g.DrawImage(temp_layer.GetBitmap(), rect, rect, GraphicsUnit.Pixel);
                    }
                }
            }

            return image_layer;
        }
    }
}
