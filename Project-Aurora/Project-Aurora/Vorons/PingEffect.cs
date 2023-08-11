//
// Voron Scripts - PingEffect
// v1.0-beta.8
// https://github.com/VoronFX/Aurora
// Copyright (C) 2016 Voronin Igor <Voron.exe@gmail.com>
// 

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Modules.ProcessMonitor;
using Aurora.Profiles;
using Aurora.Settings;
using Aurora.Utils;

namespace Aurora.Scripts.VoronScripts
{
	public class PingEffect : IEffectScript
	{
		public string ID
		{
			get { return "Voron Scripts - PingEffect - v1.0-beta.6"; }
		}

		public VariableRegistry Properties { get; private set; }

		internal enum EffectTypes
		{
			[Description("Ping pulse")]
			PingPulse = 0,

			[Description("Ping graph")]
			PingGraph = 1,
		}

		public PingEffect()
		{
			Properties = new VariableRegistry();

			Properties.RegProp("Keys or Freestyle",
				new KeySequence(new[] {
					DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4, DeviceKeys.F5,  DeviceKeys.F6,
					DeviceKeys.F7,  DeviceKeys.F8,  DeviceKeys.F9,  DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12
				}));

			Properties.RegProp("Graph Mode", false, String.Join(Environment.NewLine, "Display graph of last ping responses instead of bar representing last one response."
				, "In graph mode animation begins only after ping response (another words a bit later then in normal mode)."));

			//Properties.RegProp("Effect type", (long)EffectTypes.PingPulse,
			//	String.Join(Environment.NewLine,
			//	Enumerable.Cast<EffectTypes>(Enum.GetValues(typeof(EffectTypes))).Select(x => string.Format("{0} - {1}", (int)x, x))),
			//	(long)Enumerable.Cast<EffectTypes>(Enum.GetValues(typeof(EffectTypes))).Min(),
			//	(long)Enumerable.Cast<EffectTypes>(Enum.GetValues(typeof(EffectTypes))).Max());

			Properties.RegProp("Default Host", "google.com", "Ping this host when no known apps in foreground.");
			Properties.RegProp("Per App Hosts", "LolClient.exe @ 185.40.64.69 | LeagueClientUx.exe @ 185.40.64.69 | Blade & Soul @ 109.105.133.67",
				String.Join(Environment.NewLine, "Ping special host (i.e. game server) when certain app is in foreground.",
				"Enter process name or window title, then \"@\" followed by host.",
				"Elevated processes can't be detected only by window title.",
				"Separate apps with \"|\"."));

			Properties.RegProp("Max Ping", 400L, "Such pings or higher will fill full bar.", 50L, 3000L);
			//Properties.RegProp("Animation Reserve Delay", 100L, String.Join(Environment.NewLine,
			//	"Reserve delay between actual ping response and animation in (ms).",
			//	"The lower the value the more recent information you see, the higher the value the less chance of animation glitches."), 0L, 1000L);

			Properties.RegProp("Ping Signal Color", new RealColor(Color.Blue), "Color of ping signal");
			Properties.RegProp("Ping Signal Width", 17L, "Width of ping signal", 0L, 100L);
			Properties.RegProp("Ping Shadow Width", 17L, "Width of shadow preceding ping signal", 0L, 100L);

			Properties.RegProp("Fail Animation Duration", 2000L, "Duration of failed ping animation in (ms)", 200L, 5000L);
			Properties.RegProp("Success Animation Duration", 500L, "Duration of succeeded ping animation in (ms)", 200L, 5000L);
			Properties.RegProp("Delay Between Requests", 1000L, "Minimum delay to make a new request from start of the last request.", 200L, 10000L);
			Properties.RegProp("Delay After Animation", 500L, "Minimum delay to make a new request from last animation end. (ms)", 0L, 10000L);

			Properties.RegProp("Bar Gradient", "#FF00FF00 | #FFFFA500 | #FFFF0000",
				String.Join(Environment.NewLine,
				"Gradient that is used for effect. Separate color points with \"|\".",
				"Optionally set point position with \"@\" symbol."));

			Properties.RegProp("Fail Animation Gradient", "#00FF0000 | #FFFF0000 | #FF000000 | #FFFF0000 | #FF000000",
				String.Join(Environment.NewLine,
					"Gradient that is used for animation failed ping. Separate color points with \"|\".",
					"Optionally set point position with \"@\" symbol."));

			Properties.RegProp("Number of Pings in graph mode", 10L, "Amount of last pings that will be used to display graph.", 1, 50);

			//Properties.RegProp("BrightMode", false);
			_layer = new EffectLayer(ID);
		}

