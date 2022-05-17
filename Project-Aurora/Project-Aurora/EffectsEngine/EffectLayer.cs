using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Aurora.Devices;
using Aurora.Settings;
using Aurora.Utils;

namespace Aurora.EffectsEngine
{
    /// <summary>
    /// A class representing a bitmap layer for effects
    /// </summary>
    public class EffectLayer : IDisposable
    {
        private static readonly Lazy<EffectLayer> _emptyLayerFactory = new();
        public static EffectLayer EmptyLayer => _emptyLayerFactory.Value;

        private readonly string _name;
        private Bitmap _colormap;
        private float _opacity = 1;

        private TextureBrush _textureBrush;
        private bool _needsRender;

        internal Rectangle Dimension;

        internal TextureBrush TextureBrush
        {
            get
            {
                if (!_needsRender && _textureBrush != null) return _textureBrush;

                var colorMatrix = new ColorMatrix
                {
                    Matrix33 = _opacity
                };
                var imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(colorMatrix);
                imageAttributes.SetWrapMode(WrapMode.Clamp, Color.Empty);

                _textureBrush?.Dispose();
                _textureBrush = new TextureBrush(_colormap, Dimension, imageAttributes);
                _needsRender = false;

                return _textureBrush;
            }
            
        }

        private Color _peripheral;

        private static readonly DeviceKeys[] PossiblePeripheralKeys = {
                DeviceKeys.Peripheral,
                DeviceKeys.Peripheral_FrontLight,
                DeviceKeys.Peripheral_ScrollWheel,
                DeviceKeys.Peripheral_Logo
            };

        /// <summary>
        /// Creates a new instance of the EffectLayer class with default parameters.
        /// </summary>
        [Obsolete("Always name the layer")]
        public EffectLayer()
        {
            _name = "Unknown Layer";
            _colormap = new Bitmap(Effects.CanvasWidth, Effects.CanvasHeight);
            Effects.CanvasChanged += Invalidate;
            _textureBrush = new TextureBrush(_colormap);
            Dimension = new Rectangle(0, 0, _colormap.Width, _colormap.Height);
            _peripheral = Color.Empty;

            FillOver(Color.Empty);
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
            Effects.CanvasChanged += Invalidate;
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
            _colormap = new Bitmap(Effects.CanvasWidth, Effects.CanvasHeight);
            Effects.CanvasChanged += Invalidate;
            _textureBrush = new TextureBrush(_colormap);
            Dimension = new Rectangle(0, 0, _colormap.Width, _colormap.Height);
            Graphics.FromImage(_colormap);
            _peripheral = Color.Empty;

            FillOver(Color.FromArgb(0, 1, 1, 1));
        }

        /// <summary>
        /// Creates a new instance of the EffectLayer class with a specified layer name. And fills the layer bitmap with a specified color.
        /// </summary>
        /// <param name="name">A layer name</param>
        /// <param name="color">A color to fill the bitmap with</param>
        public EffectLayer(string name, Color color)
        {
            _name = name;
            _colormap = new Bitmap(Effects.CanvasWidth, Effects.CanvasHeight);
            Effects.CanvasChanged += Invalidate;
            _textureBrush = new TextureBrush(_colormap);
            Dimension = new Rectangle(0, 0, _colormap.Width, _colormap.Height);
            Graphics.FromImage(_colormap);
            _peripheral = color;

            FillOver(color);
        }

