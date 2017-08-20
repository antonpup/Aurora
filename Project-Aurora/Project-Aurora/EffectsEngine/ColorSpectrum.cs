using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Aurora.EffectsEngine
{
	/// <summary>
	/// A class that represents a spectrum of colors. After creating an instance, you can then retrieve a blended color within a range of [0.0f, 1.0f].
	/// </summary>
	public class ColorSpectrum
	{
		/// <summary>
		/// A predefined rainbow ColorSpectrum
		/// </summary>
		public static ColorSpectrum Rainbow = new ColorSpectrum(
			Color.FromArgb(255, 0, 0),
			Color.FromArgb(255, 127, 0),
			Color.FromArgb(255, 255, 0),
			Color.FromArgb(0, 255, 0),
			Color.FromArgb(0, 0, 255),
			Color.FromArgb(75, 0, 130),
			Color.FromArgb(139, 0, 255)
			);

		/// <summary>
		/// A predefined seamless rainbow ColorSpectrum
		/// </summary>
		public static ColorSpectrum RainbowLoop = new ColorSpectrum(
			Color.FromArgb(255, 0, 0),
			Color.FromArgb(255, 127, 0),
			Color.FromArgb(255, 255, 0),
			Color.FromArgb(0, 255, 0),
			Color.FromArgb(0, 0, 255),
			Color.FromArgb(75, 0, 130),
			Color.FromArgb(139, 0, 255),
			Color.FromArgb(255, 0, 0)
			);

		private Dictionary<float, Color> colors;
		private float shift = 0.0f;

		/// <summary>
		/// Creates a default ColorSpectrum instance with red color on either end.
		/// </summary>
		public ColorSpectrum()
		{
			colors = new Dictionary<float, Color>();

			colors[0.0f] = Color.FromArgb(255, 0, 0);
			colors[1.0f] = Color.FromArgb(255, 0, 0);
		}

		/// <summary>
		/// Creates a ColorSpectrum instance with a specified color on either end.
		/// </summary>
		/// <param name="color"></param>
		public ColorSpectrum(Color color)
		{
			colors = new Dictionary<float, Color>();

			colors[0.0f] = color;
			colors[1.0f] = color;
		}

		/// <summary>
		/// Creates a ColorSpectrum instance with a start color on one end, and end color on the other end.
		/// </summary>
		/// <param name="startcolor">The starting color (at 0.0f)</param>
		/// <param name="endcolor">The ending color (at 1.0f)</param>
		public ColorSpectrum(Color startcolor, Color endcolor)
		{
			colors = new Dictionary<float, Color>();

			colors[0.0f] = startcolor;
			colors[1.0f] = endcolor;
		}

		/// <summary>
		/// Creates a ColorSpectrum instance with equidistant colors from the passed array of colors.
		/// </summary>
		/// <param name="colorslist">The passed array of colors</param>
		public ColorSpectrum(params Color[] colorslist)
		{
			colors = new Dictionary<float, Color>();

			for (int i = 0; i < colorslist.Length; i++)
			{
				float position = i / (float)(colorslist.Length - 1);

				colors[(float)Math.Round(position, 2)] = colorslist[i];
			}
		}

		/// <summary>
		/// Copy constructor, Creates a new ColorSpectrum instance with data from the passed ColorSpectrum.
		/// </summary>
		/// <param name="otherspectrum">The passed ColorSpectrum</param>
		public ColorSpectrum(ColorSpectrum otherspectrum)
		{
			colors = otherspectrum.colors;
		}

		/// <summary>
		/// Reverses the colors and their positions of the ColorSpectrum.
		/// </summary>
		/// <returns>Itself</returns>
		public ColorSpectrum Flip()
		{
			Dictionary<float, Color> newcolors = new Dictionary<float, Color>();

			foreach (KeyValuePair<float, Color> kvp in colors)
			{
				newcolors[1.0f - kvp.Key] = kvp.Value;
			}

			colors = newcolors;

			return this;
		}

		/// <summary>
		/// Shifts the internal position counter by a specified amount.
		/// </summary>
		/// <param name="shift_amount">The amount to shift the internal counter by</param>
		/// <returns>Itself</returns>
		public ColorSpectrum Shift(float shift_amount)
		{
			shift += shift_amount;
			shift = shift % 1.0f;

			return this;
		}

		/// <summary>
		/// Returns the corrected position on the color spectrum
		/// </summary>
		/// <param name="position">The position value to be corrected</param>
		/// <returns>The corrected position value of range [0.0f, 1.0f]</returns>
		private float CorrectPosition(float position)
		{
			float ret_pos = position;

			if (ret_pos > 1.0f)
				ret_pos -= (int)ret_pos;
			else if (ret_pos < 0.0f)
				ret_pos -= (int)(ret_pos - 1.0f);

			return ret_pos;
		}

		/// <summary>
		/// Adds a new color or sets an existing color at a specified position.
		/// </summary>
		/// <param name="position">The position value of range [0.0f, 1.0f]</param>
		/// <param name="color">The color to be set</param>
		/// <returns>Itself</returns>
		public ColorSpectrum SetColorAt(float position, Color color)
		{
			if (position <= 0.0f)
				position = 0.0f;

			if (position >= 1.0f)
				position = 1.0f;

			colors[position] = color;

			return this;
		}

		/// <summary>
		/// Retrieves the color from the ColorSpectrum at a specified position.
		/// </summary>
		/// <param name="position">The position value of range</param>
		/// <param name="max_position">The maxiumum position value, used to calculate a value in [0.0f , 1.0f] range</param>
		/// /// <param name="opacity">The opacity amount [0.0D - 1.0D]</param>
		/// <returns>The color</returns>
		public Color GetColorAt(float position, float max_position = 1.0f, double opacity = 1.0D)
		{
			position = CorrectPosition((position / max_position) + shift);

			float closest_lower = 0.0f;
			float closest_higher = 1.0f;

			foreach (KeyValuePair<float, Color> kvp in colors)
			{
				if (kvp.Key == position)
				{
					return kvp.Value;
				}

				if (kvp.Key > position && kvp.Key < closest_higher)
				{
					closest_higher = kvp.Key;
				}

				if (kvp.Key < position && kvp.Key > closest_lower)
				{
					closest_lower = kvp.Key;
				}
			}

			return Utils.ColorUtils.MultiplyColorByScalar(
				Utils.ColorUtils.BlendColors(
					colors[closest_lower], colors[closest_higher], ((double)(position - closest_lower) / (double)(closest_higher - closest_lower))
					),
				opacity
				);
		}

		/// <summary>
		/// Converts the instance of ColorSpectrum into a LinearGradientBrush
		/// </summary>
		/// <param name="width">The width of the LinearGradientBrush</param>
		/// <param name="height">The height of the LinearGradientBrush</param>
		/// <param name="x">The X coordinate of the LinearGradientBrush</param>
		/// <param name="y">The Y coordinate of the LinearGradientBrush</param>
		/// <param name="opacity">The opacity amount [0.0D - 1.0D]</param>
		/// <returns>The resulting LinearGradientBrush</returns>
		public LinearGradientBrush ToLinearGradient(float width, float height = 0.0f, float x = 0.0f, float y = 0.0f, double opacity = 1.0D)
		{
			var newColors = colors.Keys
				.Select(val => new KeyValuePair<float, Color>(val, Utils.ColorUtils.MultiplyColorByScalar(colors[val], opacity)))
				.OrderBy(val => val.Key)
				.ToList();

			if (newColors.First().Key != 0.0f)
			{
				newColors.Insert(0, new KeyValuePair<float, Color>(0.0f, newColors.First().Value));
			}

			if (newColors.Last().Key != 1.0f)
			{
				newColors.Add(new KeyValuePair<float, Color>(1.0f, newColors.Last().Value));
			}

			if (shift != 0.0f)
			{
				newColors = newColors.Select(val => new KeyValuePair<float, Color>(CorrectPosition(val.Key + shift), val.Value))
					.Distinct()
					.OrderBy(val => val.Key)
					.ToList();

				var endColor = Utils.ColorUtils.BlendColors(
					newColors.First().Value, newColors.Last().Value,
					((double) (newColors.First().Key) / (double) (newColors.First().Key + 1.0f - newColors.Last().Key)));

				if (newColors.First().Key != 0.0f)
				{
					newColors.Insert(0, new KeyValuePair<float, Color>(0.0f, endColor));
				}

				if (newColors.Last().Key != 1.0f)
				{
					newColors.Add(new KeyValuePair<float, Color>(1.0f, endColor));
				}
			}

			return new LinearGradientBrush(
				new PointF(x, y),
				new PointF(x + width, y + height),
				Color.Red, Color.Red)
			{
				InterpolationColors = new ColorBlend
				{
					Colors = newColors.Select(val => val.Value).ToArray(),
					Positions = newColors.Select(val => val.Key).ToArray()
				}
			};
		}

		/// <summary>
		/// Retrieves the colors and their positions on the spectrum
		/// </summary>
		/// <returns>Dictionary with position within the spectrum and color</returns>
		public Dictionary<float, Color> GetSpectrumColors()
		{
			return new Dictionary<float, Color>(colors);
		}

		/// <summary>
		/// Multiplies all colors in the spectrum by a scalar
		/// </summary>
		/// <returns></returns>
		public ColorSpectrum MultiplyByScalar(double scalar)
		{
			Dictionary<float, Color> newcolors = new Dictionary<float, Color>();

			foreach (KeyValuePair<float, Color> kvp in colors)
				newcolors[kvp.Key] = Utils.ColorUtils.MultiplyColorByScalar(kvp.Value, scalar);

			colors = newcolors;

			return this;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((ColorSpectrum)obj);
		}

		public bool Equals(ColorSpectrum p)
		{
			if (ReferenceEquals(null, p)) return false;
			if (ReferenceEquals(this, p)) return true;

			return (shift == p.shift &&
				colors.Equals(p.colors));
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + shift.GetHashCode();
				hash = hash * 23 + colors.GetHashCode();
				return hash;
			}
		}
	}
}