		private static readonly ConcurrentDictionary<Tuple<KeySequence, string, string>, KeyValuePair<AnimationData, Pinger>> PingAnimations = new();

		private static readonly ConcurrentDictionary<string, ColorSpectrum> Gradients = new();
		private readonly EffectLayer _layer;

		private KeySequence Keys { get; set; }
		private EffectTypes EffectType { get; set; }

		private string DefaultHost { get; set; }
		private string PerAppHosts { get; set; }

		private long MaxPing { get; set; }
		private long AnimationReserveDelay { get; set; }

		private float PingSignalWidth { get; set; }
		private float PingShadowWidth { get; set; }

		private long FailAnimationDuration { get; set; }
		private long SuccessAnimationDuration { get; set; }
		private long DelayBetweenRequests { get; set; }
		private long DelayAfterAnimation { get; set; }
		private ColorSpectrum BarGradient { get; set; }
		private ColorSpectrum FailAnimationGradient { get; set; }

		private ColorSpectrum ShadowGradient { get; set; }
		private ColorSpectrum PingGradient { get; set; }
		private Color PingSignalColor { get; set; }

		private AnimationData Data { get; set; }
		private Pinger Pinger { get; set; }

		private bool BrightMode { get; set; }
		private long PingsInGraphMode { get; set; }

		private long CurrentTime { get; set; }

		private void ReadProperties(VariableRegistry properties)
		{
			Keys = properties.GetVariable<KeySequence>("Keys or Freestyle");
			EffectType = properties.GetVariable<bool>("Graph Mode") ? EffectTypes.PingGraph : EffectTypes.PingPulse;
			//(EffectTypes)properties.GetVariable<long>("Effect type");

			PingSignalWidth = properties.GetVariable<long>("Ping Signal Width") / 100f;
			PingShadowWidth = properties.GetVariable<long>("Ping Shadow Width") / 100f;

			FailAnimationDuration = properties.GetVariable<long>("Fail Animation Duration");
			SuccessAnimationDuration = properties.GetVariable<long>("Success Animation Duration");
			DelayBetweenRequests = properties.GetVariable<long>("Delay Between Requests");
			DelayAfterAnimation = properties.GetVariable<long>("Delay After Animation");

			BarGradient = Gradients.GetOrAdd(properties.GetVariable<string>("Bar Gradient"), ScriptHelper.StringToSpectrum);
			FailAnimationGradient = Gradients.GetOrAdd(properties.GetVariable<string>("Fail Animation Gradient"), ScriptHelper.StringToSpectrum);

			DefaultHost = properties.GetVariable<string>("Default Host");
			PerAppHosts = properties.GetVariable<string>("Per App Hosts");

			MaxPing = properties.GetVariable<long>("Max Ping");
			AnimationReserveDelay = 50;// properties.GetVariable<long>("Animation Reserve Delay");

			var data = PingAnimations.GetOrAdd(new Tuple<KeySequence, string, string>(Keys, DefaultHost, PerAppHosts),
				key => new KeyValuePair<AnimationData, Pinger>(new AnimationData(), new Pinger(DefaultHost, PerAppHosts.Split('|')
					.Select(x => x.Trim().Split('@').Select(x2 => x2.Trim().ToLower()))
					.ToDictionary(s => s.First(), s => s.Last()))));

			Pinger = data.Value;
			Data = data.Key;

			ShadowGradient = new ColorSpectrum(Color.Black, Color.FromArgb(0, Color.Black));
			PingSignalColor = properties.GetVariable<RealColor>("Ping Signal Color").GetDrawingColor();
			PingGradient = new ColorSpectrum(Color.FromArgb(0, PingSignalColor), PingSignalColor);

			BrightMode = false;// properties.GetVariable<bool>("BrightMode");
			PingsInGraphMode = properties.GetVariable<long>("Number of Pings in graph mode");

			CurrentTime = Time.GetMillisecondsSinceEpoch();
		}