        /// <summary>
        /// Creates a new instance of the EffectLayer class with a specified layer name. And applies a LayerEffect onto this EffectLayer instance.
        /// Using the parameters from LayerEffectConfig and a specified region in RectangleF
        /// </summary>
        /// <param name="name">A layer name</param>
        /// <param name="effect">An enum specifying which LayerEffect to apply</param>
        /// <param name="effectConfig">Configurations for the LayerEffect</param>
        /// <param name="rect">A rectangle specifying what region to apply effects in</param>
        public EffectLayer(string name, LayerEffects effect, LayerEffectConfig effectConfig, RectangleF rect = new())
        {
            _name = name;
            _colormap = new Bitmap(Effects.CanvasWidth, Effects.CanvasHeight);
            Effects.CanvasChanged += Invalidate;
            _textureBrush = new TextureBrush(_colormap);
            Dimension = new Rectangle(0, 0, _colormap.Width, _colormap.Height);
            Graphics.FromImage(_colormap);
            _peripheral = new Color();
            Brush brush;
            var shift = 0.0f;

            switch (effect)
            {
                case LayerEffects.ColorOverlay:
                    FillOver(effectConfig.primary);
                    break;
                case LayerEffects.ColorBreathing:
                    FillOver(effectConfig.primary);
                    float sine = (float)Math.Pow(Math.Sin((double)(Time.GetMillisecondsSinceEpoch() % 10000L / 10000.0f) * 2 * Math.PI * effectConfig.speed), 2);
                    FillOver(Color.FromArgb((byte)(sine * 255), effectConfig.secondary));
                    break;
                case LayerEffects.RainbowShift_Horizontal:
                    effectConfig.shift_amount += (Time.GetMillisecondsSinceEpoch() - effectConfig.last_effect_call) / 1000.0f * 5.0f * effectConfig.speed;
                    effectConfig.shift_amount %= Effects.CanvasBiggest;

                    if (effectConfig.animation_type == AnimationType.Translate_XY)
                        shift = effectConfig.shift_amount;

                    if (effectConfig.animation_reverse)
                        shift *= -1.0f;

                    brush = CreateRainbowBrush();
                    (brush as LinearGradientBrush).RotateTransform(0.0f);
                    (brush as LinearGradientBrush).TranslateTransform(shift, shift);

                    Fill(brush);

                    effectConfig.last_effect_call = Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.RainbowShift_Vertical:
                    effectConfig.shift_amount += (Time.GetMillisecondsSinceEpoch() - effectConfig.last_effect_call) / 1000.0f * 5.0f * effectConfig.speed;
                    effectConfig.shift_amount %= Effects.CanvasBiggest;

                    if (effectConfig.animation_type == AnimationType.Translate_XY)
                        shift = effectConfig.shift_amount;

                    if (effectConfig.animation_reverse)
                        shift *= -1.0f;

                    brush = CreateRainbowBrush();
                    (brush as LinearGradientBrush).RotateTransform(90.0f);
                    (brush as LinearGradientBrush).TranslateTransform(shift, shift);

                    Fill(brush);

                    effectConfig.last_effect_call = Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.RainbowShift_Diagonal:
                    effectConfig.shift_amount += (Time.GetMillisecondsSinceEpoch() - effectConfig.last_effect_call) / 1000.0f * 5.0f * effectConfig.speed;
                    effectConfig.shift_amount %= Effects.CanvasBiggest;

                    if (effectConfig.animation_type == AnimationType.Translate_XY)
                        shift = effectConfig.shift_amount;

                    if (effectConfig.animation_reverse)
                        shift *= -1.0f;

                    brush = CreateRainbowBrush();
                    (brush as LinearGradientBrush).RotateTransform(45.0f);
                    (brush as LinearGradientBrush).TranslateTransform(shift, shift);

                    Fill(brush);

                    effectConfig.last_effect_call = Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.RainbowShift_Diagonal_Other:
                    effectConfig.shift_amount += (Time.GetMillisecondsSinceEpoch() - effectConfig.last_effect_call) / 1000.0f * 5.0f * effectConfig.speed;
                    effectConfig.shift_amount %= Effects.CanvasBiggest;

                    if (effectConfig.animation_type == AnimationType.Translate_XY)
                        shift = effectConfig.shift_amount;

                    if (effectConfig.animation_reverse)
                        shift *= -1.0f;

                    brush = CreateRainbowBrush();
                    (brush as LinearGradientBrush).RotateTransform(-45.0f);
                    (brush as LinearGradientBrush).TranslateTransform(shift, shift);

                    Fill(brush);

                    effectConfig.last_effect_call = Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.RainbowShift_Custom_Angle:
                    effectConfig.shift_amount += (Time.GetMillisecondsSinceEpoch() - effectConfig.last_effect_call) / 1000.0f * 5.0f * effectConfig.speed;
                    effectConfig.shift_amount %= Effects.CanvasBiggest;


                    if (effectConfig.animation_type == AnimationType.Translate_XY)
                        shift = effectConfig.shift_amount;

                    if (effectConfig.animation_reverse)
                        shift *= -1.0f;

                    brush = CreateRainbowBrush();
                    (brush as LinearGradientBrush).RotateTransform(effectConfig.angle);
                    (brush as LinearGradientBrush).TranslateTransform(shift, shift);

                    Fill(brush);

                    effectConfig.last_effect_call = Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.GradientShift_Custom_Angle:
                    effectConfig.shift_amount += (Time.GetMillisecondsSinceEpoch() - effectConfig.last_effect_call) / 1000.0f * 0.067f * effectConfig.speed;
                    effectConfig.shift_amount %= Effects.CanvasBiggest;

                    shift = effectConfig.animation_type switch
                    {
                        AnimationType.Translate_XY => effectConfig.shift_amount,
                        AnimationType.Zoom_in when effectConfig.brush.type == EffectBrush.BrushType.Radial =>
                            (Effects.CanvasBiggest - effectConfig.shift_amount) * 40.0f % Effects.CanvasBiggest,
                        AnimationType.Zoom_out when effectConfig.brush.type == EffectBrush.BrushType.Radial =>
                            effectConfig.shift_amount * 40.0f % Effects.CanvasBiggest,
                        _ => shift
                    };

                    if (effectConfig.animation_reverse)
                        shift *= -1.0f;

                    brush = effectConfig.brush.GetDrawingBrush();
                    switch (effectConfig.brush.type)
                    {
                        case EffectBrush.BrushType.Linear:
                        {
                            if (!rect.IsEmpty)
                            {
                                (brush as LinearGradientBrush).TranslateTransform(rect.X, rect.Y);
                                (brush as LinearGradientBrush).ScaleTransform(rect.Width * 100 / effectConfig.gradient_size, rect.Height * 100 / effectConfig.gradient_size);
                            }
                            else
                            {
                                (brush as LinearGradientBrush).ScaleTransform(Effects.CanvasHeight * 100 / effectConfig.gradient_size, Effects.CanvasHeight * 100 / effectConfig.gradient_size);
                            }

                            (brush as LinearGradientBrush).RotateTransform(effectConfig.angle);
                            (brush as LinearGradientBrush).TranslateTransform(shift, shift);
                            break;
                        }
                        case EffectBrush.BrushType.Radial:
                        {
                            if (effectConfig.animation_type == AnimationType.Zoom_in || effectConfig.animation_type == AnimationType.Zoom_out)
                            {
                                float percent = shift / Effects.CanvasBiggest;

                                float x_offset = Effects.CanvasWidth / 2.0f * percent;
                                float y_offset = Effects.CanvasHeight / 2.0f * percent;


                                (brush as PathGradientBrush).WrapMode = WrapMode.Clamp;

                                if (!rect.IsEmpty)
                                {
                                    x_offset = rect.Width / 2.0f * percent;
                                    y_offset = rect.Height / 2.0f * percent;

                                    (brush as PathGradientBrush).TranslateTransform(rect.X + x_offset, rect.Y + y_offset);
                                    (brush as PathGradientBrush).ScaleTransform((rect.Width - 2.0f * x_offset) * 100 / effectConfig.gradient_size, (rect.Height - 2.0f * y_offset) * 100 / effectConfig.gradient_size);
                                }
                                else
                                {
                                    (brush as PathGradientBrush).ScaleTransform((Effects.CanvasHeight + x_offset) * 100 / effectConfig.gradient_size, (Effects.CanvasHeight + y_offset) * 100 / effectConfig.gradient_size);
                                }
                            }
                            else
                            {
                                if (!rect.IsEmpty)
                                {
                                    (brush as PathGradientBrush).TranslateTransform(rect.X, rect.Y);
                                    (brush as PathGradientBrush).ScaleTransform(rect.Width * 100 / effectConfig.gradient_size, rect.Height * 100 / effectConfig.gradient_size);
                                }
                                else
                                {
                                    (brush as PathGradientBrush).ScaleTransform(Effects.CanvasHeight * 100 / effectConfig.gradient_size, Effects.CanvasHeight * 100 / effectConfig.gradient_size);
                                }
                            }

                            (brush as PathGradientBrush).RotateTransform(effectConfig.angle);

                            //(brush as PathGradientBrush).TranslateTransform(x_shift, y_shift);
                            break;
                        }
                    }

                    Fill(brush);

                    effectConfig.last_effect_call = Time.GetMillisecondsSinceEpoch();
                    break;
            }
        }

