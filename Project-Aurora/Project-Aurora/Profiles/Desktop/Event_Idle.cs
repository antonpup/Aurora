using Aurora.Devices.Layout;
using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Aurora.Profiles.Desktop
{
    public class Event_Idle : LightEvent
    {
        private long previoustime = 0;
        private long currenttime = 0;

        private Random randomizer;

        private LayerEffectConfig effect_cfg = new LayerEffectConfig();

        private List<DeviceLED> allKeys => GlobalDeviceLayout.Instance.AllLeds;
        private Dictionary<DeviceLED, float> stars = new Dictionary<DeviceLED, float>();
        private Dictionary<DeviceLED, float> raindrops = new Dictionary<DeviceLED, float>();
        private AnimationMix matrix_lines = new AnimationMix().SetAutoRemove(true); //This will be an infinite Mix
        long nextstarset = 0L;

        private float getDeltaTime()
        {
            return (currenttime - previoustime) / 1000.0f;
        }

        public Event_Idle()
        {
            randomizer = new Random();

            previoustime = currenttime;
            currenttime = Utils.TimeUtils.GetMillisecondsSinceEpoch();
        }

        public override void UpdateLights(EffectFrame frame)
        {
            UpdateLights(frame, App.Core.LightingStateManager.Settings);
        }
        public void UpdateLights(EffectFrame frame, LightingStateManagerSettings settings)
        {
            previoustime = currenttime;
            currenttime = Utils.TimeUtils.GetMillisecondsSinceEpoch();

            Queue<EffectLayer> layers = new Queue<EffectLayer>();
            EffectLayer layer;

            effect_cfg.speed = settings.IdleSpeed;

            switch (settings.IdleType)
            {
                case IdleEffects.Dim:
                    layer = new EffectLayer("Idle - Dim");

                    layer.Fill(Color.FromArgb(125, 0, 0, 0));

                    layers.Enqueue(layer);
                    break;
                case IdleEffects.ColorBreathing:
                    layer = new EffectLayer("Idle - Color Breathing");

                    Color breathe_bg_color = settings.IdleEffectSecondaryColor;
                    layer.Fill(breathe_bg_color);

                    float sine = (float)Math.Pow(Math.Sin((double)((currenttime % 10000L) / 10000.0f) * 2 * Math.PI * settings.IdleSpeed), 2);

                    layer.Fill(Color.FromArgb((byte)(sine * 255), settings.IdleEffectPrimaryColor));

                    layers.Enqueue(layer);
                    break;
                case IdleEffects.RainbowShift_Horizontal:
                    layer = new EffectLayer("Idle - Rainbow Shift (Horizontal)", LayerEffects.RainbowShift_Horizontal, effect_cfg);

                    layers.Enqueue(layer);
                    break;
                case IdleEffects.RainbowShift_Vertical:
                    layer = new EffectLayer("Idle - Rainbow Shift (Vertical)", LayerEffects.RainbowShift_Vertical, effect_cfg);

                    layers.Enqueue(layer);
                    break;
                case IdleEffects.StarFall:
                    layer = new EffectLayer("Idle - Starfall");

                    if (nextstarset < currenttime)
                    {
                        for (int x = 0; x < settings.IdleAmount; x++)
                        {
                            DeviceLED star = allKeys[randomizer.Next(allKeys.Count)];
                            if (stars.ContainsKey(star))
                                stars[star] = 1.0f;
                            else
                                stars.Add(star, 1.0f);
                        }

                        nextstarset = currenttime + (long)(1000L * settings.IdleFrequency);
                    }

                    layer.Fill(settings.IdleEffectSecondaryColor);

                    DeviceLED[] stars_keys = stars.Keys.ToArray();

                    foreach (DeviceLED star in stars_keys)
                    {
                        layer.Set(star, Utils.ColorUtils.MultiplyColorByScalar(settings.IdleEffectPrimaryColor, stars[star]));
                        stars[star] -= getDeltaTime() * 0.05f * settings.IdleSpeed;
                    }

                    layers.Enqueue(layer);
                    break;
                case IdleEffects.RainFall:
                    layer = new EffectLayer("Idle - Rainfall");

                    if (nextstarset < currenttime)
                    {
                        for (int x = 0; x < settings.IdleAmount; x++)
                        {
                            DeviceLED star = allKeys[randomizer.Next(allKeys.Count)];
                            if (raindrops.ContainsKey(star))
                                raindrops[star] = 1.0f;
                            else
                                raindrops.Add(star, 1.0f);
                        }

                        nextstarset = currenttime + (long)(1000L * settings.IdleFrequency);
                    }

                    layer.Fill(settings.IdleEffectSecondaryColor);

                    DeviceLED[] raindrops_keys = raindrops.Keys.ToArray();

                    ColorSpectrum drop_spec = new ColorSpectrum(settings.IdleEffectPrimaryColor, Color.FromArgb(0, settings.IdleEffectPrimaryColor));

                    foreach (DeviceLED raindrop in raindrops_keys)
                    {
                        PointF pt = GlobalDeviceLayout.Instance.GetDeviceLEDBitmapRegion(raindrop).Center;

                        float transition_value = 1.0f - raindrops[raindrop];
                        float radius = transition_value * GlobalDeviceLayout.Instance.CanvasBiggest;

                        layer.GetCanvas().DrawEllipse(new Pen(drop_spec.GetColorAt(transition_value), 2),
                            new RectangleF(pt.X - radius,
                            pt.Y - radius,
                            2 * radius,
                            2 * radius));

                        raindrops[raindrop] -= getDeltaTime() * 0.05f * settings.IdleSpeed;
                    }

                    layers.Enqueue(layer);
                    break;
                case IdleEffects.Blackout:
                    layer = new EffectLayer("Idle - Blackout");

                    layer.Fill(Color.Black);

                    layers.Enqueue(layer);
                    break;
                case IdleEffects.Matrix:
                    layer = new EffectLayer("Idle - Matrix");

                    if (nextstarset < currenttime)
                    {
                        Color darker_primary = Utils.ColorUtils.MultiplyColorByScalar(settings.IdleEffectPrimaryColor, 0.50);

                        for (int x = 0; x < settings.IdleAmount; x++)
                        {
                            int width_start = randomizer.Next(GlobalDeviceLayout.Instance.CanvasWidth);
                            float delay = randomizer.Next(550) / 100.0f;
                            int random_id = randomizer.Next(125536789);

                            //Create animation
                            AnimationTrack matrix_line =
                                new AnimationTrack("Matrix Line (Head) " + random_id, 0.0f).SetFrame(
                                    0.0f * 1.0f / (0.05f * settings.IdleSpeed), new AnimationLine(width_start, -3, width_start, 0, settings.IdleEffectPrimaryColor, 3)).SetFrame(
                                    0.5f * 1.0f / (0.05f * settings.IdleSpeed), new AnimationLine(width_start, GlobalDeviceLayout.Instance.CanvasHeight, width_start, GlobalDeviceLayout.Instance.CanvasHeight + 3, settings.IdleEffectPrimaryColor, 3)).SetShift(
                                    (currenttime % 1000000L) / 1000.0f + delay
                                    );

                            AnimationTrack matrix_line_trail =
                                new AnimationTrack("Matrix Line (Trail) " + random_id, 0.0f).SetFrame(
                                    0.0f * 1.0f / (0.05f * settings.IdleSpeed), new AnimationLine(width_start, -12, width_start, -3, darker_primary, 3)).SetFrame(
                                    0.5f * 1.0f / (0.05f * settings.IdleSpeed), new AnimationLine(width_start, GlobalDeviceLayout.Instance.CanvasHeight - 12, width_start, GlobalDeviceLayout.Instance.CanvasHeight, darker_primary, 3)).SetFrame(
                                    0.75f * 1.0f / (0.05f * settings.IdleSpeed), new AnimationLine(width_start, GlobalDeviceLayout.Instance.CanvasHeight, width_start, GlobalDeviceLayout.Instance.CanvasHeight, darker_primary, 3)).SetShift(
                                    (currenttime % 1000000L) / 1000.0f + delay
                                    );

                            matrix_lines.AddTrack(matrix_line);
                            matrix_lines.AddTrack(matrix_line_trail);
                        }

                        nextstarset = currenttime + (long)(1000L * settings.IdleFrequency);
                    }

                    layer.Fill(settings.IdleEffectSecondaryColor);

                    Canvas g = layer.GetCanvas();
                    matrix_lines.Draw(g, (currenttime % 1000000L) / 1000.0f);

                    layers.Enqueue(layer);
                    break;
                case IdleEffects.RainFallSmooth:
					layer = new EffectLayer("Idle - RainfallSmooth");

					if (nextstarset < currenttime)
					{
						for (int x = 0; x < settings.IdleAmount; x++)
						{
							DeviceLED star = allKeys[randomizer.Next(allKeys.Count)];
							if (raindrops.ContainsKey(star))
								raindrops[star] = 1.0f;
							else
								raindrops.Add(star, 1.0f);
						}

						nextstarset = currenttime + (long)(1000L * settings.IdleFrequency);
					}
					layer.Fill(settings.IdleEffectSecondaryColor);

					ColorSpectrum drop_spec2 = new ColorSpectrum(
						settings.IdleEffectPrimaryColor,
						Color.FromArgb(0, settings.IdleEffectPrimaryColor));

					var drops = raindrops.Keys.ToArray().Select(d =>
					{
						PointF pt = GlobalDeviceLayout.Instance.GetDeviceLEDBitmapRegion(d).Center;
						float transitionValue = 1.0f - raindrops[d];
						float radius = transitionValue * GlobalDeviceLayout.Instance.CanvasBiggest;
						raindrops[d] -= getDeltaTime() * 0.05f * settings.IdleSpeed;
						return new Tuple<DeviceLED, PointF, float, float>(d, pt, transitionValue, radius);

					}).Where(d => d.Item3 <= 1.5).ToArray();

					float circleHalfThickness = 1f;

					foreach (var key in allKeys)
					{
						var keyInfo = GlobalDeviceLayout.Instance.GetDeviceLEDBitmapRegion(key);

						// For easy calculation every button considered as circle with this radius
						var btnRadius = ((keyInfo.Width + keyInfo.Height) / 4f);
						if (btnRadius <= 0) continue;

						foreach (var raindrop in drops)
						{

							float circleInEdge = (raindrop.Item4 - circleHalfThickness);
							float circleOutEdge = (raindrop.Item4 + circleHalfThickness);
							circleInEdge *= circleInEdge;
							circleOutEdge *= circleOutEdge;

							float xKey = Math.Abs(keyInfo.Center.X - raindrop.Item2.X);
							float yKey = Math.Abs(keyInfo.Center.Y - raindrop.Item2.Y);
							float xKeyInEdge = xKey - btnRadius;
							float xKeyOutEdge = xKey + btnRadius;
							float yKeyInEdge = yKey - btnRadius;
							float yKeyOutEdge = yKey + btnRadius;
							float keyInEdge = xKeyInEdge * xKeyInEdge + yKeyInEdge * yKeyInEdge;
							float keyOutEdge = xKeyOutEdge * xKeyOutEdge + yKeyOutEdge * yKeyOutEdge;

							var btnDiameter = keyOutEdge - keyInEdge;
							var inEdgePercent = (circleOutEdge - keyInEdge) / btnDiameter;
							var outEdgePercent = (keyOutEdge - circleInEdge) / btnDiameter;
							var percent = Math.Min(1, Math.Max(0, inEdgePercent))
								+ Math.Min(1, Math.Max(0, outEdgePercent)) - 1f;

							if (percent > 0)
							{
								layer.Set(key, (Color)EffectColor.BlendColors(
									new EffectColor(layer.Get(key)),
									new EffectColor(drop_spec2.GetColorAt(raindrop.Item3)), percent));
							}
						}
					}
					layers.Enqueue(layer);
					break;
                default:
                    break;
            }

            frame.AddOverlayLayers(layers.ToArray());
        }

        public override void SetGameState(IGameState new_game_state)
        {
            //This event does not take a game state
            //UpdateLights(frame);
        }

        public new bool IsEnabled
        {
            get { return true; }
        }
    }
}
