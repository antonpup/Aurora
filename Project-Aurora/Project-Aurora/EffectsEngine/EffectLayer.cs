using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using Aurora.Devices;
using Aurora.Utils;

namespace Aurora.EffectsEngine
{
    /// <summary>
    /// A class representing a bitmap layer for effects
    /// </summary>
    public class EffectLayer : IDisposable
    {
        private readonly String _name;
        private readonly Bitmap _colormap;
        private float _opacity = 1;

        private TextureBrush _textureBrush;
        private bool _needsRender;

        internal readonly Rectangle Dimension;

        internal TextureBrush TextureBrush
        {
            get
            {
                if (_needsRender || _textureBrush == null)
                {
                    var colorMatrix = new ColorMatrix();
                    colorMatrix.Matrix33 = _opacity;
                    var imageAttributes = new ImageAttributes();
                    imageAttributes.SetColorMatrix(
                        colorMatrix,
                        ColorMatrixFlag.Default,
                        ColorAdjustType.Brush);
                    imageAttributes.SetWrapMode(WrapMode.Clamp, Color.Empty);

                    _textureBrush?.Dispose();
                    _textureBrush = new TextureBrush(_colormap, Dimension, imageAttributes);
                    _needsRender = false;
                }
                else
                {
                    Console.Out.WriteLine("OK");;
                }

                return _textureBrush;
            }
            
        }

        Color _peripheral;

        private static readonly DeviceKeys[] PossiblePeripheralKeys = {
                DeviceKeys.Peripheral,
                DeviceKeys.Peripheral_FrontLight,
                DeviceKeys.Peripheral_ScrollWheel,
                DeviceKeys.Peripheral_Logo
            };

        /// <summary>
        /// Creates a new instance of the EffectLayer class with default parameters.
        /// </summary>
        public EffectLayer()
        {
            _name = "Effect Layer";
            _colormap = new Bitmap(Effects.canvas_width, Effects.canvas_height);
            _textureBrush = new TextureBrush(_colormap);
            Dimension = new Rectangle(0, 0, _colormap.Width, _colormap.Height);
            Graphics.FromImage(_colormap);
            _peripheral = Color.Empty;

            Fill(Color.Empty);
        }

        /// <summary>
        ///  A copy constructor, Creates a new instance of the EffectLayer class from another EffectLayer instance.
        /// </summary>
        /// <param name="anotherLayer">EffectLayer instance to copy data from</param>
        public EffectLayer(EffectLayer anotherLayer)
        {
            _name = anotherLayer._name;
            var graphicsUnit = anotherLayer.GetGraphics().PageUnit;
            var rectangleF = anotherLayer._colormap.GetBounds(ref graphicsUnit);
            _colormap = anotherLayer._colormap.Clone(rectangleF, anotherLayer._colormap.PixelFormat);
            _textureBrush = new TextureBrush(_colormap);
            Dimension = new Rectangle(0, 0, _colormap.Width, _colormap.Height);
            Graphics.FromImage(_colormap);
            _peripheral = anotherLayer._peripheral;

            _needsRender = anotherLayer._needsRender;
        }

        /// <summary>
        /// Creates a new instance of the EffectLayer class with a specified layer name.
        /// </summary>
        /// <param name="name">A layer name</param>
        public EffectLayer(string name)
        {
            _name = name;
            _colormap = new Bitmap(Effects.canvas_width, Effects.canvas_height);
            _textureBrush = new TextureBrush(_colormap);
            Dimension = new Rectangle(0, 0, _colormap.Width, _colormap.Height);
            Graphics.FromImage(_colormap);
            _peripheral = Color.Empty;

            Fill(Color.FromArgb(0, 1, 1, 1));
        }

