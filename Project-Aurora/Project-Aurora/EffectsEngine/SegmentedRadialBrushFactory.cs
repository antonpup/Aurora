using Aurora.Utils;
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Aurora.EffectsEngine {

    /// <summary>
    /// A factory that can create a segmented radial brush.
    /// </summary>
    /// <remarks>
    /// I originally tried creating this effect using the <see cref="PathGradientBrush"/>, however I cannot find a way of removing the central colour. This means that the
    /// colours gradually fade to another colour in the centre. Since the points on the path would need to be equidistant from the centre to preserve the angle and gradients,
    /// it means that some of the brush is cut off and the colours appear washed out. All round, not ideal for this use case, so that is the reason I have created this instead.
    /// </remarks>
    public class SegmentedRadialBrushFactory {

        // The resolution of the base texture size.
        private const int textureSize = 50;
        private static readonly Rectangle renderArea = new Rectangle(0, 0, textureSize, textureSize);
        private static SolidBrush fallback = new SolidBrush(Color.Transparent);

        private Color[] colors;
        private int easingAmount;
        private TextureBrush baseBrush;

        public SegmentedRadialBrushFactory(Color[] colors) {
            this.colors = colors;
            CreateBaseTextureBrush();
        }

        /// <summary>
        /// Gets or sets the colors and their orders in use by the brush.
        /// </summary>
        public Color[] Colors {
            get => (Color[])colors.Clone();
            set {
                // If the colors are equal, don't do anything
                if (colors.Length == value.Length && ((IStructuralEquatable)colors).Equals(value, StructuralComparisons.StructuralEqualityComparer))
                    return;

                // If they are not equal, create a new texture brush
                colors = value;
                CreateBaseTextureBrush();
            }
        }

        /// <summary>
        /// Determines the number of auto-generated colors between each of the colors defined in the <see cref="Colors"/> array.
        /// </summary>
        public int EasingAmount {
            get => easingAmount;
            set {
                if (easingAmount != value) {
                    easingAmount = value;
                    CreateBaseTextureBrush();
                }
            }
        }

        /// <summary>
        /// Creates a new base brush from the current properties.
        /// </summary>
        private void CreateBaseTextureBrush() {
            var colors = GetBrushColors();
            var angle = (float)(360 / colors.Length);

            // Draw the texture to be used for the brush. This is made up of circular segments 
            var texture = new Bitmap(textureSize, textureSize);
            using (var gfx = Graphics.FromImage(texture))
                for (var i = 0; i < colors.Length; i++)
                    gfx.FillPie(new SolidBrush(colors[i]), renderArea, i * angle, angle);

            // Create the texture brush from our custom bitmap texture
            baseBrush = new TextureBrush(texture);
        }

        /// <summary>
        /// Generates the colors array to be used for building the brush. This will also generate interpolated colors between defined stops if required.
        /// </summary>
        private Color[] GetBrushColors() {
            // Simply return the colors array if easing is disabled.
            if (easingAmount <= 0) return colors;

            // For each color, easingAmount many times, generate the interpolated color between the 'i'th color and the 'i + 1'th color.
            var interpolatedColors = new Color[colors.Length * (easingAmount + 1)];
            var easeAmountScale = 1f / (easingAmount + 1);
            for (var i = 0; i < colors.Length; i++) {
                var s = i * (easingAmount + 1); // The start index for this color group
                interpolatedColors[s] = colors[i];
                for (var j = 0; j < easingAmount; j++)
                    interpolatedColors[s + j + 1] = ColorUtils.BlendColors(colors[i], colors[(i + 1) % colors.Length], (j + 1) * easeAmountScale);
            }

            return interpolatedColors;
        }

        /// <summary>
        /// Gets the brush that will be centered on and sized for the specified region.
        /// </summary>
        /// <param name="region">The region which defines where the brush will be drawn and where the brush will be centered.</param>
        /// <param name="angle">The angle which the brush will be rendered at.</param>
        /// <param name="keepAspectRatio">If <c>true</c>, the scale transformation will have the same value in x as it does in y. If <c>false</c>, the scale in each dimension may be different.
        /// When <c>true</c>, the sizes/areas of each color may appear different (due to being cut off), however when <c>false</c>, they appear more consistent.
        /// If the brush is animated, <c>true</c> will make the speeed appear constant whereas <c>false</c> will cause the rotation to appear slower on the shorter side.</param>
        public Brush GetBrush(RectangleF region, float angle = 0, bool keepAspectRatio = true) {
            // Check if the region has a 0 size. If so, just return a blank brush instead (the matrix becomes invalid with 0 size scaling).
            if (region.Width == 0 || region.Height == 0) return fallback;

            var brush = (TextureBrush)baseBrush.Clone(); // Clone the brush so we don't alter the transformation of it in other places accidently
            var mtx = new Matrix();

            // Translate it so that the center of the texture (where all the colors meet) is at 0,0
            mtx.Translate(-textureSize / 2, -textureSize / 2, MatrixOrder.Append);

            // Then, rotate it to the target angle
            mtx.Rotate(angle, MatrixOrder.Append);

            // Scale it so that it'll still completely cover the textureSize area.
            // 1.45 is a rough approximation of SQRT(2) [it's actually 1.414 but we want to allow a bit of space incase of artifacts at the edges]
            mtx.Scale(1.45f, 1.45f, MatrixOrder.Append);

            // Next we need to scale the texture so that it'll cover the area defined by the region
            float sx = region.Width / textureSize, sy = region.Height / textureSize;
            // If the aspect ratio is locked, we want to scale both dimensions up to the biggest required scale
            if (keepAspectRatio)
                sx = sy = Math.Max(sx, sy);
            mtx.Scale(sx, sy, MatrixOrder.Append);

            // Finally, we need to translate the texture so that it is in the center of the region
            // (At this point, the center of the texture where the colors meet is still at 0,0)
            mtx.Translate(region.Left + (region.Width / 2), region.Top + (region.Height / 2), MatrixOrder.Append);

            // Apply the transformation and return the texture brush
            brush.Transform = mtx;
            return brush;
        }
    }
}