        public void Dispose()
        {
            _textureBrush.Dispose();
            _textureBrush = null;
            _colormap.Dispose();
            Effects.CanvasChanged -= Invalidate;
        }

        /// <summary>
        /// Creates a rainbow gradient brush, to be used in effects.
        /// </summary>
        /// <returns>Rainbow LinearGradientBrush</returns>
        private LinearGradientBrush CreateRainbowBrush()
        {
            var brush = new LinearGradientBrush(
                new Point(0, 0),
                new Point(Effects.CanvasBiggest, 0),
                Color.Red, Color.Red);
            Color[] colors = {
                Color.FromArgb(255, 0, 0),
                Color.FromArgb(255, 127, 0),
                Color.FromArgb(255, 255, 0),
                Color.FromArgb(0, 255, 0),
                Color.FromArgb(0, 0, 255),
                Color.FromArgb(75, 0, 130),
                Color.FromArgb(139, 0, 255),
                Color.FromArgb(255, 0, 0)
            };
            var numColors = colors.Length;
            var blendPositions = new float[numColors];
            for (var i = 0; i < numColors; i++)
            {
                blendPositions[i] = i / (numColors - 1f);
            }

            var colorBlend = new ColorBlend();
            colorBlend.Colors = colors;
            colorBlend.Positions = blendPositions;
            brush.InterpolationColors = colorBlend;

            return brush;
        }

