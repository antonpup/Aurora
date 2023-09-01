using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows;
using Aurora.Devices;
using Aurora.Settings;
using Aurora.Utils;
using Point = System.Drawing.Point;

namespace Aurora.EffectsEngine
{
    /// <summary>
    /// A class representing a bitmap layer for effects
    /// </summary>
    public sealed class EffectLayer : IDisposable
    {
        private static readonly Lazy<EffectLayer> EmptyLayerFactory = new(
            () => new EffectLayer("EmptyLayer", true), LazyThreadSafetyMode.PublicationOnly);
        public static EffectLayer EmptyLayer => EmptyLayerFactory.Value;

        private readonly string _name;
        private Bitmap _colormap;
        private float _opacity = 1;

        private TextureBrush? _textureBrush;
        private bool _needsRender = true;

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
                
                PerformExclude(_excludeSequence);

                _textureBrush = new TextureBrush(_colormap, Dimension, imageAttributes);
                _needsRender = false;

                return _textureBrush;
            }
        }

        [Obsolete("This creates too much garbage memory")]
        public EffectLayer(string name) : this(name, Color.FromArgb(0, 1, 1, 1))
        {
        }

        [Obsolete("This creates too much garbage memory")]
        public EffectLayer(string name, Color color)
        {
            _name = name;
            _colormap = new Bitmap(Effects.CanvasWidth, Effects.CanvasHeight);
            _textureBrush = new TextureBrush(_colormap);
            Dimension = new Rectangle(0, 0, Effects.CanvasWidth, Effects.CanvasHeight);

            FillOver(color);
        }

        public EffectLayer(string name, bool persistent) : this(name)
        {
            if (!persistent)
                return;
            WeakEventManager<Effects, EventArgs>.AddHandler(null, nameof(Effects.CanvasChanged), InvalidateColorMap);
        }

        public EffectLayer(string name, Color color, bool persistent) : this(name, color)
        {
            if (!persistent)
                return;
            WeakEventManager<Effects, EventArgs>.AddHandler(null, nameof(Effects.CanvasChanged), InvalidateColorMap);
        }

        /// <summary>
        /// Creates a new instance of the EffectLayer class with a specified layer name. And applies a LayerEffect onto this EffectLayer instance.
        /// Using the parameters from LayerEffectConfig and a specified region in RectangleF
        /// </summary>
        /// <param name="effect">An enum specifying which LayerEffect to apply</param>
        /// <param name="effectConfig">Configurations for the LayerEffect</param>
        /// <param name="rect">A rectangle specifying what region to apply effects in</param>
        public void DrawGradient(LayerEffects effect, LayerEffectConfig effectConfig, RectangleF rect = new())
        {
            Clear();
            LinearGradientBrush brush;
            var shift = 0.0f;

            switch (effect)
            {
                case LayerEffects.ColorOverlay:
                    FillOver(effectConfig.Primary);
                    break;
                case LayerEffects.ColorBreathing:
                    FillOver(effectConfig.Primary);
                    var sine = (float)Math.Pow(Math.Sin((double)(Time.GetMillisecondsSinceEpoch() % 10000L / 10000.0f) * 2 * Math.PI * effectConfig.Speed), 2);
                    FillOver(Color.FromArgb((byte)(sine * 255), effectConfig.Secondary));
                    break;
                case LayerEffects.RainbowShift_Horizontal:
                    effectConfig.ShiftAmount += (Time.GetMillisecondsSinceEpoch() - effectConfig.LastEffectCall) / 1000.0f * 5.0f * effectConfig.Speed;
                    effectConfig.ShiftAmount %= Effects.CanvasBiggest;

                    if (effectConfig.AnimationType == AnimationType.TranslateXy)
                        shift = effectConfig.ShiftAmount;

                    if (effectConfig.AnimationReverse)
                        shift *= -1.0f;

                    brush = CreateRainbowBrush();
                    brush.RotateTransform(0.0f);
                    brush.TranslateTransform(shift, shift);

                    Fill(brush);

                    effectConfig.LastEffectCall = Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.RainbowShift_Vertical:
                    effectConfig.ShiftAmount += (Time.GetMillisecondsSinceEpoch() - effectConfig.LastEffectCall) / 1000.0f * 5.0f * effectConfig.Speed;
                    effectConfig.ShiftAmount %= Effects.CanvasBiggest;

                    if (effectConfig.AnimationType == AnimationType.TranslateXy)
                        shift = effectConfig.ShiftAmount;

                    if (effectConfig.AnimationReverse)
                        shift *= -1.0f;

                    brush = CreateRainbowBrush();
                    brush.RotateTransform(90.0f);
                    brush.TranslateTransform(shift, shift);

                    Fill(brush);

                    effectConfig.LastEffectCall = Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.RainbowShift_Diagonal:
                    effectConfig.ShiftAmount += (Time.GetMillisecondsSinceEpoch() - effectConfig.LastEffectCall) / 1000.0f * 5.0f * effectConfig.Speed;
                    effectConfig.ShiftAmount %= Effects.CanvasBiggest;

                    if (effectConfig.AnimationType == AnimationType.TranslateXy)
                        shift = effectConfig.ShiftAmount;

                    if (effectConfig.AnimationReverse)
                        shift *= -1.0f;

                    brush = CreateRainbowBrush();
                    brush.RotateTransform(45.0f);
                    brush.TranslateTransform(shift, shift);

                    Fill(brush);

                    effectConfig.LastEffectCall = Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.RainbowShift_Diagonal_Other:
                    effectConfig.ShiftAmount += (Time.GetMillisecondsSinceEpoch() - effectConfig.LastEffectCall) / 1000.0f * 5.0f * effectConfig.Speed;
                    effectConfig.ShiftAmount %= Effects.CanvasBiggest;

                    if (effectConfig.AnimationType == AnimationType.TranslateXy)
                        shift = effectConfig.ShiftAmount;

                    if (effectConfig.AnimationReverse)
                        shift *= -1.0f;

                    brush = CreateRainbowBrush();
                    brush.RotateTransform(-45.0f);
                    brush.TranslateTransform(shift, shift);

                    Fill(brush);

                    effectConfig.LastEffectCall = Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.RainbowShift_Custom_Angle:
                    effectConfig.ShiftAmount += (Time.GetMillisecondsSinceEpoch() - effectConfig.LastEffectCall) / 1000.0f * 5.0f * effectConfig.Speed;
                    effectConfig.ShiftAmount %= Effects.CanvasBiggest;


                    if (effectConfig.AnimationType == AnimationType.TranslateXy)
                        shift = effectConfig.ShiftAmount;

                    if (effectConfig.AnimationReverse)
                        shift *= -1.0f;

                    brush = CreateRainbowBrush();
                    brush.RotateTransform(effectConfig.Angle);
                    brush.TranslateTransform(shift, shift);

                    Fill(brush);

                    effectConfig.LastEffectCall = Time.GetMillisecondsSinceEpoch();
                    break;
                case LayerEffects.GradientShift_Custom_Angle:
                {
                    effectConfig.ShiftAmount += (Time.GetMillisecondsSinceEpoch() - effectConfig.LastEffectCall) / 1000.0f * 0.067f * effectConfig.Speed;
                    effectConfig.ShiftAmount %= Effects.CanvasBiggest;

                    shift = effectConfig.AnimationType switch
                    {
                        AnimationType.TranslateXy => effectConfig.ShiftAmount,
                        AnimationType.ZoomIn when effectConfig.Brush.type == EffectBrush.BrushType.Radial =>
                            (Effects.CanvasBiggest - effectConfig.ShiftAmount) * 40.0f % Effects.CanvasBiggest,
                        AnimationType.ZoomOut when effectConfig.Brush.type == EffectBrush.BrushType.Radial =>
                            effectConfig.ShiftAmount * 40.0f % Effects.CanvasBiggest,
                        _ => shift
                    };

                    if (effectConfig.AnimationReverse)
                        shift *= -1.0f;

                    brush = (LinearGradientBrush) effectConfig.Brush.GetDrawingBrush();
                    switch (effectConfig.Brush.type)
                    {
                        case EffectBrush.BrushType.Linear:
                        {
                            if (!rect.IsEmpty)
                            {
                                brush.TranslateTransform(rect.X, rect.Y);
                                brush.ScaleTransform(rect.Width * 100 / effectConfig.GradientSize, rect.Height * 100 / effectConfig.GradientSize);
                            }
                            else
                            {
                                brush.ScaleTransform(Effects.CanvasHeight * 100 / effectConfig.GradientSize, Effects.CanvasHeight * 100 / effectConfig.GradientSize);
                            }

                            brush.RotateTransform(effectConfig.Angle);
                            brush.TranslateTransform(shift, shift);
                            break;
                        }
                        case EffectBrush.BrushType.Radial:
                        {
                            if (effectConfig.AnimationType == AnimationType.ZoomIn || effectConfig.AnimationType == AnimationType.ZoomOut)
                            {
                                float percent = shift / Effects.CanvasBiggest;
                                float xOffset = Effects.CanvasWidth / 2.0f * percent;
                                float yOffset = Effects.CanvasHeight / 2.0f * percent;


                                brush.WrapMode = WrapMode.Clamp;

                                if (!rect.IsEmpty)
                                {
                                    xOffset = rect.Width / 2.0f * percent;
                                    yOffset = rect.Height / 2.0f * percent;

                                    brush.TranslateTransform(rect.X + xOffset, rect.Y + yOffset);
                                    brush.ScaleTransform((rect.Width - 2.0f * xOffset) * 100 / effectConfig.GradientSize, (rect.Height - 2.0f * yOffset) * 100 / effectConfig.GradientSize);
                                }
                                else
                                {
                                    brush.ScaleTransform((Effects.CanvasHeight + xOffset) * 100 / effectConfig.GradientSize, (Effects.CanvasHeight + yOffset) * 100 / effectConfig.GradientSize);
                                }
                            }
                            else
                            {
                                if (!rect.IsEmpty)
                                {
                                    brush.TranslateTransform(rect.X, rect.Y);
                                    brush.ScaleTransform(rect.Width * 100 / effectConfig.GradientSize, rect.Height * 100 / effectConfig.GradientSize);
                                }
                                else
                                {
                                    brush.ScaleTransform(Effects.CanvasHeight * 100 / effectConfig.GradientSize, Effects.CanvasHeight * 100 / effectConfig.GradientSize);
                                }
                            }

                            brush.RotateTransform(effectConfig.Angle);
                            break;
                        }
                    }

                    Fill(brush);

                    effectConfig.LastEffectCall = Time.GetMillisecondsSinceEpoch();
                    break;
                }
            }
        }

        public void Dispose()
        {
            _keyColors.Clear();
            _excludeSequence = null;
            _keySequence = null;
            _lastFreeform = null;
            _colormap?.Dispose();
            _textureBrush?.Dispose();
            _textureBrush = null;
            _transformedDrawExcludeMap?.Dispose();
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
        public void FillOver(Color color)
        {
            using (var g = Graphics.FromImage(_colormap))
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.SmoothingMode = SmoothingMode.None;
                g.FillRectangle(new SolidBrush(color), Dimension);
            }

            Invalidate();
        }

        /// <summary>
        /// Paints over the entire bitmap of the EffectLayer with a specified color.
        /// </summary>
        /// <param name="color">Color to be used during bitmap fill</param>
        /// <returns>Itself</returns>
        public void FillOver(Brush brush)
        {
            using var g = Graphics.FromImage(_colormap);
            g.CompositingMode = CompositingMode.SourceOver;
            g.SmoothingMode = SmoothingMode.None;
            g.FillRectangle(brush, Dimension);
            Invalidate();
        }
        
        public void Clear()
        {
            using var g = Graphics.FromImage(_colormap);
            g.CompositingMode = CompositingMode.SourceCopy;
            g.FillRectangle(ClearingBrush, Dimension);
            Invalidate();
        }

        private FreeFormObject _lastFreeform = new();
        private bool _ksChanged = true;
        /// <summary>
        /// Sets a specific Devices.DeviceKeys on the bitmap with a specified color.
        /// </summary>
        /// <param name="key">DeviceKey to be set</param>
        /// <param name="color">Color to be used</param>
        /// <returns>Itself</returns>
        public void Set(DeviceKeys key, Color color)
        {
            SetOneKey(key, color);
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
        public void Set(KeySequence sequence, Color color) => Set(sequence, new SolidBrush(color));

        /// <summary>
        /// Sets a specific KeySequence on the bitmap with a specified brush.
        /// </summary>
        /// <param name="sequence">KeySequence to specify what regions of the bitmap need to be changed</param>
        /// <param name="brush">Brush to be used</param>
        /// <returns>Itself</returns>
        public void Set(KeySequence sequence, Brush brush)
        {
            if (!ReferenceEquals(sequence, _keySequence))
            {
                WeakEventManager<ObservableCollection<DeviceKeys>, EventArgs>.RemoveHandler(_keySequence.Keys,
                    nameof(_keySequence.Keys.CollectionChanged), FreeformOnValuesChanged);
                _keySequence = sequence;
                WeakEventManager<ObservableCollection<DeviceKeys>, EventArgs>.AddHandler(_keySequence.Keys,
                    nameof(_keySequence.Keys.CollectionChanged), FreeformOnValuesChanged);
                FreeformOnValuesChanged(this, EventArgs.Empty);
            }
            if (_previousSequenceType != sequence.Type)
            {
                _previousSequenceType = sequence.Type;
                _ksChanged = true;
            }

            if (_ksChanged && !_needsRender)
            {
                Clear();
                _ksChanged = false;
            }

            if (sequence.Type == KeySequenceType.Sequence)
            {
                foreach (var key in sequence.Keys)
                    SetOneKey(key, brush);
            }
            else
            {
                if (!ReferenceEquals(sequence.Freeform, _lastFreeform))
                {
                    WeakEventManager<FreeFormObject, EventArgs>.RemoveHandler(_lastFreeform, nameof(_lastFreeform.ValuesChanged), FreeformOnValuesChanged);
                    _lastFreeform = sequence.Freeform;
                    WeakEventManager<FreeFormObject, EventArgs>.AddHandler(_lastFreeform, nameof(_lastFreeform.ValuesChanged), FreeformOnValuesChanged);
                    FreeformOnValuesChanged(this, EventArgs.Empty);
                }
                else if (brush is SolidBrush solidBrush)
                {
                    if (sequence.Freeform.Equals(_lastFreeform) && _lastColor == solidBrush.Color)
                    {
                        return;
                    }
                    _lastColor = solidBrush.Color;
                }

                using var g = Graphics.FromImage(_colormap);
                var xPos = (float)Math.Round((sequence.Freeform.X + Effects.GridBaselineX) * Effects.EditorToCanvasWidth);
                var yPos = (float)Math.Round((sequence.Freeform.Y + Effects.GridBaselineY) * Effects.EditorToCanvasHeight);
                var width = (float)Math.Round(sequence.Freeform.Width * Effects.EditorToCanvasWidth);
                var height = (float)Math.Round(sequence.Freeform.Height * Effects.EditorToCanvasHeight);

                if (width < 3) width = 3;
                if (height < 3) height = 3;

                var rect = new RectangleF(xPos, yPos, width, height);   //TODO dependant property? parameter?

                var rotatePoint = new PointF(xPos + width / 2.0f, yPos + height / 2.0f);
                var myMatrix = new Matrix();
                myMatrix.RotateAt(sequence.Freeform.Angle, rotatePoint, MatrixOrder.Append);    //TODO dependant property? parameter?

                g.Transform = myMatrix;
                g.CompositingMode = CompositingMode.SourceCopy;
                g.FillRectangle(brush, rect);
                Invalidate();
            }
        }

        private void FreeformOnValuesChanged(object? sender, EventArgs args)
        {
            _ksChanged = true;
        }

        private Bitmap _transformedDrawExcludeMap;

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
            // Note that we round the X and Y off to properly imitate the above `Set(KeySequence, Color)` method.
            // Unsure exactly why this is done, but it _is_ done to replicate behaviour properly.
            //  Also unsure why the X and Y are rounded using math.Round but Width and Height are just truncated using an int cast??
            var boundsRaw = sequence.GetAffectedRegion();
            var bounds = new RectangleF((int)Math.Round(boundsRaw.X), (int)Math.Round(boundsRaw.Y), (int)boundsRaw.Width, (int)boundsRaw.Height);

            using (var gfx = Graphics.FromImage(_colormap))
            {

                // First, calculate the scaling required to transform the sourceRect's size into the bounds' size
                float sx = bounds.Width / sourceRegion.Width, sy = bounds.Height / sourceRegion.Height;

                // Perform this scale first
                // Note: that if the scale is zero, when setting the graphics transform to the matrix,
                // it throws an error, so we must have NON-ZERO values
                // Note 2: Also tried using float.Epsilon but this also caused the exception,
                // so a somewhat small number will have to suffice. Not noticed any visual issues with 0.001f.
                matrix.Scale(sx == 0 ? .001f : sx, sy == 0 ? .001f : sy, MatrixOrder.Append);

                // Second, for freeform objects, apply the rotation. This needs to be done AFTER the scaling,
                // else the scaling is applied to the rotated object, which skews it
                // We rotate around the central point of the source region, but we need to take the scaling of the dimensions into account
                if (sequence.Type == KeySequenceType.FreeForm)
                    matrix.RotateAt(
                        sequence.Freeform.Angle,
                        new PointF((sourceRegion.Left + sourceRegion.Width / 2f) * sx, (sourceRegion.Top + sourceRegion.Height / 2f) * sy), MatrixOrder.Append);

                // Third, we can translate the matrix from the source to the target location.
                matrix.Translate(bounds.X - sourceRegion.Left, bounds.Y - sourceRegion.Top, MatrixOrder.Append);

                // Finally, call the custom matrix configure action
                configureMatrix(matrix);

                // Apply the matrix transform to the graphics context and then render
                gfx.ResetTransform();
                gfx.ResetClip();
                gfx.Transform = matrix;
                render(gfx);
                if (sequence.Type == KeySequenceType.Sequence)
                {
                    var gp = GetExclusionPath(sequence);
                    gfx.ResetTransform();
                    gfx.ResetClip();
                    gfx.SetClip(gp);
                    gfx.CompositingMode = CompositingMode.SourceOver;
                    gfx.Clear(Color.Transparent);
                }
            }

            Invalidate();
        }

        private static GraphicsPath GetExclusionPath(KeySequence sequence)
        {
            if (sequence.Type == KeySequenceType.Sequence)
            {
                var gp = new GraphicsPath();
                gp.AddRectangle(new Rectangle(0, 0, Effects.CanvasWidth, Effects.CanvasHeight));
                foreach (var k in sequence.Keys)
                {
                    var keyBounds = Effects.GetBitmappingFromDeviceKey(k);
                    gp.AddRectangle(keyBounds.Rectangle); //Overlapping additons remove that shape
                }
                return gp;
            }
            else
            {
                var gp = new GraphicsPath();
                gp.AddRectangle(new Rectangle(0, 0, Effects.CanvasWidth, Effects.CanvasHeight));
                gp.AddRectangle(sequence.GetAffectedRegion());
                return gp;
            }
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

        private readonly Dictionary<DeviceKeys, Color> _keyColors = new(Effects.MaxDeviceId);
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
                if (_keyColors.TryGetValue(key, out var currentColor) && currentColor.ToArgb() == solidBrush.Color.ToArgb())
                {
                    return;
                }
                _keyColors[key] = solidBrush.Color;
            }
            var keyRectangle = Effects.GetBitmappingFromDeviceKey(key);
            _needsRender = true;

            if (keyRectangle.Top < 0 || keyRectangle.Bottom > Effects.CanvasHeight ||
                keyRectangle.Left < 0 || keyRectangle.Right > Effects.CanvasWidth)
            {
                Global.logger.Warning("Couldn't set key color {Key}. Key is outside canvas", key);
                return;
            }

            try
            {
                using var g = Graphics.FromImage(_colormap);
                g.CompositingMode = CompositingMode.SourceCopy;
                g.FillRectangle(brush, keyRectangle.Rectangle);
            }catch { /* ignore */}
        }

        /// <summary>
        /// Retrieves a color of the specified DeviceKeys key from the bitmap
        /// </summary>
        public Color Get(DeviceKeys key)
        {
            if (_keyColors.TryGetValue(key, out var color))
            {
                return color;
            }
            var keyRectangle = Effects.GetBitmappingFromDeviceKey(key);

            var keyColor = keyRectangle.IsEmpty switch
            {
                true => Color.Black,
                false => BitmapUtils.GetRegionColor(_colormap, keyRectangle.Rectangle)
            };
            _keyColors[key] = keyColor;
            return keyColor;
        }

        public Graphics GetGraphics()   //TODO deprecate
        {
            Invalidate();
            return Graphics.FromImage(_colormap);
        }

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
            if (lhs == EmptyLayer)
            {
                return rhs;
            }

            if (rhs == EmptyLayer)
            {
                return lhs;
            }
            
            using (var g = lhs.GetGraphics())
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.Low;
                g.FillRectangle(rhs.TextureBrush, rhs.Dimension);
            }
            lhs.Invalidate();

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
        private KeySequence _keySequence = new();

        /// <summary>
        /// Draws a percent effect on the layer bitmap using a KeySequence with solid colors.
        /// </summary>
        /// <param name="foregroundColor">The foreground color, used as a "Progress bar color"</param>
        /// <param name="value">The current progress value</param>
        /// <param name="total">The maxiumum progress value</param>
        public void PercentEffect(Color foregroundColor, Color backgroundColor, KeySequence sequence, double value,
            double total = 1.0D, PercentEffectType percentEffectType = PercentEffectType.Progressive,
            double flashPast = 0.0, bool flashReversed = false, bool blinkBackground = false)
        {
            if (_previousSequenceType != sequence.Type)
            {
                Clear();
                _previousSequenceType = sequence.Type;
            }
            if (sequence.Type == KeySequenceType.Sequence)
                PercentEffect(foregroundColor, backgroundColor, sequence.Keys, value, total, percentEffectType,
                    flashPast, flashReversed, blinkBackground);
            else
                PercentEffect(foregroundColor, backgroundColor, sequence.Freeform, value, total, percentEffectType, flashPast, flashReversed, blinkBackground);
        }

        /// <summary>
        /// Draws a percent effect on the layer bitmap using a KeySequence and a ColorSpectrum.
        /// </summary>
        /// <param name="spectrum">The ColorSpectrum to be used as a "Progress bar"</param>
        /// <param name="value">The current progress value</param>
        /// <param name="total">The maxiumum progress value</param>
        public void PercentEffect(ColorSpectrum spectrum, KeySequence sequence, double value, double total = 1.0D,
            PercentEffectType percentEffectType = PercentEffectType.Progressive, double flashPast = 0.0,
            bool flashReversed = false)
        {
            if (_previousSequenceType != sequence.Type)
            {
                Clear();
                _previousSequenceType = sequence.Type;
            }
            if (sequence.Type == KeySequenceType.Sequence)
                PercentEffect(spectrum, sequence.Keys.ToArray(), value, total, percentEffectType, flashPast, flashReversed);
            else
                PercentEffect(spectrum, sequence.Freeform,       value, total, percentEffectType, flashPast, flashReversed);
        }

        /// <summary>
        /// Draws a percent effect on the layer bitmap using an array of DeviceKeys keys and solid colors.
        /// </summary>
        /// <param name="foregroundColor">The foreground color, used as a "Progress bar color"</param>
        /// <param name="value">The current progress value</param>
        /// <param name="total">The maxiumum progress value</param>
        private void PercentEffect(Color foregroundColor, Color backgroundColor, IReadOnlyList<DeviceKeys> keys, double value,
            double total, PercentEffectType percentEffectType = PercentEffectType.Progressive, double flashPast = 0.0,
            bool flashReversed = false, bool blinkBackground = false)
        {
            var progressTotal = value / total;
            if (progressTotal < 0.0)
                progressTotal = 0.0;
            else if (progressTotal > 1.0)
                progressTotal = 1.0;

            var progress = progressTotal * keys.Count;

            if (flashPast > 0.0 && ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast)))
            {
                var percent = Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI);
                if (blinkBackground)
                    backgroundColor = ColorUtils.BlendColors(backgroundColor, Color.Empty, percent);
                else
                    foregroundColor = ColorUtils.BlendColors(backgroundColor, foregroundColor, percent);
            }

            if (percentEffectType is PercentEffectType.Highest_Key or PercentEffectType.Highest_Key_Blend && keys.Count > 0)
            {
                var activeKey = (int)Math.Ceiling(value / (total / keys.Count)) - 1;
                var col = percentEffectType == PercentEffectType.Highest_Key ?
                    foregroundColor : ColorUtils.BlendColors(backgroundColor, foregroundColor, progressTotal);
                SetOneKey(keys[Math.Min(Math.Max(activeKey, 0), keys.Count - 1)], col);

            }
            else
            {
                for (var i = 0; i < keys.Count; i++)
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
        /// <param name="value">The current progress value</param>
        /// <param name="total">The maxiumum progress value</param>
        private void PercentEffect(ColorSpectrum spectrum, DeviceKeys[] keys, double value, double total,
            PercentEffectType percentEffectType = PercentEffectType.Progressive, double flashPast = 0.0,
            bool flashReversed = false)
        {
            var progressTotal = (value / total) switch
            {
                < 0.0 => 0.0,
                > 1.0 => 1.0,
                _ => value / total
            };
            if (progressTotal < _percentProgress)
            {
                Clear();
            }
            _percentProgress = progressTotal;

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
                        var color = spectrum.GetColorAt((float)i / (keys.Length - 1), 1.0f, flashAmount);
                        if (i == (int)progress)
                        {
                            var percent = progress - i;
                            SetOneKey(
                                currentKey,
                                ColorUtils.MultiplyColorByScalar(color, percent)
                            );
                        }
                        else if (i < (int)progress)
                            SetOneKey(currentKey, color);
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

            if (flashPast > 0.0 && ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast)))
            {
                var percent = Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI);
                switch (blinkBackground)
                {
                    case false:
                        foregroundColor = ColorUtils.BlendColors(backgroundColor, foregroundColor, percent);
                        break;
                    case true:
                        backgroundColor = ColorUtils.BlendColors(backgroundColor, Color.Empty, percent);
                        break;
                }
            }

            var xPos = (float)Math.Round((freeform.X + Effects.GridBaselineX) * Effects.EditorToCanvasWidth);
            var yPos = (float)Math.Round((freeform.Y + Effects.GridBaselineY) * Effects.EditorToCanvasHeight);
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

        public void Invalidate()
        {
            _textureBrush = null;
            _needsRender = true;
            _keyColors.Clear();
        }
        private void InvalidateColorMap(object? sender, EventArgs args)
        {
            _colormap?.Dispose();
            _colormap = new Bitmap(Effects.CanvasWidth, Effects.CanvasHeight);
            _ksChanged = true;
            Dimension.Height = Effects.CanvasHeight;
            Dimension.Width = Effects.CanvasWidth;
            Invalidate();
        }

        private double _percentProgress = -1;
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
            var progressTotal = MathUtils.Clamp(value / total, 0, 1);
            if (progressTotal < _percentProgress)
            {
                Clear();
            }
            _percentProgress = progressTotal;

            var flashAmount = 1.0;

            if (flashPast > 0.0)
            {
                if ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast))
                {
                    flashAmount = Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI);
                }
            }

            var xPos = (float)Math.Round((freeform.X + Effects.GridBaselineX) * Effects.EditorToCanvasWidth);
            var yPos = (float)Math.Round((freeform.Y + Effects.GridBaselineY) * Effects.EditorToCanvasHeight);
            var width = freeform.Width * Effects.EditorToCanvasWidth;
            var height = freeform.Height * Effects.EditorToCanvasHeight;

            if (width < 2) width = 2;
            if (height < 2) height = 2;

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

        private KeySequence _excludeSequence = new();

        /// <summary>
        /// Excludes provided sequence from the layer (Applies a mask)
        /// </summary>
        /// <param name="sequence">The mask to be applied</param>
        public void Exclude(KeySequence sequence)
        {
            if (_excludeSequence.Equals(sequence))
            {
                return;
            }

            var freeform = _excludeSequence.Freeform;
            WeakEventManager<FreeFormObject, EventArgs>.RemoveHandler(freeform, nameof(freeform.ValuesChanged), FreeformOnValuesChanged);
            _excludeSequence = sequence;
            WeakEventManager<FreeFormObject, EventArgs>.AddHandler(freeform, nameof(freeform.ValuesChanged), FreeformOnValuesChanged);
            FreeformOnValuesChanged(this, EventArgs.Empty);
            
            _needsRender = true;
        }

        private void PerformExclude(KeySequence sequence)
        {
            var gp = new GraphicsPath();
            switch (sequence.Type)
            {
                case KeySequenceType.Sequence:
                    foreach (var k in sequence.Keys)
                    {
                        var keyBounds = Effects.GetBitmappingFromDeviceKey(k);
                        gp.AddRectangle(keyBounds.Rectangle); //Overlapping additions remove that shape
                        _keyColors.Remove(k, out _);
                    }
                    break;
                case KeySequenceType.FreeForm:
                    var freeform = sequence.Freeform;
                    
                    var xPos = (float)Math.Round((freeform.X + Effects.GridBaselineX) * Effects.EditorToCanvasWidth);
                    var yPos = (float)Math.Round((freeform.Y + Effects.GridBaselineY) * Effects.EditorToCanvasHeight);
                    var width = (float)Math.Round(freeform.Width * Effects.EditorToCanvasWidth);
                    var height = (float)Math.Round(freeform.Height * Effects.EditorToCanvasHeight);

                    var rotatePoint = new PointF(xPos + width / 2.0f, yPos + height / 2.0f);
                    var myMatrix = new Matrix();
                    myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);

                    gp.AddRectangle(new RectangleF(xPos, yPos, width, height));
                    gp.Transform(myMatrix);
                    _keyColors.Clear();
                    break;
            }

            using var g = Graphics.FromImage(_colormap);
            g.SetClip(gp);
            g.Clear(Color.Transparent);
        }

        /// <summary>
        /// Inlcudes provided sequence from the layer (Applies a mask)
        /// </summary>
        public void OnlyInclude(KeySequence sequence)
        {
            var exclusionPath = GetExclusionPath(sequence);
            using var g = Graphics.FromImage(_colormap);
            g.SetClip(exclusionPath);
            g.Clear(Color.Transparent);
            Invalidate();
        }

        public override string ToString() => _name;
    }
}