		public object UpdateLights(VariableRegistry properties, IGameState state = null)
		{
			ReadProperties(properties);

			_layer.Clear();
			Render(_layer);
			return _layer;
		}

		public void Render(EffectLayer effectLayer)
		{
			// layers							|					|
			// 0.OldReplyPingBar				|==========>		|
			// 1.PingShadowAnimation			|     >=-			|
			// 2.NewBar							|====>				|
			// 3.SignalAnimation				|  -=>				|

			ProcessAnimationPhase();

			var pingPos = -PingShadowWidth;
			var newBarEnd = 0f;
			var oldBarEnd = 0f;
			var graphShift = Data.Phase == AnimationPhase.CompletingFailAnimation
							 && CurrentTime - Data.FailAnimationStartTime > FailAnimationDuration / 2
				? 0
				: -1;

			if (CurrentTime >= Data.SuccessAnimationStartTime && CurrentTime <= Data.FinalAnimationEndTime)
			{
				var phase = Math.Min(1, ((CurrentTime - Data.SuccessAnimationStartTime) / (float)SuccessAnimationDuration));

				if (EffectType != EffectTypes.PingGraph || Data.Phase != AnimationPhase.CompletingFailAnimation)
					pingPos += phase * (1f + PingSignalWidth + PingShadowWidth);

				newBarEnd = pingPos;
				if (Pinger.Reply != null && Pinger.Reply.Status == IPStatus.Success)
				{
					newBarEnd = Math.Min(1f, Pinger.Reply.RoundtripTime / (float)MaxPing);
					newBarEnd = Math.Max(0, Math.Min(newBarEnd, pingPos));
				}
			}

			if (EffectType == EffectTypes.PingGraph)
			{
				oldBarEnd = 1f;
				newBarEnd = pingPos;
			}
			else if (Data.OldReply != null && Data.OldReply.Status == IPStatus.Success)
			{
				oldBarEnd = Math.Min(1f, Data.OldReply.RoundtripTime / (float)MaxPing);
			}

			var oldBarStart = Math.Max(0, pingPos);

			if (Keys.Type == KeySequenceType.FreeForm)
			{
				var gradLayers = new GradientCascade();

				if (EffectType == EffectTypes.PingGraph)
				{
					gradLayers.Add(GraphGradient(Data.Replies.ToArray(), (int)PingsInGraphMode, graphShift),
						oldBarStart, oldBarEnd, oldBarStart, oldBarEnd);

					if (Data.Phase != AnimationPhase.CompletingFailAnimation)
						gradLayers.Add(GraphGradient(Data.Replies.ToArray(), (int)PingsInGraphMode, 0),
							0, newBarEnd, 0, newBarEnd);
				}
				else
				{
					if (oldBarEnd - oldBarStart > 0)
					{
						gradLayers.Add(BarGradient, oldBarStart, oldBarEnd, oldBarStart, oldBarEnd);
					}
					if (newBarEnd > 0f)
					{
						gradLayers.Add(BarGradient, 0, newBarEnd, 0, newBarEnd);
					}
				}

				if (CurrentTime >= Data.SuccessAnimationStartTime && CurrentTime <= Data.FinalAnimationEndTime)
				{
					//pingPos = 0.5f;

					gradLayers.Add(PingGradient, 0, 1, pingPos - PingSignalWidth, pingPos);
					gradLayers.Add(ShadowGradient, 0, 1, pingPos, pingPos + PingShadowWidth);
				}
				if (Data.Phase == AnimationPhase.CompletingFailAnimation
					&& CurrentTime >= Data.FailAnimationStartTime && CurrentTime <= Data.FinalAnimationEndTime)
				{
					var phase = Math.Min(1, ((CurrentTime - Data.FailAnimationStartTime) / (float)FailAnimationDuration));
					gradLayers.Add(new ColorSpectrum(FailAnimationGradient.GetColorAt(phase)), 0, 1, 0, 1);
				}

				gradLayers.Draw(Keys.Freeform, effectLayer);
			}
			else
			{
				for (int i = 0; i < Keys.Keys.Count; i++)
				{
					var keyColor = effectLayer.Get(Keys.Keys[i]);

					float kL = i / (Keys.Keys.Count - 1f);

					if (EffectType == EffectTypes.PingGraph)
					{
						keyColor = ColorUtils.BlendColors(keyColor,
							GraphGradient(Data.Replies.ToArray(), (int)PingsInGraphMode, graphShift).GetColorAt(kL),
							GetKeyBlend(pingPos, i, oldBarEnd));

						if (Data.Phase != AnimationPhase.CompletingFailAnimation)
							keyColor = ColorUtils.BlendColors(keyColor,
								GraphGradient(Data.Replies.ToArray(), (int)PingsInGraphMode, 0).GetColorAt(kL), GetKeyBlend(0, i, newBarEnd));
					}
					else
					{
						keyColor = ColorUtils.BlendColors(keyColor, BarGradient.GetColorAt(kL), GetKeyBlend(pingPos, i, oldBarEnd));
						keyColor = ColorUtils.BlendColors(keyColor, BarGradient.GetColorAt(kL), GetKeyBlend(0, i, newBarEnd));
					}

					if (PingShadowWidth > 0f)
						keyColor = ColorUtils.BlendColors(keyColor, Color.Black,
							GetKeyBlend(pingPos, i, pingPos + PingShadowWidth)
							* (1 - ((i / (float)Keys.Keys.Count - pingPos) / PingShadowWidth)));

					if (PingSignalWidth > 0f)
						keyColor = ColorUtils.BlendColors(keyColor, PingSignalColor,
							GetKeyBlend(pingPos - PingSignalWidth, i, pingPos)
							* (((i + 1) / (float)Keys.Keys.Count - (pingPos - PingSignalWidth)) / PingSignalWidth));

					if (Data.Phase == AnimationPhase.CompletingFailAnimation)
						keyColor = ColorUtils.AddColors(keyColor, FailAnimationGradient.GetColorAt(
							Math.Min(1, (CurrentTime - Data.FinalAnimationEndTime) / (float)FailAnimationDuration)));

					effectLayer.Set(Keys.Keys[i], keyColor);
				}
			}
		}