        /// <summary>
        /// Fills the entire bitmap of the EffectLayer with a specified brush.
        /// </summary>
        /// <param name="brush">Brush to be used during bitmap fill</param>
        public void Fill(Brush brush)
        {
            using var g = Graphics.FromImage(_colormap);
            g.CompositingMode = CompositingMode.SourceCopy;
            g.FillRectangle(brush, Dimension);
            Invalidate();
        }

        /// <summary>
        /// Paints over the entire bitmap of the EffectLayer with a specified color.
        /// </summary>
        /// <param name="color">Color to be used during bitmap fill</param>
        /// <returns>Itself</returns>
        [Obsolete("Use with Brush argument")]
        public EffectLayer FillOver(Color color)
        {
            using (var g = GetGraphics())
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.SmoothingMode = SmoothingMode.None;
                g.FillRectangle(new SolidBrush(color), Dimension);
                Invalidate();
            }

            return this;
        }

        /// <summary>
        /// Paints over the entire bitmap of the EffectLayer with a specified color.
        /// </summary>
        /// <param name="color">Color to be used during bitmap fill</param>
        /// <returns>Itself</returns>
        public void FillOver(Brush brush)
        {
            using var g = GetGraphics();
            g.CompositingMode = CompositingMode.SourceOver;
            g.SmoothingMode = SmoothingMode.None;
            g.FillRectangle(brush, Dimension);
            Invalidate();
        }
        
