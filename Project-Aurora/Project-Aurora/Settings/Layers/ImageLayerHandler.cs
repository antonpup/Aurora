using Aurora.Devices.Layout;
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
using Canvas = Aurora.Devices.Layout.Canvas;

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

        public ImageLayerHandler()
        {
            _ID = "Image";
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
                    Canvas g = temp_layer.GetCanvas();
                    g.DrawImage(_loaded_image, new Rectangle(0, 0, GlobalDeviceLayout.Instance.CanvasWidth, GlobalDeviceLayout.Instance.CanvasHeight), new Rectangle(0, 0, _loaded_image.Width, _loaded_image.Height), GraphicsUnit.Pixel);

                    foreach (var key in Properties.Sequence.keys)
                        image_layer.Set(key, Utils.ColorUtils.AddColors(image_layer.Get(key), temp_layer.Get(key)));
                }
                else
                {
                    Rectangle rect = Properties.Sequence.freeform.RectangleBitmap;

                    Canvas g = temp_layer.GetCanvas();
                    g.DrawImage(_loaded_image, rect, new Rectangle(0, 0, _loaded_image.Width, _loaded_image.Height), GraphicsUnit.Pixel);

                    g = image_layer.GetCanvas();
                    PointF rotatePoint = new PointF(rect.X + (rect.Width / 2.0f), rect.Y + (rect.Height / 2.0f));

                    Matrix myMatrix = new Matrix();
                    myMatrix.RotateAt(Properties.Sequence.freeform.Angle, rotatePoint, MatrixOrder.Append);

                    g.DrawImage(temp_layer.GetCanvas(), rect, rect, GraphicsUnit.Pixel, myMatrix);
                }
            }

            return image_layer;
        }
    }
}
