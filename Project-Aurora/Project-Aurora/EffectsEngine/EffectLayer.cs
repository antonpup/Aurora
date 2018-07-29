using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

namespace Aurora.EffectsEngine
{
    /// <summary>
    /// A class representing a bitmap layer for effects
    /// </summary>
    public class EffectLayer : IDisposable
    {
        private String name;
        private Bitmap colormap;

        private object bufferLock = new object();

        private bool needsRender = false;

        Color peripheral;

        private static Devices.DeviceKeys[] possible_peripheral_keys = {
                Devices.DeviceKeys.Peripheral,
                Devices.DeviceKeys.Peripheral_FrontLight,
                Devices.DeviceKeys.Peripheral_ScrollWheel,
                Devices.DeviceKeys.Peripheral_Logo
            };

        static private ColorSpectrum rainbow = new ColorSpectrum(ColorSpectrum.RainbowLoop);

        /// <summary>
        /// Creates a new instance of the EffectLayer class with default parameters.
        /// </summary>
        public EffectLayer()
        {
            name = "Effect Layer";
            colormap = new Bitmap(Effects.canvas_width, Effects.canvas_height);
            peripheral = Color.FromArgb(0, 0, 0, 0);

            Fill(Color.FromArgb(0, 0, 0, 0));
        }

        /// <summary>
        ///  A copy constructor, Creates a new instance of the EffectLayer class from another EffectLayer instance.
        /// </summary>
        /// <param name="another_layer">EffectLayer instance to copy data from</param>
        public EffectLayer(EffectLayer another_layer)
        {
            this.name = another_layer.name;
            colormap = new Bitmap(another_layer.colormap);
            peripheral = another_layer.peripheral;

            needsRender = another_layer.needsRender;
        }

        /// <summary>
        /// Creates a new instance of the EffectLayer class with a specified layer name.
        /// </summary>
        /// <param name="name">A layer name</param>
        public EffectLayer(string name)
        {
            this.name = name;
            colormap = new Bitmap(Effects.canvas_width, Effects.canvas_height);
            peripheral = Color.FromArgb(0, 0, 0, 0);

            Fill(Color.FromArgb(0, 0, 0, 0));
        }

        /// <summary>
        /// Creates a new instance of the EffectLayer class with a specified layer name. And fills the layer bitmap with a specified color.
        /// </summary>
        /// <param name="name">A layer name</param>
        /// <param name="color">A color to fill the bitmap with</param>
        public EffectLayer(string name, Color color)
        {
            this.name = name;
            colormap = new Bitmap(Effects.canvas_width, Effects.canvas_height);
            peripheral = color;

            Fill(color);
        }