		private void ProcessAnimationPhase()
		{
			switch (Data.Phase)
			{
				case AnimationPhase.WaitingForNextRequest:
					if (CurrentTime > Data.NextPingStartTime && Pinger.SendRequest())
					{
						Data.Phase = EffectType == EffectTypes.PingPulse ?
							AnimationPhase.WaitingForPingStart : AnimationPhase.WaitingForPingEnd;
					}
					break;
				case AnimationPhase.WaitingForPingStart:
					if (Pinger.State == PingerState.RequestSent
						|| Pinger.State == PingerState.ResponseRecieved)
					{
						var minimalRequiredReserveTime = Math.Max(0,
							(long)(MaxPing - (SuccessAnimationDuration * (1f + PingShadowWidth) /
											   (1f + PingSignalWidth + PingShadowWidth))));
						Data.SuccessAnimationStartTime = Pinger.PingStartedTime + AnimationReserveDelay + minimalRequiredReserveTime;
						Data.FinalAnimationEndTime = Data.SuccessAnimationStartTime + SuccessAnimationDuration;
						Data.Phase = AnimationPhase.WaitingForPingEnd;
					}
					break;
				case AnimationPhase.WaitingForPingEnd:
					if (Pinger.State == PingerState.ResponseRecieved)
					{
						if (EffectType == EffectTypes.PingGraph)
						{
							if (Pinger.Reply != null && Pinger.Reply.Status == IPStatus.Success)
							{
								if (Data.Replies.Count > 0)
									Data.Replies.RemoveLast();
								Data.Replies.AddLast((int?)Pinger.Reply.RoundtripTime);
							}
							Data.SuccessAnimationStartTime = CurrentTime;
						}
						if (CurrentTime < Data.SuccessAnimationStartTime)
						{
							Data.SuccessAnimationStartTime = CurrentTime;
						}
						Data.FinalAnimationEndTime = Data.SuccessAnimationStartTime + SuccessAnimationDuration;
						Data.Phase = AnimationPhase.CompletingSuccessAnimation;
						if (Pinger.Reply == null || Pinger.Reply.Status != IPStatus.Success)
						{
							Data.FailAnimationStartTime = Math.Max(CurrentTime, Data.SuccessAnimationStartTime);
							Data.FinalAnimationEndTime = Data.FailAnimationStartTime + FailAnimationDuration;
							Data.Phase = AnimationPhase.CompletingFailAnimation;
						}
						Data.NextPingStartTime = Math.Max(Data.FinalAnimationEndTime + DelayAfterAnimation,
							Pinger.PingStartedTime + DelayBetweenRequests);
					}
					break;
				case AnimationPhase.CompletingSuccessAnimation:
				case AnimationPhase.CompletingFailAnimation:
					if (CurrentTime > Data.FinalAnimationEndTime)
					{
						if (EffectType == EffectTypes.PingGraph)
						{
							Data.Replies.AddLast((int?)null);
							while (Data.Replies.Count > PingsInGraphMode + 1)
								Data.Replies.RemoveFirst();
						}
						Data.OldReply = Pinger.Reply;
						Pinger.Reset();
						Data.Phase = AnimationPhase.WaitingForNextRequest;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public float GetKeyBlend(float leftEdge, float pos, float rightEdge)
		{
			var leftEdgePercent = (1 + pos) / Keys.Keys.Count - leftEdge;
			var rightEdgePercent = rightEdge - (pos / Keys.Keys.Count);
			leftEdgePercent /= 1f / Keys.Keys.Count;
			rightEdgePercent /= 1f / Keys.Keys.Count;
			leftEdgePercent = 1 - Math.Max(0, Math.Min(1, leftEdgePercent));
			rightEdgePercent = 1 - Math.Max(0, Math.Min(1, rightEdgePercent));
			return 1 - leftEdgePercent - rightEdgePercent;
		}

		public ColorSpectrum GraphGradient(int?[] replies, int count, int shift)
		{
			var blackTransparent = Color.FromArgb(0, Color.Black);
			var gradient = new ColorSpectrum();
			for (int i = 0; i < count; i++)
			{
				int? reply = null;
				int ri = i - (count - replies.Length) + shift;

				if (ri >= 0 && ri < replies.Length)
				{
					reply = replies[ri];
				}

				gradient.SetColorAt((float)i / (count - 1),
					(reply.HasValue)
						? BarGradient.GetColorAt(Math.Min(1, (float)reply.Value / MaxPing))
						: blackTransparent);
			}

			return gradient;
		}
	}

	internal static class ColorSpectrumExtensionMethods
	{
		public static Color GetRegionAverageColor(this ColorSpectrum spectrum, float start, float end)
		{
			var width = Math.Abs(end - start);
			start = Math.Min(1, Math.Max(0, start));
			end = Math.Min(1, Math.Max(0, end));
			var colors = spectrum.GetSpectrumColors()
				.Where(x => x.Key > start && x.Key < end)
				.Concat(new[]
					{new KeyValuePair<double, Color>(start, spectrum.GetColorAt(start)), new KeyValuePair<double, Color>(end, spectrum.GetColorAt(end))})
				.OrderBy(x => x.Key).ToArray();
			double red = 0;
			double green = 0;
			double blue = 0;
			double alpha = 0;
			for (var i = 1; i < colors.Length; i++)
			{
				var subcolor = spectrum.GetColorAt(((colors[i].Key - colors[i - 1].Key) / 2f) + colors[i - 1].Key);
				var ratio = Math.Abs(colors[i].Key - colors[i - 1].Key) / width;
				red += subcolor.R * ratio;
				green += subcolor.G * ratio;
				blue += subcolor.B * ratio;
				alpha += subcolor.A * ratio;
			}
			return Color.FromArgb((int)alpha, (int)red, (int)green, (int)blue);
		}

		public static ColorSpectrum Overlay(this ColorSpectrum spectrum, ColorSpectrum overlayGradient, float start, float end)
		{
			var overlayed = new ColorSpectrum();
			var sizeRatio = Math.Abs(end - start);
			foreach (var color in spectrum.GetSpectrumColors())
			{
				var pos = (color.Key - start) * sizeRatio;
				overlayed.SetColorAt(color.Key, ColorUtils.AddColors(color.Value, overlayGradient.GetColorAt(pos)));
			}
			foreach (var color in overlayGradient.GetSpectrumColors())
			{
				var pos = (color.Key / sizeRatio) + start;
				if (pos >= 0f && pos <= 1f)
					overlayed.SetColorAt(pos, ColorUtils.AddColors(spectrum.GetColorAt(pos), color.Value));
			}
			return overlayed;
		}

		public static ColorSpectrum Cut(this ColorSpectrum spectrum, float start, float end)
		{
			var cutted = new ColorSpectrum();
			cutted.SetColorAt(0.0f, spectrum.GetColorAt(start));
			cutted.SetColorAt(1.0f, spectrum.GetColorAt(end));
			foreach (var color in spectrum.GetSpectrumColors()
				.Where(x => x.Key > start && x.Key < end))
			{
				cutted.SetColorAt((color.Key - start) / Math.Abs(end - start), color.Value);
			}
			Debug.WriteLine(start.ToString() + " " + end.ToString() + "|" + string.Join(" ", cutted.GetSpectrumColors().Select(x => x.Key.ToString() + " " + x.Value.ToString())));
			return cutted;
		}
	}

	internal class GradientCascade
	{
		private readonly List<Tuple<ColorSpectrum, PointF, PointF>> gradients = new();

		public void Add(ColorSpectrum spectrum, float gradientStart, float gradientEnd, float targetStart, float targetEnd)
		{
			if (((targetStart >= 0 && targetStart <= 1)
				|| (targetEnd >= 0 && targetEnd <= 1))
				&& targetStart < targetEnd)
			{
				gradients.Add(new Tuple<ColorSpectrum, PointF, PointF>(spectrum,
					new PointF(gradientStart, gradientEnd), new PointF(targetStart, targetEnd)));
			}
		}

		public void Draw(List<DeviceKeys> keys, EffectLayer effectLayer)
		{
			var keyWidth = 1f / keys.Count;
			for (var i = 0; i < keys.Count; i++)
			{
				var keyStart = i * keyWidth;
				var keyEnd = (i + 1) * keyWidth;
				var keyColor = effectLayer.Get(keys[i]);
				foreach (var gradient in gradients)
				{
					var left = Math.Max(0, Math.Max(keyStart, gradient.Item3.X));
					var right = Math.Min(1, Math.Min(keyEnd, gradient.Item3.Y));
					var gradWidthRatio = gradient.Item3.Y - gradient.Item3.X;
					var gr_left = (keyStart - gradient.Item3.X) / gradWidthRatio;
					var gr_right = (keyEnd - gradient.Item3.X) / gradWidthRatio;
					if (gr_left >= 0 && gr_left <= 1
						|| gr_right >= 0 && gr_right <= 1)
					{
						if (right - left > 0)
						{
							var alfa = (right - left) / keyWidth;
							var gr_cut_ratio = gradient.Item2.Y - gradient.Item2.X;
							var gr_cut_left = (gr_left * gr_cut_ratio) + gradient.Item2.X;
							var gr_cut_right = (gr_right * gr_cut_ratio) + gradient.Item2.X;
							var gradColor = gradient.Item1.GetRegionAverageColor(gr_cut_left, gr_cut_right);
							keyColor = ColorUtils.BlendColors(keyColor, gradColor, gradColor.A / 255f * alfa);
						}
					}
				}
				effectLayer.Set(keys[i], keyColor);
			}
		}

		public void DrawBright(List<DeviceKeys> keys, EffectLayer effectLayer)
		{
			Func<float, float, float, float> getBlend2 = (l, pos, r) =>
			{
				var leftEdgePercent = (1 + pos) / keys.Count - l;
				var rightEdgePercent = r - (pos / keys.Count);
				leftEdgePercent /= 1f / keys.Count;
				rightEdgePercent /= 1f / keys.Count;
				leftEdgePercent = 1 - Math.Max(0, Math.Min(1, leftEdgePercent));
				rightEdgePercent = 1 - Math.Max(0, Math.Min(1, rightEdgePercent));
				return 1 - leftEdgePercent - rightEdgePercent;
			};

			var keyWidth = 1f / keys.Count;
			for (var i = 0; i < keys.Count; i++)
			{
				var keyStart = i * keyWidth;
				var keyEnd = (i + 1) * keyWidth;
				var keyColor = effectLayer.Get(keys[i]);
				foreach (var gradient in gradients)
				{
					var left = Math.Max(0, Math.Max(keyStart, gradient.Item3.X));
					var right = Math.Min(1, Math.Min(keyEnd, gradient.Item3.Y));
					var gradWidthRatio = gradient.Item3.Y - gradient.Item3.X;
					var gr_left = (keyStart - gradient.Item3.X) / gradWidthRatio;
					var gr_right = (keyEnd - gradient.Item3.X) / gradWidthRatio;
					if (gr_left >= 0 && gr_left <= 1
						|| gr_right >= 0 && gr_right <= 1)
					{
						if (right - left > 0)
						{
							var alfa = (right - left) / keyWidth;
							var gr_cut_ratio = gradient.Item2.Y - gradient.Item2.X;
							var gr_cut_left = (gr_left * gr_cut_ratio) + gradient.Item2.X;
							var gr_cut_right = (gr_right * gr_cut_ratio) + gradient.Item2.X;
							var middle = Math.Min(1, Math.Max(0, gr_cut_left + (gr_cut_right - gr_cut_left) / 2f));
							var gradColor = gradient.Item1.GetColorAt(middle);
							keyColor = ColorUtils.BlendColors(keyColor, gradColor, gradColor.A / 255f * getBlend2(gradient.Item3.X, i, gradient.Item3.Y));
						}
					}
				}
				effectLayer.Set(keys[i], keyColor);
			}
		}

		public void Draw(FreeFormObject freeform, EffectLayer effectLayer)
		{
			using (Graphics g = effectLayer.GetGraphics())
			{
				float x_pos = (float)Math.Round((freeform.X + Effects.GridBaselineX) * Effects.EditorToCanvasWidth);
				float y_pos = (float)Math.Round((freeform.Y + Effects.GridBaselineY) * Effects.EditorToCanvasHeight);
				float width = (float)(freeform.Width * Effects.EditorToCanvasWidth);
				float height = (float)(freeform.Height * Effects.EditorToCanvasHeight);

				if (width < 3) width = 3;
				if (height < 3) height = 3;

				var rotatePoint = new PointF(x_pos + (width / 2.0f), y_pos + (height / 2.0f));
				var myMatrix = new Matrix();
				myMatrix.RotateAt(freeform.Angle, rotatePoint, MatrixOrder.Append);
				g.Transform = myMatrix;

				foreach (var gradient in gradients)
				{
					var x_pos_gr = x_pos + width * gradient.Item3.X;
					var width_gr = width * (gradient.Item3.Y - gradient.Item3.X);

					var rect = new RectangleF(x_pos_gr, y_pos, width_gr, height);
					rect.Intersect(new RectangleF(x_pos, y_pos, width, height));
					if (!rect.IsEmpty)
					{
						LinearGradientBrush brush = gradient.Item1.ToLinearGradient(
							width_gr / (gradient.Item2.Y - gradient.Item2.X), 0,
							x_pos_gr - (width_gr * gradient.Item2.X / (gradient.Item2.Y - gradient.Item2.X)), 0);

						brush.WrapMode = WrapMode.TileFlipX;
						g.FillRectangle(brush, rect);
					}
				}
			}
		}
	}

	internal enum AnimationPhase
	{
		WaitingForNextRequest, WaitingForPingStart, WaitingForPingEnd, CompletingSuccessAnimation, CompletingFailAnimation
	}
	internal class AnimationData
	{

		public long SuccessAnimationStartTime;
		public long FailAnimationStartTime;
		public long NextPingStartTime;
		public long FinalAnimationEndTime;
		public AnimationPhase Phase;
		public PingReply OldReply;
		public readonly LinkedList<int?> Replies = new();
	}
	internal enum PingerState
	{
		Initial, RequestSent, ResponseRecieved,
	}

	internal class Pinger
	{
		private long pingStartedTime;
		private long pingEndedTime;
		private PingReply reply;
		private volatile PingerState state;


		private Dictionary<string, string> HostsPerApplication { get; set; }
		private string DefaultHost { get; set; }

		public long PingStartedTime
		{
			get { return Volatile.Read(ref pingStartedTime); }
			private set { Volatile.Write(ref pingStartedTime, value); }
		}

		public long PingEndedTime
		{
			get { return Volatile.Read(ref pingEndedTime); }
			private set { Volatile.Write(ref pingEndedTime, value); }
		}

		public PingReply Reply
		{
			get { return Volatile.Read(ref reply); }
			private set { Volatile.Write(ref reply, value); }
		}

		public PingerState State
		{
			get { return state; }
			private set { state = value; }
		}
		private readonly Task updater;

		public Pinger(string defaultHost, Dictionary<string, string> hostsPerApplication)
		{
			DefaultHost = defaultHost;
			HostsPerApplication = hostsPerApplication;
			updater = Task.Run((Action)(async () =>
			{
				while (true)
				{
					try
					{
						var ping = new Ping();
						while (true)
						{
							var pingReplyTask = ping.SendPingAsync(ChooseHost());
							PingStartedTime = Time.GetMillisecondsSinceEpoch();
							State = PingerState.RequestSent;
							try
							{
								Reply = await pingReplyTask;
							}
							catch (Exception e)
							{
								Reply = null;
							}
							PingEndedTime = Time.GetMillisecondsSinceEpoch();
							State = PingerState.ResponseRecieved;
							var newActivator = new TaskCompletionSource<bool>();
							Volatile.Write(ref pingActivator, newActivator);
							await newActivator.Task;
						}
					}
					catch (Exception exc)
					{
						Global.logger.Error(exc, "PingCounter exception: ");
					}
					await Task.Delay(500);
				}
			}));
		}


		private TaskCompletionSource<bool> pingActivator;

		public bool SendRequest()
		{
			var pingActivatorCopy = Volatile.Read(ref pingActivator);
			if (pingActivatorCopy != null)
			{
				if (Interlocked.CompareExchange(ref pingActivator, null, pingActivatorCopy) == pingActivatorCopy)
				{
					Task.Run(() => pingActivatorCopy.SetResult(true));
					return true;
				}
			}
			return false;
		}

		public void Reset()
		{
			Reply = null;
			State = PingerState.Initial;
		}

		private string ChooseHost()
		{
			try
			{
				var activeProcessName = ActiveProcessMonitor.Instance.ProcessName;
				if (!string.IsNullOrWhiteSpace(activeProcessName))
					activeProcessName = Path.GetFileName(activeProcessName).ToLower();
				var activeWindowTitle = ActiveProcessMonitor.Instance.ProcessTitle;

				if (HostsPerApplication != null)
				{
					string host;
					if (HostsPerApplication.TryGetValue(activeProcessName, out host)
						|| HostsPerApplication.TryGetValue(activeWindowTitle, out host))
					{
						return host;
					}
				}
			}
			catch (Exception exception)
			{
				Global.logger.Error(exception, "PingEffect error:");
			}
			return DefaultHost;
		}
	}
}