        public void Clear()
        {
            using var g = GetGraphics();
            g.CompositingMode = CompositingMode.SourceCopy;
            g.FillRectangle(ClearingBrush, Dimension);
            Invalidate();
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

        private FreeFormObject _lastFreeform = new();
        private bool _ksChanged = true;
        /// <summary>
        /// Sets a specific KeySequence on the bitmap with a specified brush.
        /// </summary>
        /// <param name="sequence">KeySequence to specify what regions of the bitmap need to be changed</param>
        /// <param name="brush">Brush to be used</param>
        /// <returns>Itself</returns>
        public EffectLayer Set(KeySequence sequence, Brush brush)
        {
            if (_previousSequenceType != sequence.type)
            {
                Clear();
                _previousSequenceType = sequence.type;
            }

            if (sequence.type == KeySequenceType.Sequence)
            {
                foreach (var key in sequence.keys)
                    SetOneKey(key, brush);
            }
            else
            {
                if (brush is SolidBrush solidBrush)
                {
                    if (sequence.freeform.Equals(_lastFreeform) && !_ksChanged && _lastColor == solidBrush.Color)
                    {
                        return this;
                    }

                    _lastColor = solidBrush.Color;
                }

                Clear();

                if (!sequence.freeform.Equals(_lastFreeform))
                {
                    _lastFreeform.ValuesChanged -= FreeformOnValuesChanged;
                    _lastFreeform = sequence.freeform;
                    sequence.freeform.ValuesChanged += FreeformOnValuesChanged;
                }
                _ksChanged = false;

                using var g = Graphics.FromImage(_colormap);
                var xPos = (float)Math.Round((sequence.freeform.X + Effects.grid_baseline_x) * Effects.EditorToCanvasWidth);
                var yPos = (float)Math.Round((sequence.freeform.Y + Effects.grid_baseline_y) * Effects.EditorToCanvasHeight);
                var width = (float)Math.Round(sequence.freeform.Width * Effects.EditorToCanvasWidth);
                var height = (float)Math.Round(sequence.freeform.Height * Effects.EditorToCanvasHeight);

                if (width < 3) width = 3;
                if (height < 3) height = 3;

                var rect = new RectangleF(xPos, yPos, width, height);

                var rotatePoint = new PointF(xPos + width / 2.0f, yPos + height / 2.0f);

                var myMatrix = new Matrix();
                myMatrix.RotateAt(sequence.freeform.Angle, rotatePoint, MatrixOrder.Append);

                g.Transform = myMatrix;
                g.CompositingMode = CompositingMode.SourceCopy;
                g.FillRectangle(brush, rect);
                Invalidate();
            }

            return this;
        }

        private void FreeformOnValuesChanged(FreeFormObject newobject)
        {
            _ksChanged = true;
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
                    matrix.RotateAt(sequence.freeform.Angle, new PointF((sourceRegion.Left + sourceRegion.Width / 2f) * sx, (sourceRegion.Top + sourceRegion.Height / 2f) * sy), MatrixOrder.Append);

                // Third, we can translate the matrix from the source to the target location.
                matrix.Translate(bounds.X - sourceRegion.Left, bounds.Y - sourceRegion.Top, MatrixOrder.Append);

                // Finally, call the custom matrix configure action
                configureMatrix(matrix);

                // Apply the matrix transform to the graphics context and then render
                gfx.Transform = matrix;
                render(gfx);
            }

            Invalidate();
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
            DrawTransformed(sequence, render, new RectangleF(0, 0, Effects.CanvasWidth, Effects.CanvasHeight));
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

        private readonly Dictionary<DeviceKeys, Color> _keyColors = new();
        private static readonly SolidBrush ClearingBrush = new(Color.Transparent);
        private Color _lastColor = Color.Empty;

        /// <summary>
        /// Sets one DeviceKeys key with a specific brush on the bitmap
        /// </summary>
        /// <param name="key">DeviceKey to be set</param>
        /// <param name="brush">Brush to be used</param>
        private void SetOneKey(DeviceKeys key, Brush brush)
        {
            if (brush is SolidBrush solidBrush)
            {
                if (_keyColors.TryGetValue(key, out var currentColor) && currentColor == solidBrush.Color)
                {
                    return;
                }
                _keyColors[key] = solidBrush.Color;
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
                if (keymaping.Top < 0 || keymaping.Bottom > Effects.CanvasHeight ||
                    keymaping.Left < 0 || keymaping.Right > Effects.CanvasWidth)
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
            if (_keyColors.TryGetValue(key, out var color))
            {
                return color;
            }
            try
            {
                var keyMaping = Effects.GetBitmappingFromDeviceKey(key);

                var keyColor = keyMaping.IsEmpty switch
                {
                    true when key == DeviceKeys.Peripheral => _peripheral,
                    true => Color.Black,
                    _ => BitmapUtils.GetRegionColor(_colormap, keyMaping.Rectangle)
                };
                _keyColors[key] = keyColor;
                return keyColor;
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
            Invalidate();
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

        public void Add(EffectLayer other)
        {
            var _ = this + other;
        }

        /// <summary>
        /// + Operator, sums two EffectLayer together.
        /// </summary>
        /// <param name="lhs">Left Hand Side EffectLayer</param>
        /// <param name="rhs">Right Hand Side EffectLayer</param>
        /// <returns>Left hand side EffectLayer, which is a combination of two passed EffectLayers</returns>
        public static EffectLayer operator +(EffectLayer lhs, EffectLayer rhs)
        {
            using (var g = lhs.GetGraphics())
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.Low;
                g.FillRectangle(rhs.TextureBrush, rhs.Dimension);
            }
            lhs.Invalidate();

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
            if (!ColorUtils.NearlyEqual(layer._opacity,(float)value, 0.0001f))
            {
                layer._opacity = (float) value;
                layer.Invalidate();
            }
            return layer;
        }

        private KeySequenceType _previousSequenceType;
        
        /// <summary>
        /// Draws a percent effect on the layer bitmap using a KeySequence with solid colors.
        /// </summary>
        /// <param name="foregroundColor">The foreground color, used as a "Progress bar color"</param>
        /// <param name="sequence">The sequence of keys that the percent effect will be drawn on</param>
        /// <param name="value">The current progress value</param>
        /// <param name="total">The maxiumum progress value</param>
        /// <param name="flashPast"></param>
        /// <param name="flashReversed"></param>
        /// <param name="blinkBackground"></param>
        public void PercentEffect(Color foregroundColor, Color backgroundColor, KeySequence sequence, double value,
            double total = 1.0D, PercentEffectType percentEffectType = PercentEffectType.Progressive,
            double flashPast = 0.0, bool flashReversed = false, bool blinkBackground = false)
        {
            if (_previousSequenceType != sequence.type)
            {
                Clear();
                _previousSequenceType = sequence.type;
            }
            if (sequence.type == KeySequenceType.Sequence)
                PercentEffect(foregroundColor, backgroundColor, sequence.keys.ToArray(), value, total, percentEffectType, flashPast, flashReversed, blinkBackground);
            else
                PercentEffect(foregroundColor, backgroundColor, sequence.freeform, value, total, percentEffectType, flashPast, flashReversed, blinkBackground);
        }

        /// <summary>
        /// Draws a percent effect on the layer bitmap using a KeySequence and a ColorSpectrum.
        /// </summary>
        /// <param name="spectrum">The ColorSpectrum to be used as a "Progress bar"</param>
        /// <param name="sequence">The sequence of keys that the percent effect will be drawn on</param>
        /// <param name="value">The current progress value</param>
        /// <param name="total">The maxiumum progress value</param>
        public void PercentEffect(ColorSpectrum spectrum, KeySequence sequence, double value, double total = 1.0D,
            PercentEffectType percentEffectType = PercentEffectType.Progressive, double flashPast = 0.0,
            bool flashReversed = false)
        {
            if (_previousSequenceType != sequence.type)
            {
                Clear();
                _previousSequenceType = sequence.type;
            }
            if (sequence.type == KeySequenceType.Sequence)
                PercentEffect(spectrum, sequence.keys.ToArray(), value, total, percentEffectType, flashPast, flashReversed);
            else
                PercentEffect(spectrum, sequence.freeform, value, total, percentEffectType, flashPast, flashReversed);
        }

        /// <summary>
        /// Draws a percent effect on the layer bitmap using an array of DeviceKeys keys and solid colors.
        /// </summary>
        /// <param name="foregroundColor">The foreground color, used as a "Progress bar color"</param>
        /// <param name="keys">The array of keys that the percent effect will be drawn on</param>
        /// <param name="value">The current progress value</param>
        /// <param name="total">The maxiumum progress value</param>
        private void PercentEffect(Color foregroundColor, Color backgroundColor, DeviceKeys[] keys, double value,
            double total, PercentEffectType percentEffectType = PercentEffectType.Progressive, double flashPast = 0.0,
            bool flashReversed = false, bool blinkBackground = false)
        {
            var progressTotal = value / total;
            if (progressTotal < 0.0)
                progressTotal = 0.0;
            else if (progressTotal > 1.0)
                progressTotal = 1.0;

            var progress = progressTotal * keys.Length;

            if (flashPast > 0.0)
            {
                if ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast))
                {
                    if (blinkBackground)
                        backgroundColor = ColorUtils.BlendColors(backgroundColor, Color.Empty, Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI));
                    else
                        foregroundColor = ColorUtils.BlendColors(backgroundColor, foregroundColor, Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI));
                }
            }