        /// <summary>
        /// Creates a new instance of the EffectLayer class with a specified layer name. And applies a LayerEffect onto this EffectLayer instance.
        /// Using the parameters from LayerEffectConfig and a specified region in RectangleF
        /// </summary>
        /// <param name="name">A layer name</param>
        /// <param name="effect">An enum specifying which LayerEffect to apply</param>
        /// <param name="effect_config">Configurations for the LayerEffect</param>
        /// <param name="rect">A rectangle specifying what region to apply effects in</param>
        public EffectLayer(string name, LayerEffects effect, LayerEffectConfig effect_config, RectangleF rect = new RectangleF())
        {
            this.name = name;
            colormap = new Bitmap(Effects.canvas_width, Effects.canvas_height);
            peripheral = new Color();
            Brush brush;
            float shift = 0.0f;

            switch (effect)
            {
                case LayerEffects.ColorOverlay:
                    Fill(effect_config.primary);
                    break;
                case LayerEffects.ColorBreathing:
                    Fill(effect_config.primary);
                    float sine = (float)Math.Pow(Math.Sin((double)((Utils.Time.GetMillisecondsSinceEpoch() % 10000L) / 10000.0f) * 2 * Math.PI * effect_config.speed), 2);
                    Fill(Color.FromArgb((byte)(sine * 255), effect_config.secondary));
                    break;
                case LayerEffects.RainbowShift_Horizontal:
                    effect_config.shift_amount += ((Utils.Time.GetMillisecondsSinceEpoch() - effect_config.last_effect_call) / 1000.0f) * 5.0f * effect_config.speed;
                    effect_config.shift_amount = effect_config.shift_amount % Effects.canvas_biggest;

                    if (effect_config.animation_type == AnimationType.Translate_XY)
                        shift = effect_config.shift_amount;

                    if (effect_config.animation_reverse)
                        shift *= -1.0f;

                    brush = CreateRainbowBrush();
                    (brush as LinearGradientBrush).RotateTransform(0.0f);
                    (brush as LinearGradientBrush).TranslateTransform(shift, shift);

                    Fill(brush);

                    effect_config.last_effect_call = Utils.Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.RainbowShift_Vertical:
                    effect_config.shift_amount += ((Utils.Time.GetMillisecondsSinceEpoch() - effect_config.last_effect_call) / 1000.0f) * 5.0f * effect_config.speed;
                    effect_config.shift_amount = effect_config.shift_amount % Effects.canvas_biggest;

                    if (effect_config.animation_type == AnimationType.Translate_XY)
                        shift = effect_config.shift_amount;

                    if (effect_config.animation_reverse)
                        shift *= -1.0f;

                    brush = CreateRainbowBrush();
                    (brush as LinearGradientBrush).RotateTransform(90.0f);
                    (brush as LinearGradientBrush).TranslateTransform(shift, shift);

                    Fill(brush);

                    effect_config.last_effect_call = Utils.Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.RainbowShift_Diagonal:
                    effect_config.shift_amount += ((Utils.Time.GetMillisecondsSinceEpoch() - effect_config.last_effect_call) / 1000.0f) * 5.0f * effect_config.speed;
                    effect_config.shift_amount = effect_config.shift_amount % Effects.canvas_biggest;

                    if (effect_config.animation_type == AnimationType.Translate_XY)
                        shift = effect_config.shift_amount;

                    if (effect_config.animation_reverse)
                        shift *= -1.0f;

                    brush = CreateRainbowBrush();
                    (brush as LinearGradientBrush).RotateTransform(45.0f);
                    (brush as LinearGradientBrush).TranslateTransform(shift, shift);

                    Fill(brush);

                    effect_config.last_effect_call = Utils.Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.RainbowShift_Diagonal_Other:
                    effect_config.shift_amount += ((Utils.Time.GetMillisecondsSinceEpoch() - effect_config.last_effect_call) / 1000.0f) * 5.0f * effect_config.speed;
                    effect_config.shift_amount = effect_config.shift_amount % Effects.canvas_biggest;

                    if (effect_config.animation_type == AnimationType.Translate_XY)
                        shift = effect_config.shift_amount;

                    if (effect_config.animation_reverse)
                        shift *= -1.0f;

                    brush = CreateRainbowBrush();
                    (brush as LinearGradientBrush).RotateTransform(-45.0f);
                    (brush as LinearGradientBrush).TranslateTransform(shift, shift);

                    Fill(brush);

                    effect_config.last_effect_call = Utils.Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.RainbowShift_Custom_Angle:
                    effect_config.shift_amount += ((Utils.Time.GetMillisecondsSinceEpoch() - effect_config.last_effect_call) / 1000.0f) * 5.0f * effect_config.speed;
                    effect_config.shift_amount = effect_config.shift_amount % Effects.canvas_biggest;


                    if (effect_config.animation_type == AnimationType.Translate_XY)
                        shift = effect_config.shift_amount;

                    if (effect_config.animation_reverse)
                        shift *= -1.0f;

                    brush = CreateRainbowBrush();
                    (brush as LinearGradientBrush).RotateTransform(effect_config.angle);
                    (brush as LinearGradientBrush).TranslateTransform(shift, shift);

                    Fill(brush);

                    effect_config.last_effect_call = Utils.Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.GradientShift_Custom_Angle:
                    effect_config.shift_amount += ((Utils.Time.GetMillisecondsSinceEpoch() - effect_config.last_effect_call) / 1000.0f) * 0.067f * effect_config.speed;
                    effect_config.shift_amount = effect_config.shift_amount % Effects.canvas_biggest;

                    if (effect_config.animation_type == AnimationType.Translate_XY)
                        shift = effect_config.shift_amount;
                    else if (effect_config.animation_type == AnimationType.Zoom_in && effect_config.brush.type == EffectBrush.BrushType.Radial)
                        shift = ((Effects.canvas_biggest - effect_config.shift_amount) * 40.0f) % Effects.canvas_biggest;
                    else if (effect_config.animation_type == AnimationType.Zoom_out && effect_config.brush.type == EffectBrush.BrushType.Radial)
                        shift = (effect_config.shift_amount * 40.0f) % Effects.canvas_biggest;

                    if (effect_config.animation_reverse)
                        shift *= -1.0f;

                    brush = effect_config.brush.GetDrawingBrush();
                    if (effect_config.brush.type == EffectBrush.BrushType.Linear)
                    {
                        if (!rect.IsEmpty)
                        {
                            (brush as LinearGradientBrush).TranslateTransform(rect.X, rect.Y);
                            (brush as LinearGradientBrush).ScaleTransform(rect.Width, rect.Height);
                        }
                        else
                        {
                            (brush as LinearGradientBrush).ScaleTransform(Effects.canvas_height, Effects.canvas_height);
                        }

                        (brush as LinearGradientBrush).RotateTransform(effect_config.angle);
                        (brush as LinearGradientBrush).TranslateTransform(shift, shift);
                    }
                    else if (effect_config.brush.type == EffectBrush.BrushType.Radial)
                    {
                        if (effect_config.animation_type == AnimationType.Zoom_in || effect_config.animation_type == AnimationType.Zoom_out)
                        {
                            float percent = shift / Effects.canvas_biggest;

                            float x_offset = (Effects.canvas_width / 2.0f) * percent;
                            float y_offset = (Effects.canvas_height / 2.0f) * percent;


                            (brush as PathGradientBrush).WrapMode = WrapMode.Clamp;

                            if (!rect.IsEmpty)
                            {
                                x_offset = (rect.Width / 2.0f) * percent;
                                y_offset = (rect.Height / 2.0f) * percent;

                                (brush as PathGradientBrush).TranslateTransform(rect.X + x_offset, rect.Y + y_offset);
                                (brush as PathGradientBrush).ScaleTransform(rect.Width - (2.0f * x_offset), rect.Height - (2.0f * y_offset));
                            }
                            else
                            {
                                (brush as PathGradientBrush).ScaleTransform(Effects.canvas_height + x_offset, Effects.canvas_height + y_offset);
                            }
                        }
                        else
                        {
                            if (!rect.IsEmpty)
                            {
                                (brush as PathGradientBrush).TranslateTransform(rect.X, rect.Y);
                                (brush as PathGradientBrush).ScaleTransform(rect.Width, rect.Height);
                            }
                            else
                            {
                                (brush as PathGradientBrush).ScaleTransform(Effects.canvas_height, Effects.canvas_height);
                            }
                        }

                        (brush as PathGradientBrush).RotateTransform(effect_config.angle);

                        //(brush as PathGradientBrush).TranslateTransform(x_shift, y_shift);
                    }

                    Fill(brush);

                    effect_config.last_effect_call = Utils.Time.GetMillisecondsSinceEpoch();
                    break;
                default:
                    break;
            }
        }

