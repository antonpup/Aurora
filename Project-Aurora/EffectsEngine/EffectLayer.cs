using Aurora.EffectsEngine.Functions;
using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.EffectsEngine
{
    public class EffectLayer : IDisposable
    {
        private String name;
        private Bitmap colormap;

        private object bufferLock = new object();

        private bool needsRender = false;

        Color peripheral;
        private List<EffectColorFunction> post_functions = new List<EffectColorFunction>();

        static private ColorSpectrum rainbow = new ColorSpectrum(ColorSpectrum.RainbowLoop);

        public EffectLayer()
        {
            name = "Effect Layer";
            colormap = new Bitmap(Effects.canvas_width, Effects.canvas_height);
            peripheral = Color.FromArgb(0, 0, 0, 0);

            Fill(Color.FromArgb(0, 0, 0, 0));
        }

        public EffectLayer(EffectLayer another_layer)
        {
            this.name = another_layer.name;
            colormap = new Bitmap(another_layer.colormap);
            peripheral = another_layer.peripheral;

            needsRender = another_layer.needsRender;
            post_functions = new List<EffectColorFunction>(another_layer.post_functions);
        }

        public EffectLayer(string name)
        {
            this.name = name;
            colormap = new Bitmap(Effects.canvas_width, Effects.canvas_height);
            peripheral = Color.FromArgb(0, 0, 0, 0);

            Fill(Color.FromArgb(0, 0, 0, 0));
        }

        public EffectLayer(string name, Color color)
        {
            this.name = name;
            colormap = new Bitmap(Effects.canvas_width, Effects.canvas_height);
            peripheral = color;

            Fill(color);
        }

        public EffectLayer(string name, LayerEffects effect, LayerEffectConfig effect_config, float scale_x = 0.0f, float scale_y = 0.0f)
        {
            this.name = name;
            colormap = new Bitmap(Effects.canvas_width, Effects.canvas_height);
            peripheral = new Color();
            Brush brush;

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

                    brush = CreateRainbowBrush();
                    (brush as LinearGradientBrush).RotateTransform(0.0f);
                    (brush as LinearGradientBrush).TranslateTransform(effect_config.shift_amount, effect_config.shift_amount);

                    Fill(brush);

                    effect_config.last_effect_call = Utils.Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.RainbowShift_Vertical:
                    effect_config.shift_amount += ((Utils.Time.GetMillisecondsSinceEpoch() - effect_config.last_effect_call) / 1000.0f) * 5.0f * effect_config.speed;
                    effect_config.shift_amount = effect_config.shift_amount % Effects.canvas_biggest;

                    brush = CreateRainbowBrush();
                    (brush as LinearGradientBrush).RotateTransform(90.0f);
                    (brush as LinearGradientBrush).TranslateTransform(effect_config.shift_amount, effect_config.shift_amount);

                    Fill(brush);

                    effect_config.last_effect_call = Utils.Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.RainbowShift_Diagonal:
                    effect_config.shift_amount += ((Utils.Time.GetMillisecondsSinceEpoch() - effect_config.last_effect_call) / 1000.0f) * 5.0f * effect_config.speed;
                    effect_config.shift_amount = effect_config.shift_amount % Effects.canvas_biggest;

                    brush = CreateRainbowBrush();
                    (brush as LinearGradientBrush).RotateTransform(45.0f);
                    (brush as LinearGradientBrush).TranslateTransform(effect_config.shift_amount, effect_config.shift_amount);

                    Fill(brush);

                    effect_config.last_effect_call = Utils.Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.RainbowShift_Diagonal_Other:
                    effect_config.shift_amount += ((Utils.Time.GetMillisecondsSinceEpoch() - effect_config.last_effect_call) / 1000.0f) * 5.0f * effect_config.speed;
                    effect_config.shift_amount = effect_config.shift_amount % Effects.canvas_biggest;

                    brush = CreateRainbowBrush();
                    (brush as LinearGradientBrush).RotateTransform(-45.0f);
                    (brush as LinearGradientBrush).TranslateTransform(effect_config.shift_amount, effect_config.shift_amount);

                    Fill(brush);

                    effect_config.last_effect_call = Utils.Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.RainbowShift_Custom_Angle:
                    effect_config.shift_amount += ((Utils.Time.GetMillisecondsSinceEpoch() - effect_config.last_effect_call) / 1000.0f) * 5.0f * effect_config.speed;
                    effect_config.shift_amount = effect_config.shift_amount % Effects.canvas_biggest;

                    brush = CreateRainbowBrush();
                    (brush as LinearGradientBrush).RotateTransform(effect_config.angle);
                    (brush as LinearGradientBrush).TranslateTransform(effect_config.shift_amount, effect_config.shift_amount);

                    Fill(brush);

                    effect_config.last_effect_call = Utils.Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.GradientShift_Custom_Angle:
                    effect_config.shift_amount += ((Utils.Time.GetMillisecondsSinceEpoch() - effect_config.last_effect_call) / 1000.0f) * 0.25f * effect_config.speed;
                    effect_config.shift_amount = effect_config.shift_amount % Effects.canvas_biggest;

                    brush = effect_config.brush.GetDrawingBrush();
                    if (effect_config.brush.type == EffectBrush.BrushType.Linear)
                    {
                        (brush as LinearGradientBrush).ScaleTransform(
                            scale_x != 0.0f ? scale_x : Effects.canvas_height,
                            scale_y != 0.0f ? scale_y : Effects.canvas_height
                            );
                        (brush as LinearGradientBrush).RotateTransform(effect_config.angle);
                        (brush as LinearGradientBrush).TranslateTransform(effect_config.shift_amount, effect_config.shift_amount);
                    }
                    else if (effect_config.brush.type == EffectBrush.BrushType.Radial)
                    {
                        (brush as PathGradientBrush).ScaleTransform(
                            scale_x != 0.0f ? scale_x : Effects.canvas_height,
                            scale_y != 0.0f ? scale_y : Effects.canvas_height
                            );
                        (brush as PathGradientBrush).RotateTransform(effect_config.angle);
                        //(brush as PathGradientBrush).TranslateTransform(effect_config.shift_amount, effect_config.shift_amount);
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

        public void Fill(Brush brush)
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
        }

        public void Fill(Color color)
        {
            using (Graphics g = Graphics.FromImage(colormap))
            {
                Rectangle rect = new Rectangle(0, 0, colormap.Width, colormap.Height);
                g.FillRectangle(new SolidBrush(color), rect);
                needsRender = true;
            }
        }


        public void Set(int x, int y, Color color)
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
        }

        public void Set(Devices.DeviceKeys key, Color color)
        {
            SetOneKey(key, color);
        }

        public void Set(KeySequence sequence, Color color)
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
                    float x_pos = (float)Math.Round(sequence.freeform.X * Effects.editor_to_canvas_width);
                    float y_pos = (float)Math.Round(sequence.freeform.Y * Effects.editor_to_canvas_height);
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
        }

        private void SetOneKey(Devices.DeviceKeys key, Color color)
        {
            Bitmaping keymaping = Effects.GetBitmappingFromDeviceKey(key);

            if (keymaping.Equals(new Bitmaping(0, 0, 0, 0)) && key == Devices.DeviceKeys.Peripheral)
            {
                peripheral = color;
            }
            else
            {
                if (keymaping.topleft_y < 0 || keymaping.bottomright_y > Effects.canvas_height ||
                    keymaping.topleft_x < 0 || keymaping.bottomright_x > Effects.canvas_width)
                {
                    Global.logger.LogLine("Coudln't set key color " + key.ToString(), Logging_Level.Warning);
                    return;
                }
                else
                {
                    using (Graphics g = Graphics.FromImage(colormap))
                    {
                        Rectangle keyarea = new Rectangle(keymaping.topleft_x, keymaping.topleft_y, keymaping.bottomright_x - keymaping.topleft_x, keymaping.bottomright_y - keymaping.topleft_y);
                        g.FillRectangle(new SolidBrush(color), keyarea);
                        needsRender = true;
                    }
                }
            }
        }

        public Color Get(int x, int y)
        {
            if (needsRender)
                RenderLayer();


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

        public Color Get(Devices.DeviceKeys key)
        {
            Bitmaping keymaping = Effects.GetBitmappingFromDeviceKey(key);

            if (keymaping.Equals(new Bitmaping(0, 0, 0, 0)) && key == Devices.DeviceKeys.Peripheral)
            {
                return peripheral;
            }
            else
            {
                if (keymaping.bottomright_x - keymaping.topleft_x == 0 || keymaping.bottomright_y - keymaping.topleft_y == 0)
                    return Color.FromArgb(0, 0, 0);

                if (needsRender)
                    RenderLayer();

                EffectColor color = new EffectColor(0, 0, 0, 0);

                long Red = 0;
                long Green = 0;
                long Blue = 0;
                long Alpha = 0;

                BitmapData srcData = colormap.LockBits(
                    new Rectangle(keymaping.topleft_x, keymaping.topleft_y, keymaping.bottomright_x - keymaping.topleft_x, keymaping.bottomright_y - keymaping.topleft_y),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                int stride = srcData.Stride;

                IntPtr Scan0 = srcData.Scan0;

                int width = keymaping.bottomright_x - keymaping.topleft_x;
                int height = keymaping.bottomright_y - keymaping.topleft_y;

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            Blue += p[(y * stride) + x * 4];
                            Green += p[(y * stride) + x * 4 + 1];
                            Red += p[(y * stride) + x * 4 + 2];
                            Alpha += p[(y * stride) + x * 4 + 3];
                        }
                    }
                }

                int Colorscount = width * height;

                colormap.UnlockBits(srcData);

                return Color.FromArgb((int)(Alpha / Colorscount), (int)(Red / Colorscount), (int)(Green / Colorscount), (int)(Blue / Colorscount));
            }
        }

        public Graphics GetGraphics()
        {
            if (needsRender)
                RenderLayer();

            return Graphics.FromImage(colormap);
        }

        public Bitmap GetBitmap()
        {
            if (needsRender)
                RenderLayer();

            return colormap;
        }

        public static EffectLayer operator +(EffectLayer layer1, EffectLayer layer2)
        {
            EffectLayer added = new EffectLayer(layer1);
            added.name += " + " + layer2.name;

            if (added.needsRender)
                added.RenderLayer();

            if (layer2.needsRender)
                layer2.RenderLayer();

            using (Graphics g = added.GetGraphics())
            {
                lock (layer2.bufferLock)
                    g.DrawImage(layer2.colormap, 0, 0);
            }

            added.Set(Devices.DeviceKeys.Peripheral, Utils.ColorUtils.AddColors(layer1.Get(Devices.DeviceKeys.Peripheral), layer2.Get(Devices.DeviceKeys.Peripheral)));

            return added;
        }

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
                        p[(y * stride) + x * 4] = Utils.ColorUtils.ColorByteMultiplication(p[(y * stride) + x * 4], value);
                        p[(y * stride) + x * 4 + 1] = Utils.ColorUtils.ColorByteMultiplication(p[(y * stride) + x * 4 + 1], value);
                        p[(y * stride) + x * 4 + 2] = Utils.ColorUtils.ColorByteMultiplication(p[(y * stride) + x * 4 + 2], value);
                        p[(y * stride) + x * 4 + 3] = Utils.ColorUtils.ColorByteMultiplication(p[(y * stride) + x * 4 + 3], value);
                    }
                }
            }

            layer.colormap.UnlockBits(srcData);

            layer.peripheral = Utils.ColorUtils.MultiplyColorByScalar(layer.peripheral, value);

            return layer;
        }

        public void PercentEffect(Color foregroundColor, Color backgroundColor, Settings.KeySequence sequence, double value, double total = 1.0D, PercentEffectType effectType = PercentEffectType.Progressive)
        {
            if (sequence.type == KeySequenceType.Sequence)
                PercentEffect(foregroundColor, backgroundColor, sequence.keys.ToArray(), value, total, effectType);
            else
                PercentEffect(foregroundColor, backgroundColor, sequence.freeform, value, total, effectType);
        }

        public void PercentEffect(ColorSpectrum spectrum, Settings.KeySequence sequence, double value, double total = 1.0D, PercentEffectType effectType = PercentEffectType.Progressive)
        {
            if (sequence.type == KeySequenceType.Sequence)
                PercentEffect(spectrum, sequence.keys.ToArray(), value, total, effectType);
            else
                PercentEffect(spectrum, sequence.freeform, value, total, effectType);
        }

        public void PercentEffect(Color foregroundColor, Color backgroundColor, Devices.DeviceKeys[] keys, double value, double total, PercentEffectType effectType = PercentEffectType.Progressive)
        {
            double progress_total = value / total;
            if (progress_total < 0.0)
                progress_total = 0.0;
            else if (progress_total > 1.0)
                progress_total = 1.0;

            double progress = progress_total * keys.Count();

            for (int i = 0; i < keys.Count(); i++)
            {
                Devices.DeviceKeys current_key = keys[i];

                switch (effectType)
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
        }

        public void PercentEffect(ColorSpectrum spectrum, Devices.DeviceKeys[] keys, double value, double total, PercentEffectType effectType = PercentEffectType.Progressive)
        {
            double progress_total = value / total;
            if (progress_total < 0.0)
                progress_total = 0.0;
            else if (progress_total > 1.0)
                progress_total = 1.0;

            double progress = progress_total * keys.Count();

            for (int i = 0; i < keys.Count(); i++)
            {
                Devices.DeviceKeys current_key = keys[i];

                switch (effectType)
                {
                    case (PercentEffectType.AllAtOnce):
                        SetOneKey(current_key, spectrum.GetColorAt((float)progress_total));
                        break;
                    case (PercentEffectType.Progressive_Gradual):
                        if (i == (int)progress)
                        {
                            double percent = (double)progress - i;
                            SetOneKey(current_key, Utils.ColorUtils.MultiplyColorByScalar(spectrum.GetColorAt((float)i / (float)keys.Count()), percent));
                        }
                        else if (i < (int)progress)
                            SetOneKey(current_key, spectrum.GetColorAt((float)i / (float)keys.Count()));
                        break;
                    default:
                        if (i < (int)progress)
                            SetOneKey(current_key, spectrum.GetColorAt((float)i / (float)keys.Count()));
                        break;
                }
            }
        }

        public void PercentEffect(Color foregroundColor, Color backgroundColor, Settings.FreeFormObject freeform, double value, double total, PercentEffectType effectType = PercentEffectType.Progressive)
        {
            double progress_total = value / total;
            if (progress_total < 0.0 || Double.IsNaN(progress_total))
                progress_total = 0.0;
            else if (progress_total > 1.0)
                progress_total = 1.0;


            using (Graphics g = Graphics.FromImage(colormap))
            {
                float x_pos = (float)Math.Round((freeform.X + Effects.grid_baseline_x) * Effects.editor_to_canvas_width);
                float y_pos = (float)Math.Round((freeform.Y + Effects.grid_baseline_y) * Effects.editor_to_canvas_height);
                float width = (float)(freeform.Width * Effects.editor_to_canvas_width);
                float height = (float)(freeform.Height * Effects.editor_to_canvas_height);

                if (width < 3) width = 3;
                if (height < 3) height = 3;


                if (effectType == PercentEffectType.AllAtOnce)
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
        }

        public void PercentEffect(ColorSpectrum spectrum, Settings.FreeFormObject freeform, double value, double total, PercentEffectType effectType = PercentEffectType.Progressive)
        {
            double progress_total = value / total;
            if (progress_total < 0.0)
                progress_total = 0.0;
            else if (progress_total > 1.0)
                progress_total = 1.0;

            using (Graphics g = Graphics.FromImage(colormap))
            {
                float x_pos = (float)Math.Round((freeform.X + Effects.grid_baseline_x) * Effects.editor_to_canvas_width);
                float y_pos = (float)Math.Round((freeform.Y + Effects.grid_baseline_y) * Effects.editor_to_canvas_height);
                float width = (float)(freeform.Width * Effects.editor_to_canvas_width);
                float height = (float)(freeform.Height * Effects.editor_to_canvas_height);

                if (width < 3) width = 3;
                if (height < 3) height = 3;

                if (effectType == PercentEffectType.AllAtOnce)
                {
                    Rectangle rect = new Rectangle((int)x_pos, (int)y_pos, (int)width, (int)height);

                    PointF rotatePoint = new PointF(x_pos + (width / 2.0f), y_pos + (height / 2.0f));

                    Matrix myMatrix = new Matrix();
                    myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

                    g.Transform = myMatrix;
                    g.FillRectangle(new SolidBrush(spectrum.GetColorAt((float)progress_total)), rect);
                }
                else
                {
                    double progress = progress_total * width;

                    Rectangle rect = new Rectangle((int)x_pos, (int)y_pos, (int)progress, (int)height);

                    PointF rotatePoint = new PointF(x_pos + (width / 2.0f), y_pos + (height / 2.0f));

                    Matrix myMatrix = new Matrix();
                    myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

                    g.Transform = myMatrix;
                    LinearGradientBrush brush = spectrum.ToLinearGradient(width, 0, x_pos, 0);
                    g.FillRectangle(brush, rect);
                }

                
            }
        }

        public void DrawFreeForm(Settings.FreeFormObject freeform, Color color)
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
        }

        public void DrawColorZones(ColorZone[] colorzones)
        {
            foreach (ColorZone cz in colorzones)
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
                    else if (cz.effect != LayerEffects.None)
                    {
                        float x_pos = (float)Math.Round((cz.keysequence.freeform.X + Effects.grid_baseline_x) * Effects.editor_to_canvas_width);
                        float y_pos = (float)Math.Round((cz.keysequence.freeform.Y + Effects.grid_baseline_y) * Effects.editor_to_canvas_height);
                        float width = (float)Math.Round((double)(cz.keysequence.freeform.Width * Effects.editor_to_canvas_width));
                        float height = (float)Math.Round((double)(cz.keysequence.freeform.Height * Effects.editor_to_canvas_height));

                        if (width < 3) width = 3;
                        if (height < 3) height = 3;

                        Rectangle rect = new Rectangle((int)x_pos, (int)y_pos, (int)width, (int)height);


                        EffectLayer temp_layer = new EffectLayer("Color Zone Effect", cz.effect, cz.effect_config, );

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
        }

        public void AddPostFunction(EffectColorFunction function)
        {
            this.post_functions.Add(function);
            needsRender = true;
        }

        public void RenderLayer()
        {
            BitmapData srcData = colormap.LockBits(
                    new Rectangle(0, 0, colormap.Width, colormap.Height),
                    ImageLockMode.ReadWrite,
                    PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            IntPtr Scan0 = srcData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                Parallel.ForEach(post_functions, (func) =>
                {

                    Tuple<EffectPoint, Color>[] points = func.getPointMap();

                    foreach (Tuple<EffectPoint, Color> point in points)
                    {
                        if (float.IsNaN(point.Item1.X) || float.IsNaN(point.Item1.Y) ||
                            (int)point.Item1.X >= Effects.canvas_width || (int)point.Item1.Y >= Effects.canvas_height ||
                            (int)point.Item1.X < 0 || (int)point.Item1.Y < 0
                            )
                            continue;

                        int x = (int)point.Item1.X;
                        int y = (int)point.Item1.Y;

                        Color current_color = Color.FromArgb(p[(y * stride) + x * 4 + 3], p[(y * stride) + x * 4 + 2], p[(y * stride) + x * 4 + 1], p[(y * stride) + x * 4]);

                        Color resulting_color = Utils.ColorUtils.AddColors(current_color, point.Item2);

                        p[(y * stride) + x * 4] = resulting_color.B;
                        p[(y * stride) + x * 4 + 1] = resulting_color.G;
                        p[(y * stride) + x * 4 + 2] = resulting_color.R;
                        p[(y * stride) + x * 4 + 3] = resulting_color.A;
                    }

                });
            }

            colormap.UnlockBits(srcData);

            needsRender = false;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