            if (percentEffectType is PercentEffectType.Highest_Key or PercentEffectType.Highest_Key_Blend && keys.Length > 0)
            {
                var activeKey = (int)Math.Ceiling(value / (total / keys.Length)) - 1;
                var col = percentEffectType == PercentEffectType.Highest_Key ? foregroundColor : ColorUtils.BlendColors(backgroundColor, foregroundColor, progressTotal);
                SetOneKey(keys[Math.Min(Math.Max(activeKey, 0), keys.Length - 1)], col);

            }
            else
            {
                for (var i = 0; i < keys.Length; i++)
                {
                    var currentKey = keys[i];

                    switch (percentEffectType)
                    {
                        case PercentEffectType.AllAtOnce:
                            SetOneKey(currentKey, ColorUtils.BlendColors(backgroundColor, foregroundColor, progressTotal));
                            break;
                        case PercentEffectType.Progressive_Gradual:
                            if (i == (int)progress)
                            {
                                var percent = progress - i;
                                SetOneKey(currentKey, ColorUtils.BlendColors(backgroundColor, foregroundColor, percent));
                            }
                            else if (i < (int)progress)
                                SetOneKey(currentKey, foregroundColor);
                            else
                                SetOneKey(currentKey, backgroundColor);
                            break;
                        default:
                            SetOneKey(currentKey, i < (int) progress ? foregroundColor : backgroundColor);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Draws a percent effect on the layer bitmap using DeviceKeys keys and a ColorSpectrum.
        /// </summary>
        /// <param name="spectrum">The ColorSpectrum to be used as a "Progress bar"</param>
        /// <param name="keys">The array of keys that the percent effect will be drawn on</param>
        /// <param name="value">The current progress value</param>
        /// <param name="total">The maxiumum progress value</param>
        private void PercentEffect(ColorSpectrum spectrum, DeviceKeys[] keys, double value, double total,
            PercentEffectType percentEffectType = PercentEffectType.Progressive, double flashPast = 0.0,
            bool flashReversed = false)
        {
            var progressTotal = value / total;
            if (progressTotal < 0.0)
                progressTotal = 0.0;
            else if (progressTotal > 1.0)
                progressTotal = 1.0;

            var progress = progressTotal * keys.Length;

            var flashAmount = 1.0;

            if (flashPast > 0.0)
            {
                if ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast))
                {
                    flashAmount = Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI);
                }
            }

            switch (percentEffectType)
            {
                case PercentEffectType.AllAtOnce:
                    foreach (var currentKey in keys)
                    {
                        SetOneKey(currentKey, spectrum.GetColorAt((float)progressTotal, 1.0f, flashAmount));
                    }
                    break;
                case PercentEffectType.Progressive_Gradual:
                    Clear();
                    for (var i = 0; i < keys.Length; i++)
                    {
                        var currentKey = keys[i];
                        if (i == (int)progress)
                        {
                            var percent = progress - i;
                            SetOneKey(
                                currentKey,
                                ColorUtils.MultiplyColorByScalar(spectrum.GetColorAt((float)i / (keys.Length - 1), 1.0f, flashAmount), percent)
                            );
                        }
                        else if (i < (int)progress)
                            SetOneKey(currentKey, spectrum.GetColorAt((float)i / (keys.Length - 1), 1.0f, flashAmount));
                    }
                    break;
                default:
                    for (var i = 0; i < keys.Length; i++)
                    {
                        var currentKey = keys[i];
                        if (i < (int)progress)
                            SetOneKey(currentKey, spectrum.GetColorAt((float)i / (keys.Length - 1), 1.0f, flashAmount));
                    }
                    break;
            }
        }