        public void Dispose()
        {
            colormap.Dispose();
            colormap = null;
        }

        /// <summary>
        /// Creates a rainbow gradient brush, to be used in effects.
        /// </summary>
        /// <returns>Rainbow LinearGradientBrush</returns>
        private LinearGradientBrush CreateRainbowBrush()
        {
            LinearGradientBrush the_brush =
                        new LinearGradientBrush(
                            new Point(0, 0),
                            new Point(Effects.canvas_biggest, 0),
                            Color.Red, Color.Red);
            Color[] colors = new Color[]
            {
                            Color.FromArgb(255, 0, 0),
                            Color.FromArgb(255, 127, 0),
                            Color.FromArgb(255, 255, 0),
                            Color.FromArgb(0, 255, 0),
                            Color.FromArgb(0, 0, 255),
                            Color.FromArgb(75, 0, 130),
                            Color.FromArgb(139, 0, 255),
                            Color.FromArgb(255, 0, 0)
            };
            int num_colors = colors.Length;
            float[] blend_positions = new float[num_colors];
            for (int i = 0; i < num_colors; i++)
            {
                blend_positions[i] = i / (num_colors - 1f);
            }

            ColorBlend color_blend = new ColorBlend();
            color_blend.Colors = colors;
            color_blend.Positions = blend_positions;
            the_brush.InterpolationColors = color_blend;

            return the_brush;
        }

        /// <summary>
        /// Fills the entire bitmap of the EffectLayer with a specified brush.
        /// </summary>
        /// <param name="brush">Brush to be used during bitmap fill</param>
        /// <returns>Itself</returns>
        public EffectLayer Fill(Brush brush)
        {
            using (Graphics g = Graphics.FromImage(colormap))
            {
                /*if(brush is PathGradientBrush)
                {
                    GraphicsPath gp = new GraphicsPath();
                    gp.AddEllipse((brush as PathGradientBrush).Rectangle);

                    g.FillPath(brush, gp);
                }
                else
                {
                    Rectangle rect = new Rectangle(0, 0, colormap.Width, colormap.Height);
                    g.FillRectangle(brush, rect);
                }
                */
                Rectangle rect = new Rectangle(0, 0, colormap.Width, colormap.Height);
                g.FillRectangle(brush, rect);
                needsRender = true;
            }

            return this;
        }

        /// <summary>
        /// Fills the entire bitmap of the EffectLayer with a specified color.
        /// </summary>
        /// <param name="color">Color to be used during bitmap fill</param>
        /// <returns>Itself</returns>
        public EffectLayer Fill(Color color)
        {
            using (Graphics g = Graphics.FromImage(colormap))
            {
                Rectangle rect = new Rectangle(0, 0, colormap.Width, colormap.Height);
                g.FillRectangle(new SolidBrush(color), rect);
                needsRender = true;
            }

            return this;
        }

        /// <summary>
        /// Sets a specific coordinate on the bitmap with a specified color.
        /// </summary>
        /// <param name="x">X Coordinate on the bitmap</param>
        /// <param name="y">Y Coordinate on the bitmap</param>
        /// <param name="color">Color to be used</param>
        /// <returns>Itself</returns>
        public EffectLayer Set(int x, int y, Color color)
        {
            BitmapData srcData = colormap.LockBits(
                    new Rectangle(x, y, 1, 1),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            IntPtr Scan0 = srcData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                p[0] = color.B;
                p[1] = color.G;
                p[2] = color.R;
                p[3] = color.A;
            }

            colormap.UnlockBits(srcData);

            needsRender = true;

            return this;
        }

        /// <summary>
        /// Sets a specific Devices.DeviceKeys on the bitmap with a specified color.
        /// </summary>
        /// <param name="key">DeviceKey to be set</param>
        /// <param name="color">Color to be used</param>
        /// <returns>Itself</returns>
        public EffectLayer Set(Devices.DeviceKeys key, Color color)
        {
            SetOneKey(key, color);

            return this;
        }

        /// <summary>
        /// Sets a specific Devices.DeviceKeys on the bitmap with a specified color.
        /// </summary>
        /// <param name="keys">Array of DeviceKeys to be set</param>
        /// <param name="color">Color to be used</param>
        /// <returns>Itself</returns>
        public EffectLayer Set(Devices.DeviceKeys[] keys, Color color)
        {
            foreach(var key in keys)
                SetOneKey(key, color);

            return this;
        }

        /// <summary>
        /// Sets a specific KeySequence on the bitmap with a specified color.
        /// </summary>
        /// <param name="sequence">KeySequence to specify what regions of the bitmap need to be changed</param>
        /// <param name="color">Color to be used</param>
        /// <returns>Itself</returns>
        public EffectLayer Set(KeySequence sequence, Color color)
        {
            if (sequence.type == KeySequenceType.Sequence)
            {
                foreach (var key in sequence.keys)
                    Set(key, color);
            }
            else
            {
                using (Graphics g = Graphics.FromImage(colormap))
                {
                    float x_pos = (float)Math.Round((sequence.freeform.X + Effects.grid_baseline_x) * Effects.editor_to_canvas_width);
                    float y_pos = (float)Math.Round((sequence.freeform.Y + Effects.grid_baseline_y) * Effects.editor_to_canvas_height);
                    float width = (float)(sequence.freeform.Width * Effects.editor_to_canvas_width);
                    float height = (float)(sequence.freeform.Height * Effects.editor_to_canvas_height);

                    if (width < 3) width = 3;
                    if (height < 3) height = 3;

                    Rectangle rect = new Rectangle((int)x_pos, (int)y_pos, (int)width, (int)height);

                    PointF rotatePoint = new PointF(x_pos + (width / 2.0f), y_pos + (height / 2.0f));

                    Matrix myMatrix = new Matrix();
                    myMatrix.RotateAt(sequence.freeform.Angle, rotatePoint, MatrixOrder.Append);

                    g.Transform = myMatrix;
                    g.FillRectangle(new SolidBrush(color), rect);
                }
            }

            return this;
        }

