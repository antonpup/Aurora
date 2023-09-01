using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Controls;
using Aurora.Settings.Layers.Controls;

namespace Aurora.Settings.Layers
{
    public class ImageLayerHandlerProperties : LayerHandlerProperties2Color<ImageLayerHandlerProperties>
    {
        public string _ImagePath { get; set; }

        [JsonIgnore]
        public string ImagePath { get { return Logic._ImagePath ?? _ImagePath; } }

        public ImageLayerHandlerProperties()
        { }

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

        public ImageLayerHandler(): base("ImageLayer")
        {
        }

        protected override UserControl CreateControl()
        {
            return new Control_ImageLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            var image_layer = new EffectLayer("ImageLayer");

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

                if (Properties.Sequence.Type == KeySequenceType.Sequence)
                {
                    using (var g = temp_layer.GetGraphics())
                    {
                        g.DrawImage(_loaded_image, new RectangleF(0, 0, Effects.CanvasWidth, Effects.CanvasHeight), new RectangleF(0, 0, _loaded_image.Width, _loaded_image.Height), GraphicsUnit.Pixel);
                    }

                    foreach (var key in Properties.Sequence.Keys)
                        image_layer.Set(key, Utils.ColorUtils.AddColors(image_layer.Get(key), temp_layer.Get(key)));
                }
                else
                {
                    var x_pos = (float)Math.Round((Properties.Sequence.Freeform.X + Effects.GridBaselineX) * Effects.EditorToCanvasWidth);
                    var y_pos = (float)Math.Round((Properties.Sequence.Freeform.Y + Effects.GridBaselineY) * Effects.EditorToCanvasHeight);
                    var width = (float)Math.Round((double)(Properties.Sequence.Freeform.Width * Effects.EditorToCanvasWidth));
                    var height = (float)Math.Round((double)(Properties.Sequence.Freeform.Height * Effects.EditorToCanvasHeight));

                    if (width < 3) width = 3;
                    if (height < 3) height = 3;

                    var rect = new Rectangle((int)x_pos, (int)y_pos, (int)width, (int)height);

                    using (var g = temp_layer.GetGraphics())
                    {
                        g.DrawImage(_loaded_image, rect, new RectangleF(0, 0, _loaded_image.Width, _loaded_image.Height), GraphicsUnit.Pixel);
                    }

                    using (var g = image_layer.GetGraphics())
                    {
                        var rotatePoint = new PointF(x_pos + (width / 2.0f), y_pos + (height / 2.0f));

                        var myMatrix = new Matrix();
                        myMatrix.RotateAt(Properties.Sequence.Freeform.Angle, rotatePoint, MatrixOrder.Append);

                        g.Transform = myMatrix;
                        g.DrawImage(temp_layer.GetBitmap(), rect, rect, GraphicsUnit.Pixel);
                    }
                }
            }

            return image_layer;
        }
    }
}