        /// <summary>
        /// Creates a new instance of the EffectLayer class with a specified layer name. And fills the layer bitmap with a specified color.
        /// </summary>
        /// <param name="name">A layer name</param>
        /// <param name="color">A color to fill the bitmap with</param>
        public EffectLayer(string name, Color color)
        {
            _name = name;
            _colormap = new Bitmap(Effects.canvas_width, Effects.canvas_height);
            _textureBrush = new TextureBrush(_colormap);
            Dimension = new Rectangle(0, 0, _colormap.Width, _colormap.Height);
            Graphics.FromImage(_colormap);
            _peripheral = color;

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
        public EffectLayer(string name, LayerEffects effect, LayerEffectConfig effect_config, RectangleF rect = new())
        {
            _name = name;
            _colormap = new Bitmap(Effects.canvas_width, Effects.canvas_height);
            _textureBrush = new TextureBrush(_colormap);
            Dimension = new Rectangle(0, 0, _colormap.Width, _colormap.Height);
            Graphics.FromImage(_colormap);
            _peripheral = new Color();
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
                            (brush as LinearGradientBrush).ScaleTransform(rect.Width * 100 / effect_config.gradient_size, rect.Height * 100 / effect_config.gradient_size);
                        }
                        else
                        {
                            (brush as LinearGradientBrush).ScaleTransform(Effects.canvas_height * 100 / effect_config.gradient_size, Effects.canvas_height * 100 / effect_config.gradient_size);
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
                                (brush as PathGradientBrush).ScaleTransform((rect.Width - (2.0f * x_offset)) * 100 / effect_config.gradient_size, (rect.Height - (2.0f * y_offset)) * 100 / effect_config.gradient_size);
                            }
                            else
                            {
                                (brush as PathGradientBrush).ScaleTransform((Effects.canvas_height + x_offset) * 100 / effect_config.gradient_size, (Effects.canvas_height + y_offset) * 100 / effect_config.gradient_size);
                            }
                        }
                        else
                        {
                            if (!rect.IsEmpty)
                            {
                                (brush as PathGradientBrush).TranslateTransform(rect.X, rect.Y);
                                (brush as PathGradientBrush).ScaleTransform(rect.Width * 100 / effect_config.gradient_size, rect.Height * 100 / effect_config.gradient_size);
                            }
                            else
                            {
                                (brush as PathGradientBrush).ScaleTransform(Effects.canvas_height * 100 / effect_config.gradient_size, Effects.canvas_height * 100 / effect_config.gradient_size);
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
            _textureBrush.Dispose();
            _textureBrush = null;
            _colormap.Dispose();
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
        public void Fill(Brush brush)
        {
            using (Graphics g = Graphics.FromImage(_colormap))
            {
                g.FillRectangle(brush, Dimension);
                _needsRender = true;
            }
        }

        /// <summary>
        /// Fills the entire bitmap of the EffectLayer with a specified color.
        /// </summary>
        /// <param name="color">Color to be used during bitmap fill</param>
        /// <returns>Itself</returns>
        public EffectLayer Fill(Color color)
        {
            using (Graphics g = GetGraphics())
            {
                g.FillRectangle(new SolidBrush(color), Dimension);
                _needsRender = true;
            }

            return this;
        }
        
        public void Clear()
        {
            _keyBrushes.Clear();
            using (Graphics g = GetGraphics())
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.FillRectangle(ClearingBrush, Dimension);
                _needsRender = true;
            }
        }

        /// <summary>
        /// Sets a specific Devices.DeviceKeys on the bitmap with a specified color.
        /// </summary>
        /// <param name="key">DeviceKey to be set</param>
        /// <param name="color">Color to be used</param>
        /// <returns>Itself</returns>
        public EffectLayer Set(DeviceKeys key, Color color)
        {
            SetOneKey(key, color);

            return this;
        }

        /// <summary>
        /// Sets a specific Devices.DeviceKeys on the bitmap with a specified color.
        /// </summary>
        /// <param name="keys">Array of DeviceKeys to be set</param>
        /// <param name="color">Color to be used</param>
        public void Set(DeviceKeys[] keys, Color color)
        {
            foreach (var key in keys)
                SetOneKey(key, color);
        }

        /// <summary>
        /// Sets a specific KeySequence on the bitmap with a specified color.
        /// </summary>
        /// <param name="sequence">KeySequence to specify what regions of the bitmap need to be changed</param>
        /// <param name="color">Color to be used</param>
        /// <returns>Itself</returns>
        public EffectLayer Set(KeySequence sequence, Color color) => Set(sequence, new SolidBrush(color));

        /// <summary>
        /// Sets a specific KeySequence on the bitmap with a specified brush.
        /// </summary>
        /// <param name="sequence">KeySequence to specify what regions of the bitmap need to be changed</param>
        /// <param name="brush">Brush to be used</param>
        /// <returns>Itself</returns>
        public EffectLayer Set(KeySequence sequence, Brush brush)
        {
            if (sequence.type == KeySequenceType.Sequence)
            {
                foreach (var key in sequence.keys)
                    SetOneKey(key, brush);
            }
            else
            {
                using (Graphics g = Graphics.FromImage(_colormap))
                {
                    float x_pos = (float)Math.Round((sequence.freeform.X + Effects.grid_baseline_x) * Effects.editor_to_canvas_width);
                    float y_pos = (float)Math.Round((sequence.freeform.Y + Effects.grid_baseline_y) * Effects.editor_to_canvas_height);
                    float width = (float)Math.Round((sequence.freeform.Width * Effects.editor_to_canvas_width));
                    float height = (float)Math.Round((sequence.freeform.Height * Effects.editor_to_canvas_height));

                    if (width < 3) width = 3;
                    if (height < 3) height = 3;

                    Rectangle rect = new Rectangle((int)x_pos, (int)y_pos, (int)width, (int)height);

                    PointF rotatePoint = new PointF(x_pos + (width / 2.0f), y_pos + (height / 2.0f));

                    Matrix myMatrix = new Matrix();
                    myMatrix.RotateAt(sequence.freeform.Angle, rotatePoint, MatrixOrder.Append);

                    g.Transform = myMatrix;
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.FillRectangle(brush, rect);
                    _needsRender = true;
                }
            }

            return this;
        }

        /// <summary>
        /// Allows drawing some arbitrary content to the sequence bounds, including translation, scaling and rotation.<para/>
        /// Usage:<code>
        /// someEffectLayer.DrawTransformed(Properties.Sequence,<br/>
        ///     m => {<br/>
        ///         // We are prepending the transformations since we want the mirroring to happen BEFORE the rotation and scaling happens.<br/>
        ///         m.Translate(100, 0, MatrixOrder.Prepend); // These two are backwards because we are Prepending (so this is prepended first)<br/>
        ///         m.Scale(-1, 1, MatrixOrder.Prepend); // Then this is prepended before the tranlate.<br/>
        ///     },<br/>
        ///     gfx => {<br/>
        ///         gfx.FillRectangle(Brushes.Red, 0, 0, 30, 100);<br/>
        ///         gfx.FillRectangle(Brushes.Blue, 70, 0, 30, 100);<br/>
        ///     },
        ///     new RectangleF(0, 0, 100, 100);</code>
        /// This code will draw an X-mirrored image of a red stipe and a blue stripe (with a transparent gap in between) to the target keysequence area.
        /// </summary>
        /// <param name="sequence">The target sequence whose bounds will be used as the target location on the drawing canvas.</param>
        /// <param name="configureMatrix">An action that further configures the transformation matrix before render is called.</param>
        /// <param name="render">An action that receives a transformed graphics context and can render whatever it needs to.</param>
        /// <param name="sourceRegion">The source region of the rendered content. This is used when calculating the transformation matrix, so that this
        ///     rectangle in the render context is transformed to the keysequence bounds in the layer's context. Note that no clipping is performed.</param>
        public void DrawTransformed(KeySequence sequence, Action<Matrix> configureMatrix, Action<Graphics> render, RectangleF sourceRegion)
        {
            // The matrix represents the transformation that will be applied to the rendered content
            var matrix = new Matrix();

            // The bounds represent the target position of the render part
            // Note that we round the X and Y off to properly imitate the above `Set(KeySequence, Color)` method. Unsure exactly why this is done, but it _is_ done to replicate behaviour properly.
            //  Also unsure why the X and Y are rounded using math.Round but Width and Height are just truncated using an int cast??
            var boundsRaw = sequence.GetAffectedRegion();
            var bounds = new RectangleF((int)Math.Round(boundsRaw.X), (int)Math.Round(boundsRaw.Y), (int)boundsRaw.Width, (int)boundsRaw.Height);

            using (var gfx = Graphics.FromImage(_colormap))
            {

                // First, calculate the scaling required to transform the sourceRect's size into the bounds' size
                float sx = bounds.Width / sourceRegion.Width, sy = bounds.Height / sourceRegion.Height;

                // Perform this scale first
                // Note: that if the scale is zero, when setting the graphics transform to the matrix, it throws an error, so we must have NON-ZERO values
                // Note 2: Also tried using float.Epsilon but this also caused the exception, so a somewhat small number will have to suffice. Not noticed any visual issues with 0.001f.
                matrix.Scale(sx == 0 ? .001f : sx, sy == 0 ? .001f : sy, MatrixOrder.Append);

                // Second, for freeform objects, apply the rotation. This needs to be done AFTER the scaling, else the scaling is applied to the rotated object, which skews it
                // We rotate around the central point of the source region, but we need to take the scaling of the dimensions into account
                if (sequence.type == KeySequenceType.FreeForm)
                    matrix.RotateAt(sequence.freeform.Angle, new PointF((sourceRegion.Left + (sourceRegion.Width / 2f)) * sx, (sourceRegion.Top + (sourceRegion.Height / 2f)) * sy), MatrixOrder.Append);

                // Third, we can translate the matrix from the source to the target location.
                matrix.Translate(bounds.X - sourceRegion.Left, bounds.Y - sourceRegion.Top, MatrixOrder.Append);

                // Finally, call the custom matrix configure action
                configureMatrix(matrix);

                // Apply the matrix transform to the graphics context and then render
                gfx.Transform = matrix;
                render(gfx);
            }

            _needsRender = true;
        }

        /// <summary>
        /// Allows drawing some arbitrary content to the sequence bounds, including translation, scaling and rotation.<para/>
        /// See <see cref="DrawTransformed(KeySequence, Action{Matrix}, Action{Graphics}, RectangleF)"/> for usage.
        /// </summary>
        /// <param name="sequence">The target sequence whose bounds will be used as the target location on the drawing canvas.</param>
        /// <param name="render">An action that receives a transformed graphics context and can render whatever it needs to.</param>
        /// <param name="sourceRegion">The source region of the rendered content. This is used when calculating the transformation matrix, so that this
        ///     rectangle in the render context is transformed to the keysequence bounds in the layer's context. Note that no clipping is performed.</param>
        public void DrawTransformed(KeySequence sequence, Action<Graphics> render, RectangleF sourceRegion)
        {
            DrawTransformed(sequence, _ => { }, render, sourceRegion);
        }

        /// <summary>
        /// Allows drawing some arbitrary content to the sequence bounds, including translation, scaling and rotation.
        /// Uses the full canvas size as the source region.<para/>
        /// See <see cref="DrawTransformed(KeySequence, Action{Matrix}, Action{Graphics}, RectangleF)"/> for usage.
        /// </summary>
        /// <param name="sequence">The target sequence whose bounds will be used as the target location on the drawing canvas.</param>
        /// <param name="render">An action that receives a transformed graphics context and can render whatever it needs to.</param>
        public void DrawTransformed(KeySequence sequence, Action<Graphics> render)
        {
            DrawTransformed(sequence, render, new RectangleF(0, 0, Effects.canvas_width, Effects.canvas_height));
        }

        /// <summary>
        /// Sets one DeviceKeys key with a specific color on the bitmap
        /// </summary>
        /// <param name="key">DeviceKey to be set</param>
        /// <param name="color">Color to be used</param>
        private void SetOneKey(DeviceKeys key, Color color)
        {
            SetOneKey(key, new SolidBrush(color));
        }

        private readonly Dictionary<DeviceKeys, SolidBrush> _keyBrushes = new();
        private static readonly SolidBrush ClearingBrush = new(Color.Transparent);

        /// <summary>
        /// Sets one DeviceKeys key with a specific brush on the bitmap
        /// </summary>
        /// <param name="key">DeviceKey to be set</param>
        /// <param name="brush">Brush to be used</param>
        private void SetOneKey(DeviceKeys key, Brush brush)
        {
            if (brush is SolidBrush solidBrush)
            {
                if (_keyBrushes.TryGetValue(key, out var currentBrush) && currentBrush.Color == solidBrush.Color)
                {
                    return;
                }
                _keyBrushes[key] = solidBrush;
            }
            BitmapRectangle keymaping = Effects.GetBitmappingFromDeviceKey(key);
            _needsRender = true;

            if (key == DeviceKeys.Peripheral)
            {
                if (brush is SolidBrush pSolidBrush)
                    _peripheral = pSolidBrush.Color;
                // TODO Add support for this ^ to other brush types

                using (Graphics g = Graphics.FromImage(_colormap))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    foreach (DeviceKeys periKey in PossiblePeripheralKeys)
                    {
                        BitmapRectangle periKeymaping = Effects.GetBitmappingFromDeviceKey(periKey);

                        if (periKeymaping.IsValid)
                        {
                            g.FillRectangle(brush, periKeymaping.Rectangle);
                        }
                    }
                }
            }
            else
            {
                if (keymaping.Top < 0 || keymaping.Bottom > Effects.canvas_height ||
                    keymaping.Left < 0 || keymaping.Right > Effects.canvas_width)
                {
                    Global.logger.Warn("Coudln't set key color " + key);
                    return;
                }

                using (Graphics g = Graphics.FromImage(_colormap))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.FillRectangle(brush, keymaping.Rectangle);
                }
            }
        }