        /// <summary>
        /// Sets one DeviceKeys key with a specific color on the bitmap
        /// </summary>
        /// <param name="key">DeviceKey to be set</param>
        /// <param name="color">Color to be used</param>
        /// <returns>Itself</returns>
        private EffectLayer SetOneKey(Devices.DeviceKeys key, Color color)
        {
            BitmapRectangle keymaping = Effects.GetBitmappingFromDeviceKey(key);

            if (key == Devices.DeviceKeys.Peripheral)
            {
                peripheral = color;
                using (Graphics g = Graphics.FromImage(colormap))
                {
                    foreach (Devices.DeviceKeys peri_key in possible_peripheral_keys)
                    {
                        BitmapRectangle peri_keymaping = Effects.GetBitmappingFromDeviceKey(peri_key);

                        if (peri_keymaping.IsValid)
                            g.FillRectangle(new SolidBrush(color), peri_keymaping.Rectangle);
                    }

                    needsRender = true;
                }
            }
            else
            {
                if (keymaping.Top < 0 || keymaping.Bottom > Effects.canvas_height ||
                    keymaping.Left < 0 || keymaping.Right > Effects.canvas_width)
                {
                    Global.logger.Warn("Coudln't set key color " + key.ToString());
                    return this; ;
                }
                else
                {
                    using (Graphics g = Graphics.FromImage(colormap))
                    {
                        g.FillRectangle(new SolidBrush(color), keymaping.Rectangle);
                        needsRender = true;
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// Retrieves a color of the specified X and Y coordinate on the bitmap
        /// </summary>
        /// <param name="x">X Coordiante on the bitmap</param>
        /// <param name="y">Y Coordinate on the bitmap</param>
        /// <returns>Color at (X,Y)</returns>
        public Color Get(int x, int y)
        {
            BitmapData srcData = colormap.LockBits(
                    new Rectangle(x, y, 1, 1),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            IntPtr Scan0 = srcData.Scan0;

            byte red, green, blue, alpha;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                blue = p[0];
                green = p[1];
                red = p[2];
                alpha = p[3];
            }

            colormap.UnlockBits(srcData);

            return Color.FromArgb(alpha, red, green, blue);
        }

        /// <summary>
        /// Retrieves a color of the specified DeviceKeys key from the bitmap
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Color of the Key</returns>
        public Color Get(Devices.DeviceKeys key)
        {
            try
            {
                BitmapRectangle keymaping = Effects.GetBitmappingFromDeviceKey(key);

                if (keymaping.IsEmpty && key == Devices.DeviceKeys.Peripheral)
                {
                    return peripheral;
                }
                else
                {
                    if (keymaping.IsEmpty)
                        return Color.FromArgb(0, 0, 0);

                    return Utils.BitmapUtils.GetRegionColor(colormap, keymaping.Rectangle);
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error("EffectLayer.Get() Exception: " + exc);

                return Color.FromArgb(0, 0, 0);
            }
        }

        /// <summary>
        /// Get an instance of Drawing.Graphics, to allow drawing on the bitmap.
        /// </summary>
        /// <returns>Graphics instance</returns>
        public Graphics GetGraphics()
        {
            return Graphics.FromImage(colormap);
        }

        /// <summary>
        /// Get the current layer bitmap.
        /// </summary>
        /// <returns>Layer Bitmap</returns>
        public Bitmap GetBitmap()
        {
            return colormap;
        }

        /// <summary>
        /// + Operator, sums two EffectLayer together.
        /// </summary>
        /// <param name="lhs">Left Hand Side EffectLayer</param>
        /// <param name="rhs">Right Hand Side EffectLayer</param>
        /// <returns>A new instance of EffectLayer, which is a combination of two passed EffectLayers</returns>
        public static EffectLayer operator +(EffectLayer lhs, EffectLayer rhs)
        {
            EffectLayer added = new EffectLayer(lhs);
            added.name += " + " + rhs.name;

            using (Graphics g = added.GetGraphics())
            {
                lock (rhs.bufferLock)
                    g.DrawImage(rhs.colormap, 0, 0);
            }

            added.peripheral = Utils.ColorUtils.AddColors(lhs.peripheral, rhs.peripheral);

            return added;
        }

        /// <summary>
        /// * Operator, Multiplies an EffectLayer by a double, adjusting opacity and color of the layer bitmap.
        /// </summary>
        /// <param name="layer">EffectLayer to be adjusted</param>
        /// <param name="value">Double value that each bit in the bitmap will be multiplied by</param>
        /// <returns>The passed instance of EffectLayer with adjustments</returns>
        public static EffectLayer operator *(EffectLayer layer, double value)
        {
            BitmapData srcData = layer.colormap.LockBits(
            new Rectangle(0, 0, layer.colormap.Width, layer.colormap.Height),
            ImageLockMode.ReadWrite,
            PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            IntPtr Scan0 = srcData.Scan0;

            int width = layer.colormap.Width;
            int height = layer.colormap.Height;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        //p[(y * stride) + x * 4] = Utils.ColorUtils.ColorByteMultiplication(p[(y * stride) + x * 4], value);
                        //p[(y * stride) + x * 4 + 1] = Utils.ColorUtils.ColorByteMultiplication(p[(y * stride) + x * 4 + 1], value);
                        //p[(y * stride) + x * 4 + 2] = Utils.ColorUtils.ColorByteMultiplication(p[(y * stride) + x * 4 + 2], value);
                        p[(y * stride) + x * 4 + 3] = Utils.ColorUtils.ColorByteMultiplication(p[(y * stride) + x * 4 + 3], value);
                    }
                }
            }

            layer.colormap.UnlockBits(srcData);

            layer.peripheral = Utils.ColorUtils.MultiplyColorByScalar(layer.peripheral, value);

            return layer;
        }

        /// <summary>
        /// Draws a percent effect on the layer bitmap using a KeySequence with solid colors.
        /// </summary>
        /// <param name="foregroundColor">The foreground color, used as a "Progress bar color"</param>
        /// <param name="backgroundColor">The background color</param>
        /// <param name="sequence">The sequence of keys that the percent effect will be drawn on</param>
        /// <param name="value">The current progress value</param>
        /// <param name="total">The maxiumum progress value</param>
        /// <param name="percentEffectType">The percent effect type</param>
        /// <returns>Itself</returns>
        public EffectLayer PercentEffect(Color foregroundColor, Color backgroundColor, Settings.KeySequence sequence, double value, double total = 1.0D, PercentEffectType percentEffectType = PercentEffectType.Progressive, double flash_past = 0.0, bool flash_reversed = false, bool blink_background = false)
        {
            if (sequence.type == KeySequenceType.Sequence)
                PercentEffect(foregroundColor, backgroundColor, sequence.keys.ToArray(), value, total, percentEffectType, flash_past, flash_reversed, blink_background);
            else
                PercentEffect(foregroundColor, backgroundColor, sequence.freeform, value, total, percentEffectType, flash_past, flash_reversed);

            return this;
        }

        /// <summary>
        /// Draws a percent effect on the layer bitmap using a KeySequence and a ColorSpectrum.
        /// </summary>
        /// <param name="spectrum">The ColorSpectrum to be used as a "Progress bar"</param>
        /// <param name="sequence">The sequence of keys that the percent effect will be drawn on</param>
        /// <param name="value">The current progress value</param>
        /// <param name="total">The maxiumum progress value</param>
        /// <param name="percentEffectType">The percent effect type</param>
        /// <returns>Itself</returns>
        public EffectLayer PercentEffect(ColorSpectrum spectrum, Settings.KeySequence sequence, double value, double total = 1.0D, PercentEffectType percentEffectType = PercentEffectType.Progressive, double flash_past = 0.0, bool flash_reversed = false, bool blink_background = false)
        {
            if (sequence.type == KeySequenceType.Sequence)
                PercentEffect(spectrum, sequence.keys.ToArray(), value, total, percentEffectType, flash_past, flash_reversed);
            else
                PercentEffect(spectrum, sequence.freeform, value, total, percentEffectType, flash_past, flash_reversed);

            return this;
        }

        /// <summary>
        /// Draws a percent effect on the layer bitmap using an array of DeviceKeys keys and solid colors.
        /// </summary>
        /// <param name="foregroundColor">The foreground color, used as a "Progress bar color"</param>
        /// <param name="backgroundColor">The background color</param>
        /// <param name="keys">The array of keys that the percent effect will be drawn on</param>
        /// <param name="value">The current progress value</param>
        /// <param name="total">The maxiumum progress value</param>
        /// <param name="percentEffectType">The percent effect type</param>
        /// <returns>Itself</returns>
        public EffectLayer PercentEffect(Color foregroundColor, Color backgroundColor, Devices.DeviceKeys[] keys, double value, double total, PercentEffectType percentEffectType = PercentEffectType.Progressive, double flash_past = 0.0, bool flash_reversed = false, bool blink_background = false)
        {
            double progress_total = value / total;
            if (progress_total < 0.0)
                progress_total = 0.0;
            else if (progress_total > 1.0)
                progress_total = 1.0;

            double progress = progress_total * keys.Count();

            if (flash_past > 0.0)
            {
                if ((flash_reversed && progress_total >= flash_past) || (!flash_reversed && progress_total <= flash_past))
                {
                    if (blink_background)
                        backgroundColor = Utils.ColorUtils.BlendColors(backgroundColor, Color.FromArgb(0, 0, 0, 0), Math.Sin((Utils.Time.GetMillisecondsSinceEpoch() % 1000.0D) / 1000.0D * Math.PI));
                    else
                        foregroundColor = Utils.ColorUtils.BlendColors(backgroundColor, foregroundColor, Math.Sin((Utils.Time.GetMillisecondsSinceEpoch() % 1000.0D) / 1000.0D * Math.PI));
                }
            }

            for (int i = 0; i < keys.Count(); i++)
            {
                Devices.DeviceKeys current_key = keys[i];

                switch (percentEffectType)
                {
                    case (PercentEffectType.AllAtOnce):
                        SetOneKey(current_key, Utils.ColorUtils.BlendColors(backgroundColor, foregroundColor, progress_total));
                        break;
                    case (PercentEffectType.Progressive_Gradual):
                        if (i == (int)progress)
                        {
                            double percent = (double)progress - i;
                            SetOneKey(current_key, Utils.ColorUtils.BlendColors(backgroundColor, foregroundColor, percent));
                        }
                        else if (i < (int)progress)
                            SetOneKey(current_key, foregroundColor);
                        else
                            SetOneKey(current_key, backgroundColor);
                        break;
                    default:
                        if (i < (int)progress)
                            SetOneKey(current_key, foregroundColor);
                        else
                            SetOneKey(current_key, backgroundColor);
                        break;
                }
            }

            return this;
        }

        /// <summary>
        /// Draws a percent effect on the layer bitmap using DeviceKeys keys and a ColorSpectrum.
        /// </summary>
        /// <param name="spectrum">The ColorSpectrum to be used as a "Progress bar"</param>
        /// <param name="keys">The array of keys that the percent effect will be drawn on</param>
        /// <param name="value">The current progress value</param>
        /// <param name="total">The maxiumum progress value</param>
        /// <param name="percentEffectType">The percent effect type</param>
        /// <returns>Itself</returns>
        public EffectLayer PercentEffect(ColorSpectrum spectrum, Devices.DeviceKeys[] keys, double value, double total, PercentEffectType percentEffectType = PercentEffectType.Progressive, double flash_past = 0.0, bool flash_reversed = false)
        {
            double progress_total = value / total;
            if (progress_total < 0.0)
                progress_total = 0.0;
            else if (progress_total > 1.0)
                progress_total = 1.0;

            double progress = progress_total * keys.Count();

            double flash_amount = 1.0;

            if (flash_past > 0.0)
            {
                if ((flash_reversed && progress_total >= flash_past) || (!flash_reversed && progress_total <= flash_past))
                {
                    flash_amount = Math.Sin((Utils.Time.GetMillisecondsSinceEpoch() % 1000.0D) / 1000.0D * Math.PI);
                }
            }

            for (int i = 0; i < keys.Count(); i++)
            {
                Devices.DeviceKeys current_key = keys[i];

                switch (percentEffectType)
                {
                    case (PercentEffectType.AllAtOnce):
                        SetOneKey(current_key, spectrum.GetColorAt((float)progress_total, 1.0f, flash_amount));
                        break;
                    case (PercentEffectType.Progressive_Gradual):
                        if (i == (int)progress)
                        {
                            double percent = (double)progress - i;
                            SetOneKey(current_key,
                                Utils.ColorUtils.MultiplyColorByScalar(spectrum.GetColorAt((float)i / (float)(keys.Count() - 1), 1.0f, flash_amount), percent)
                                );
                        }
                        else if (i < (int)progress)
                            SetOneKey(current_key, spectrum.GetColorAt((float)i / (float)(keys.Count() - 1), 1.0f, flash_amount));
                        break;
                    default:
                        if (i < (int)progress)
                            SetOneKey(current_key, spectrum.GetColorAt((float)i / (float)(keys.Count() - 1), 1.0f, flash_amount));
                        break;
                }
            }

            return this;
        }

        /// <summary>
        /// Draws a percent effect on the layer bitmap using a FreeFormObject and solid colors.
        /// </summary>
        /// <param name="foregroundColor">The foreground color, used as a "Progress bar color"</param>
        /// <param name="backgroundColor">The background color</param>
        /// <param name="freeform">The FreeFormObject that the progress effect will be drawn on</param>
        /// <param name="value">The current progress value</param>
        /// <param name="total">The maxiumum progress value</param>
        /// <param name="percentEffectType">The percent effect type</param>
        /// <returns>Itself</returns>
        public EffectLayer PercentEffect(Color foregroundColor, Color backgroundColor, Settings.FreeFormObject freeform, double value, double total, PercentEffectType percentEffectType = PercentEffectType.Progressive, double flash_past = 0.0, bool flash_reversed = false, bool blink_background = false)
        {
            double progress_total = value / total;
            if (progress_total < 0.0 || Double.IsNaN(progress_total))
                progress_total = 0.0;
            else if (progress_total > 1.0)
                progress_total = 1.0;

            if (flash_past > 0.0)
            {
                if ((flash_reversed && progress_total >= flash_past) || (!flash_reversed && progress_total <= flash_past))
                {
                    if(!blink_background)
                        foregroundColor = Utils.ColorUtils.BlendColors(backgroundColor, foregroundColor, Math.Sin((Utils.Time.GetMillisecondsSinceEpoch() % 1000.0D) / 1000.0D * Math.PI));
                    if(blink_background)
                        backgroundColor = Utils.ColorUtils.BlendColors(backgroundColor, foregroundColor, Math.Sin((Utils.Time.GetMillisecondsSinceEpoch() % 1000.0D) / 1000.0D * Math.PI));
                }
            }

            using (Graphics g = Graphics.FromImage(colormap))
            {
                float x_pos = (float)Math.Round((freeform.X + Effects.grid_baseline_x) * Effects.editor_to_canvas_width);
                float y_pos = (float)Math.Round((freeform.Y + Effects.grid_baseline_y) * Effects.editor_to_canvas_height);
                float width = (float)(freeform.Width * Effects.editor_to_canvas_width);
                float height = (float)(freeform.Height * Effects.editor_to_canvas_height);

                if (width < 3) width = 3;
                if (height < 3) height = 3;


                if (percentEffectType == PercentEffectType.AllAtOnce)
                {
                    Rectangle rect = new Rectangle((int)x_pos, (int)y_pos, (int)width, (int)height);

                    PointF rotatePoint = new PointF(x_pos + (width / 2.0f), y_pos + (height / 2.0f));

                    Matrix myMatrix = new Matrix();
                    myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

                    g.Transform = myMatrix;
                    g.FillRectangle(new SolidBrush(Utils.ColorUtils.BlendColors(backgroundColor, foregroundColor, progress_total)), rect);
                }
                else
                {
                    double progress = progress_total * width;

                    Rectangle rect = new Rectangle((int)x_pos, (int)y_pos, (int)progress, (int)height);

                    PointF rotatePoint = new PointF(x_pos + (width / 2.0f), y_pos + (height / 2.0f));

                    Matrix myMatrix = new Matrix();
                    myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

                    g.Transform = myMatrix;

                    Rectangle rect_rest = new Rectangle((int)x_pos, (int)y_pos, (int)width, (int)height);
                    g.FillRectangle(new SolidBrush(backgroundColor), rect_rest);

                    g.FillRectangle(new SolidBrush(foregroundColor), rect);
                }
            }

            return this;
        }

        /// <summary>
        ///  Draws a percent effect on the layer bitmap using a FreeFormObject and a ColorSpectrum.
        /// </summary>
        /// <param name="spectrum">The ColorSpectrum to be used as a "Progress bar"</param>
        /// <param name="freeform">The FreeFormObject that the progress effect will be drawn on</param>
        /// <param name="value">The current progress value</param>
        /// <param name="total">The maxiumum progress value</param>
        /// <param name="percentEffectType">The percent effect type</param>
        /// <returns>Itself</returns>
        public EffectLayer PercentEffect(ColorSpectrum spectrum, Settings.FreeFormObject freeform, double value, double total, PercentEffectType percentEffectType = PercentEffectType.Progressive, double flash_past = 0.0, bool flash_reversed = false)
        {
            double progress_total = value / total;
            if (progress_total < 0.0)
                progress_total = 0.0;
            else if (progress_total > 1.0)
                progress_total = 1.0;

            double flash_amount = 1.0;

            if (flash_past > 0.0)
            {
                if ((flash_reversed && progress_total >= flash_past) || (!flash_reversed && progress_total <= flash_past))
                {
                    flash_amount = Math.Sin((Utils.Time.GetMillisecondsSinceEpoch() % 1000.0D) / 1000.0D * Math.PI);
                }
            }

            using (Graphics g = Graphics.FromImage(colormap))
            {
                float x_pos = (float)Math.Round((freeform.X + Effects.grid_baseline_x) * Effects.editor_to_canvas_width);
                float y_pos = (float)Math.Round((freeform.Y + Effects.grid_baseline_y) * Effects.editor_to_canvas_height);
                float width = (float)(freeform.Width * Effects.editor_to_canvas_width);
                float height = (float)(freeform.Height * Effects.editor_to_canvas_height);

                if (width < 3) width = 3;
                if (height < 3) height = 3;

                if (percentEffectType == PercentEffectType.AllAtOnce)
                {
                    Rectangle rect = new Rectangle((int)x_pos, (int)y_pos, (int)width, (int)height);

                    PointF rotatePoint = new PointF(x_pos + (width / 2.0f), y_pos + (height / 2.0f));

                    Matrix myMatrix = new Matrix();
                    myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

                    g.Transform = myMatrix;
                    g.FillRectangle(new SolidBrush(spectrum.GetColorAt((float)progress_total, 1.0f, flash_amount)), rect);
                }
                else
                {
                    double progress = progress_total * width;

                    Rectangle rect = new Rectangle((int)x_pos, (int)y_pos, (int)progress, (int)height);

                    PointF rotatePoint = new PointF(x_pos + (width / 2.0f), y_pos + (height / 2.0f));

                    Matrix myMatrix = new Matrix();
                    myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

                    g.Transform = myMatrix;
                    LinearGradientBrush brush = spectrum.ToLinearGradient(width, 0, x_pos, 0, flash_amount);
					brush.WrapMode = WrapMode.Tile;
                    g.FillRectangle(brush, rect);
                }

                return this;
            }
        }

        /// <summary>
        /// Draws a FreeFormObject on the layer bitmap using a specified color.
        /// </summary>
        /// <param name="freeform">The FreeFormObject that will be filled with a color</param>
        /// <param name="color">The color to be used</param>
        /// <returns>Itself</returns>
        public EffectLayer DrawFreeForm(Settings.FreeFormObject freeform, Color color)
        {
            using (Graphics g = Graphics.FromImage(colormap))
            {
                float x_pos = (float)Math.Round((freeform.X + Effects.grid_baseline_x) * Effects.editor_to_canvas_width);
                float y_pos = (float)Math.Round((freeform.Y + Effects.grid_baseline_y) * Effects.editor_to_canvas_height);
                float width = (float)Math.Round(freeform.Width * Effects.editor_to_canvas_width);
                float height = (float)Math.Round(freeform.Height * Effects.editor_to_canvas_height);

                if (width < 3) width = 3;
                if (height < 3) height = 3;

                Rectangle rect = new Rectangle((int)x_pos, (int)y_pos, (int)width, (int)height);

                PointF rotatePoint = new PointF(x_pos + (width / 2.0f), y_pos + (height / 2.0f));

                Matrix myMatrix = new Matrix();
                myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

                g.Transform = myMatrix;
                g.FillRectangle(new SolidBrush(color), rect);
            }

            return this;
        }

        /// <summary>
        /// Draws ColorZones on the layer bitmap.
        /// </summary>
        /// <param name="colorzones">An array of ColorZones</param>
        /// <returns>Itself</returns>
        public EffectLayer DrawColorZones(ColorZone[] colorzones)
        {
            foreach (ColorZone cz in colorzones.Reverse())
            {
                if (cz.keysequence.type == KeySequenceType.Sequence)
                {
                    foreach (var key in cz.keysequence.keys)
                        Set(key, cz.color);

                    if (cz.effect != LayerEffects.None)
                    {
                        EffectLayer temp_layer = new EffectLayer("Color Zone Effect", cz.effect, cz.effect_config);

                        foreach (var key in cz.keysequence.keys)
                            Set(key, Utils.ColorUtils.AddColors(Get(key), temp_layer.Get(key)));

                        temp_layer.Dispose();
                    }
                }
                else
                {
                    if (cz.effect == LayerEffects.None)
                    {
                        DrawFreeForm(cz.keysequence.freeform, cz.color);
                    }
                    else
                    {
                        float x_pos = (float)Math.Round((cz.keysequence.freeform.X + Effects.grid_baseline_x) * Effects.editor_to_canvas_width);
                        float y_pos = (float)Math.Round((cz.keysequence.freeform.Y + Effects.grid_baseline_y) * Effects.editor_to_canvas_height);
                        float width = (float)Math.Round((double)(cz.keysequence.freeform.Width * Effects.editor_to_canvas_width));
                        float height = (float)Math.Round((double)(cz.keysequence.freeform.Height * Effects.editor_to_canvas_height));

                        if (width < 3) width = 3;
                        if (height < 3) height = 3;

                        Rectangle rect = new Rectangle((int)x_pos, (int)y_pos, (int)width, (int)height);

                        EffectLayer temp_layer = new EffectLayer("Color Zone Effect", cz.effect, cz.effect_config, rect);

                        using (Graphics g = Graphics.FromImage(colormap))
                        {
                            PointF rotatePoint = new PointF(x_pos + (width / 2.0f), y_pos + (height / 2.0f));

                            Matrix myMatrix = new Matrix();
                            myMatrix.RotateAt(cz.keysequence.freeform.Angle, rotatePoint, MatrixOrder.Append);

                            g.Transform = myMatrix;
                            g.DrawImage(temp_layer.GetBitmap(), rect, rect, GraphicsUnit.Pixel);
                        }

                        temp_layer.Dispose();
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// Excludes provided sequence from the layer (Applies a mask)
        /// </summary>
        /// <param name="sequence">The mask to be applied</param>
        /// <returns>Itself</returns>
        public EffectLayer Exclude(KeySequence sequence)
        {
            //Create draw alpha mask
            EffectLayer _alpha_mask = new EffectLayer(this.name + " - Alpha Mask", Color.Transparent);
            _alpha_mask.Set(sequence, Color.Black);

            //Apply alpha mask
            BitmapData srcData_alpha = _alpha_mask.colormap.LockBits(
                new Rectangle(0, 0, _alpha_mask.colormap.Width, _alpha_mask.colormap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            int alpha_mask_stride = srcData_alpha.Stride;
            IntPtr alpha_mask_Scan0 = srcData_alpha.Scan0;

            
            BitmapData srcData = colormap.LockBits(
                new Rectangle(0, 0, colormap.Width, colormap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            IntPtr Scan0 = srcData.Scan0;

            int width = colormap.Width;
            int height = colormap.Height;

            unsafe
            {
                byte* p_alpha = (byte*)(void*)alpha_mask_Scan0;
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte mask_alpha = p_alpha[(y * alpha_mask_stride) + x * 4 + 3];
                        if (mask_alpha != 0)
                            p[(y * stride) + x * 4 + 3] = (byte)(255 - mask_alpha);
                    }
                }
            }

            _alpha_mask.colormap.UnlockBits(srcData_alpha);
            colormap.UnlockBits(srcData);

            return this;
        }

        /// <summary>
        /// Inlcudes provided sequence from the layer (Applies a mask)
        /// </summary>
        /// <param name="sequence">The mask to be applied</param>
        /// <returns>Itself</returns>
        public EffectLayer OnlyInclude(KeySequence sequence)
        {
            //Create draw alpha mask
            EffectLayer _alpha_mask = new EffectLayer(this.name + " - Alpha Mask", Color.Transparent);
            _alpha_mask.Set(sequence, Color.Black);

            //Apply alpha mask
            BitmapData srcData_alpha = _alpha_mask.colormap.LockBits(
                new Rectangle(0, 0, _alpha_mask.colormap.Width, _alpha_mask.colormap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            int alpha_mask_stride = srcData_alpha.Stride;
            IntPtr alpha_mask_Scan0 = srcData_alpha.Scan0;


            BitmapData srcData = colormap.LockBits(
                new Rectangle(0, 0, colormap.Width, colormap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            IntPtr Scan0 = srcData.Scan0;

            int width = colormap.Width;
            int height = colormap.Height;

            unsafe
            {
                byte* p_alpha = (byte*)(void*)alpha_mask_Scan0;
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte mask_alpha = p_alpha[(y * alpha_mask_stride) + x * 4 + 3];

                        p[(y * stride) + x * 4 + 3] = (byte)(mask_alpha * (p[(y * stride) + x * 4 + 3] / 255.0f));
                    }
                }
            }

            _alpha_mask.colormap.UnlockBits(srcData_alpha);
            colormap.UnlockBits(srcData);

            return this;
        }

        /// <summary>
        /// Returns the layer name
        /// </summary>
        /// <returns>Layer name</returns>
        public override string ToString()
        {
            return name;
        }
    }
}