        /// <summary>
        /// Draws a percent effect on the layer bitmap using a FreeFormObject and solid colors.
        /// </summary>
        /// <param name="foregroundColor">The foreground color, used as a "Progress bar color"</param>
        /// <param name="freeform">The FreeFormObject that the progress effect will be drawn on</param>
        /// <param name="value">The current progress value</param>
        /// <param name="total">The maxiumum progress value</param>
        private void PercentEffect(Color foregroundColor, Color backgroundColor, FreeFormObject freeform, double value,
            double total, PercentEffectType percentEffectType = PercentEffectType.Progressive, double flashPast = 0.0,
            bool flashReversed = false, bool blinkBackground = false)
        {
            var progressTotal = value / total;
            switch (progressTotal)
            {
                case < 0.0:
                case double.NaN:
                    progressTotal = 0.0;
                    break;
                case > 1.0:
                    progressTotal = 1.0;
                    break;
            }

            if (flashPast > 0.0)
            {
                if ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast))
                {
                    switch (blinkBackground)
                    {
                        case false:
                            foregroundColor = ColorUtils.BlendColors(backgroundColor, foregroundColor, Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI));
                            break;
                        case true:
                            backgroundColor = ColorUtils.BlendColors(backgroundColor, Color.Empty, Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI));
                            break;
                    }
                }
            }

            var xPos = (float)Math.Round((freeform.X + Effects.grid_baseline_x) * Effects.EditorToCanvasWidth);
            var yPos = (float)Math.Round((freeform.Y + Effects.grid_baseline_y) * Effects.EditorToCanvasHeight);
            var width = freeform.Width * Effects.EditorToCanvasWidth;
            var height = freeform.Height * Effects.EditorToCanvasHeight;

            if (width < 3) width = 3;
            if (height < 3) height = 3;
            
            using var g = Graphics.FromImage(_colormap);
            if (percentEffectType == PercentEffectType.AllAtOnce)
            {
                var rect = new RectangleF(xPos, yPos, width, height);

                var rotatePoint = new PointF(xPos + width / 2.0f, yPos + height / 2.0f);

                var myMatrix = new Matrix();
                myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

                g.Transform = myMatrix;
                g.FillRectangle(new SolidBrush(ColorUtils.BlendColors(backgroundColor, foregroundColor, progressTotal)), rect);
                Invalidate();
            }
            else
            {
                var progress = progressTotal * width;
                var rect = new RectangleF(xPos, yPos, (float)progress, height);
                var rotatePoint = new PointF(xPos + width / 2.0f, yPos + height / 2.0f);

                var myMatrix = new Matrix();
                myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

                g.Transform = myMatrix;

                var rectRest = new RectangleF(xPos, yPos, width, height);
                g.FillRectangle(new SolidBrush(backgroundColor), rectRest);
                g.FillRectangle(new SolidBrush(foregroundColor), rect);
                Invalidate();
            }
        }

        private void Invalidate()
        {
            _needsRender = true;
            _keyColors.Clear();
        }
        private void Invalidate(object sender, object args)
        {
            _colormap.Dispose();
            _colormap = new Bitmap(Effects.CanvasWidth, Effects.CanvasHeight);
            Dimension.Height = Effects.CanvasHeight;
            Dimension.Width = Effects.CanvasWidth;
            Invalidate();
        }

        /// <summary>
        ///  Draws a percent effect on the layer bitmap using a FreeFormObject and a ColorSpectrum.
        /// </summary>
        /// <param name="spectrum">The ColorSpectrum to be used as a "Progress bar"</param>
        /// <param name="freeform">The FreeFormObject that the progress effect will be drawn on</param>
        /// <param name="value">The current progress value</param>
        /// <param name="total">The maxiumum progress value</param>
        private void PercentEffect(ColorSpectrum spectrum, FreeFormObject freeform, double value, double total,
            PercentEffectType percentEffectType = PercentEffectType.Progressive, double flashPast = 0.0,
            bool flashReversed = false)
        {
            var progressTotal = value / total;
            if (progressTotal < 0.0)
                progressTotal = 0.0;
            else if (progressTotal > 1.0)
                progressTotal = 1.0;

            var flashAmount = 1.0;

            if (flashPast > 0.0)
            {
                if ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast))
                {
                    flashAmount = Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI);
                }
            }

            var xPos = (float)Math.Round((freeform.X + Effects.grid_baseline_x) * Effects.EditorToCanvasWidth);
            var yPos = (float)Math.Round((freeform.Y + Effects.grid_baseline_y) * Effects.EditorToCanvasHeight);
            var width = freeform.Width * Effects.EditorToCanvasWidth;
            var height = freeform.Height * Effects.EditorToCanvasHeight;

            if (width < 3) width = 3;
            if (height < 3) height = 3;

            using var g = Graphics.FromImage(_colormap);
            if (percentEffectType == PercentEffectType.AllAtOnce)
            {
                var rect = new RectangleF(xPos, yPos, width, height);

                var rotatePoint = new PointF(xPos + width / 2.0f, yPos + height / 2.0f);

                var myMatrix = new Matrix();
                myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

                g.Transform = myMatrix;
                g.FillRectangle(new SolidBrush(spectrum.GetColorAt((float)progressTotal, 1.0f, flashAmount)), rect);
            }
            else
            {
                var progress = progressTotal * width;

                var rect = new RectangleF(xPos, yPos, (float)progress, height);

                var rotatePoint = new PointF(xPos + width / 2.0f, yPos + height / 2.0f);

                var myMatrix = new Matrix();
                myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

                g.Transform = myMatrix;
                var brush = spectrum.ToLinearGradient(width, 0, xPos, 0, flashAmount);
                brush.WrapMode = WrapMode.Tile;
                g.FillRectangle(brush, rect);
            }
            Invalidate();
        }

        /// <summary>
        /// Excludes provided sequence from the layer (Applies a mask)
        /// </summary>
        /// <param name="sequence">The mask to be applied</param>
        public void Exclude(KeySequence sequence)
        {
            //Create draw alpha mask
            var alphaMask = new EffectLayer(_name + " - Alpha Mask", Color.Transparent);
            alphaMask.Set(sequence, Color.Black);

            //Apply alpha mask
            BitmapData srcData_alpha = alphaMask._colormap.LockBits(
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
                        byte mask_alpha = p_alpha[y * alpha_mask_stride + x * 4 + 3];
                        if (mask_alpha != 0)
                            p[y * stride + x * 4 + 3] = (byte)(255 - mask_alpha);
                    }
                }
            }

            alphaMask._colormap.UnlockBits(srcData_alpha);
            _colormap.UnlockBits(srcData);
            Invalidate();
        }

        /// <summary>
        /// Inlcudes provided sequence from the layer (Applies a mask)
        /// </summary>
        /// <param name="sequence">The mask to be applied</param>
        public void OnlyInclude(KeySequence sequence)
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
                        byte mask_alpha = p_alpha[y * alpha_mask_stride + x * 4 + 3];

                        p[y * stride + x * 4 + 3] = (byte)(mask_alpha * (p[y * stride + x * 4 + 3] / 255.0f));
                    }
                }
            }

            _alpha_mask._colormap.UnlockBits(srcData_alpha);
            _colormap.UnlockBits(srcData);
            Invalidate();
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