        /// <summary>
        /// Retrieves a color of the specified DeviceKeys key from the bitmap
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Color of the Key</returns>
        public Color Get(DeviceKeys key)
        {
            if (_keyBrushes.TryGetValue(key, out var brush))
            {
                return brush.Color;
            }
            try
            {
                BitmapRectangle keymaping = Effects.GetBitmappingFromDeviceKey(key);

                if (keymaping.IsEmpty && key == DeviceKeys.Peripheral)
                {
                    return _peripheral;
                }
                else
                {
                    if (keymaping.IsEmpty)
                        return Color.FromArgb(0, 0, 0);

                    return BitmapUtils.GetRegionColor(_colormap, keymaping.Rectangle);
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
        public Graphics GetGraphics()   //TODO deprecate
        {
            _needsRender = true;
            return Graphics.FromImage(_colormap);
        }

        /// <summary>
        /// Get the current layer bitmap.
        /// </summary>
        /// <returns>Layer Bitmap</returns>
        public Bitmap GetBitmap()
        {
            return _colormap;
        }

        /// <summary>
        /// + Operator, sums two EffectLayer together.
        /// </summary>
        /// <param name="lhs">Left Hand Side EffectLayer</param>
        /// <param name="rhs">Right Hand Side EffectLayer</param>
        /// <returns>Left hand side EffectLayer, which is a combination of two passed EffectLayers</returns>
        public static EffectLayer operator +(EffectLayer lhs, EffectLayer rhs)
        {
            using (Graphics g = lhs.GetGraphics())
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillRectangle(rhs.TextureBrush, rhs.Dimension);
            }
            lhs._needsRender = true;

            lhs._peripheral = ColorUtils.AddColors(lhs._peripheral, rhs._peripheral);
            return lhs;
        }

        /// <summary>
        /// * Operator, Multiplies an EffectLayer by a double, adjusting opacity and color of the layer bitmap.
        /// </summary>
        /// <param name="layer">EffectLayer to be adjusted</param>
        /// <param name="value">Double value that each bit in the bitmap will be multiplied by</param>
        /// <returns>The passed instance of EffectLayer with adjustments</returns>
        public static EffectLayer operator *(EffectLayer layer, double value)
        {
            if (!ColorUtils.NearlyEqual(layer._opacity,(float)value, 0.001f))
            {
                layer._opacity = (float) value;
                layer._needsRender = true;
            }
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
        public EffectLayer PercentEffect(Color foregroundColor, Color backgroundColor, KeySequence sequence, double value, double total = 1.0D, PercentEffectType percentEffectType = PercentEffectType.Progressive, double flash_past = 0.0, bool flash_reversed = false, bool blink_background = false)
        {
            if (sequence.type == KeySequenceType.Sequence)
                PercentEffect(foregroundColor, backgroundColor, sequence.keys.ToArray(), value, total, percentEffectType, flash_past, flash_reversed, blink_background);
            else
                PercentEffect(foregroundColor, backgroundColor, sequence.freeform, value, total, percentEffectType, flash_past, flash_reversed, blink_background);

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
                        backgroundColor = Utils.ColorUtils.BlendColors(backgroundColor, Color.Empty, Math.Sin((Utils.Time.GetMillisecondsSinceEpoch() % 1000.0D) / 1000.0D * Math.PI));
                    else
                        foregroundColor = Utils.ColorUtils.BlendColors(backgroundColor, foregroundColor, Math.Sin((Utils.Time.GetMillisecondsSinceEpoch() % 1000.0D) / 1000.0D * Math.PI));
                }
            }

            if ((percentEffectType == PercentEffectType.Highest_Key || percentEffectType == PercentEffectType.Highest_Key_Blend) && keys.Length > 0)
            {
                var activeKey = (int)Math.Ceiling(value / (total / keys.Length)) - 1;
                var col = percentEffectType == PercentEffectType.Highest_Key ? foregroundColor : Utils.ColorUtils.BlendColors(backgroundColor, foregroundColor, progress_total);
                SetOneKey(keys[Math.Min(Math.Max(activeKey, 0), keys.Length - 1)], col);

            }
            else
            {
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

            switch (percentEffectType)
            {
                case (PercentEffectType.AllAtOnce):
                    for (int i = 0; i < keys.Count(); i++)
                    {
                        Devices.DeviceKeys current_key = keys[i];
                        SetOneKey(current_key, spectrum.GetColorAt((float)progress_total, 1.0f, flash_amount));
                    }
                    break;
                case (PercentEffectType.Progressive_Gradual):
                    Clear();
                    for (int i = 0; i < keys.Count(); i++)
                    {
                        Devices.DeviceKeys current_key = keys[i];
                        if (i == (int)progress)
                        {
                            double percent = (double)progress - i;
                            SetOneKey(
                                current_key,
                                Utils.ColorUtils.MultiplyColorByScalar(spectrum.GetColorAt((float)i / (float)(keys.Count() - 1), 1.0f, flash_amount), percent)
                            );
                        }
                        else if (i < (int)progress)
                            SetOneKey(current_key, spectrum.GetColorAt((float)i / (float)(keys.Count() - 1), 1.0f, flash_amount));
                    }
                    break;
                default:
                    for (int i = 0; i < keys.Count(); i++)
                    {
                        Devices.DeviceKeys current_key = keys[i];
                        if (i < (int)progress)
                            SetOneKey(current_key, spectrum.GetColorAt((float)i / (float)(keys.Count() - 1), 1.0f, flash_amount));
                    }
                    break;
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
                    if (!blink_background)
                        foregroundColor = ColorUtils.BlendColors(backgroundColor, foregroundColor, Math.Sin((Time.GetMillisecondsSinceEpoch() % 1000.0D) / 1000.0D * Math.PI));
                    if (blink_background)
                        backgroundColor = ColorUtils.BlendColors(backgroundColor, Color.Empty, Math.Sin((Time.GetMillisecondsSinceEpoch() % 1000.0D) / 1000.0D * Math.PI));
                }
            }

            using (Graphics g = Graphics.FromImage(_colormap))
            {
                float x_pos = (float)Math.Round((freeform.X + Effects.grid_baseline_x) * Effects.editor_to_canvas_width);
                float y_pos = (float)Math.Round((freeform.Y + Effects.grid_baseline_y) * Effects.editor_to_canvas_height);
                float width = freeform.Width * Effects.editor_to_canvas_width;
                float height = freeform.Height * Effects.editor_to_canvas_height;

                if (width < 3) width = 3;
                if (height < 3) height = 3;


                if (percentEffectType == PercentEffectType.AllAtOnce)
                {
                    Rectangle rect = new Rectangle((int)x_pos, (int)y_pos, (int)width, (int)height);

                    PointF rotatePoint = new PointF(x_pos + (width / 2.0f), y_pos + (height / 2.0f));

                    Matrix myMatrix = new Matrix();
                    myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

                    g.Transform = myMatrix;
                    g.FillRectangle(new SolidBrush(ColorUtils.BlendColors(backgroundColor, foregroundColor, progress_total)), rect);
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
            _needsRender = true;

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

            using (Graphics g = Graphics.FromImage(_colormap))
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
                _needsRender = true;

                return this;
            }
        }

        /// <summary>
        /// Excludes provided sequence from the layer (Applies a mask)
        /// </summary>
        /// <param name="sequence">The mask to be applied</param>
        public void Exclude(KeySequence sequence)
        {
            //Create draw alpha mask
            EffectLayer _alpha_mask = new EffectLayer(_name + " - Alpha Mask", Color.Transparent);
            _alpha_mask.Set(sequence, Color.Black);

            //Apply alpha mask
            BitmapData srcData_alpha = _alpha_mask._colormap.LockBits(
                Dimension,
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            int alpha_mask_stride = srcData_alpha.Stride;
            IntPtr alpha_mask_Scan0 = srcData_alpha.Scan0;


            BitmapData srcData = _colormap.LockBits(
                Dimension,
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            IntPtr Scan0 = srcData.Scan0;

            int width = Dimension.Width;
            int height = Dimension.Height;

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

            _alpha_mask._colormap.UnlockBits(srcData_alpha);
            _colormap.UnlockBits(srcData);
            _needsRender = true;
        }

        /// <summary>
        /// Inlcudes provided sequence from the layer (Applies a mask)
        /// </summary>
        /// <param name="sequence">The mask to be applied</param>
        public void OnlyInclude(KeySequence sequence)
        {
            //Create draw alpha mask
            EffectLayer _alpha_mask = new EffectLayer(this._name + " - Alpha Mask", Color.Transparent);
            _alpha_mask.Set(sequence, Color.Black);

            //Apply alpha mask
            BitmapData srcData_alpha = _alpha_mask._colormap.LockBits(
                Dimension,
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            int alpha_mask_stride = srcData_alpha.Stride;
            IntPtr alpha_mask_Scan0 = srcData_alpha.Scan0;


            BitmapData srcData = _colormap.LockBits(
                Dimension,
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            IntPtr Scan0 = srcData.Scan0;

            int width = Dimension.Width;
            int height = Dimension.Height;

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

            _alpha_mask._colormap.UnlockBits(srcData_alpha);
            _colormap.UnlockBits(srcData);
            _needsRender = true;
        }

        /// <summary>
        /// Returns the layer name
        /// </summary>
        /// <returns>Layer name</returns>
        public override string ToString()
        {
            return _name;
        }
    }
}
