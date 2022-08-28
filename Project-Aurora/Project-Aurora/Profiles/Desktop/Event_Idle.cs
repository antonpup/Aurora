using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Aurora.Settings;
using Aurora.Utils;

namespace Aurora.Profiles.Desktop
{
    public class Event_Idle : LightEvent
    {
	    private readonly EffectLayer _layer = new("IDLE");

        private long _previousTime;
        private long _currentTime;

        private readonly Random _randomizer;

        private readonly LayerEffectConfig _effectCfg = new();

        private readonly DeviceKeys[] _allKeys = Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>().ToArray();
        
        private readonly Brush _dimBrush = new SolidBrush(Color.FromArgb(125, 0, 0, 0));
        
        private readonly SolidBrush _idEffectSecondaryColorBrush = new(Color.White);
        
        private readonly Dictionary<DeviceKeys, float> _stars = new();
        private long _nextStarSet;
        
        private readonly Dictionary<DeviceKeys, float> _raindrops = new();
        private ColorSpectrum _dropSpec = new(Global.Configuration.IdleEffectPrimaryColor, Color.FromArgb(0, Global.Configuration.IdleEffectPrimaryColor));
        private ColorSpectrum _dropSpec2 = new(Global.Configuration.IdleEffectPrimaryColor, Color.FromArgb(0, Global.Configuration.IdleEffectPrimaryColor));
        
        private readonly AnimationMix _matrixLines = new AnimationMix().SetAutoRemove(true); //This will be an infinite Mix

        public Event_Idle()
        {
	        _randomizer = new Random();

	        _previousTime = _currentTime;
	        _currentTime = Time.GetMillisecondsSinceEpoch();
	        
	        Global.Configuration.PropertyChanged += IdleTypeChanged;
        }

        protected virtual void Dispose(bool disposing)
        {
	        if (disposing)
	        {
		        Global.Configuration.PropertyChanged -= IdleTypeChanged;
	        }
        }

        public sealed override void Dispose()
        {
	        Dispose(true);
	        GC.SuppressFinalize(this);
        }

        private bool _invalidated;
        private void IdleTypeChanged(object sender, PropertyChangedEventArgs e)
        {
	        _dropSpec = new ColorSpectrum(Global.Configuration.IdleEffectPrimaryColor, Color.FromArgb(0, Global.Configuration.IdleEffectPrimaryColor));
	        _dropSpec2 = new ColorSpectrum(Global.Configuration.IdleEffectPrimaryColor, Color.FromArgb(0, Global.Configuration.IdleEffectPrimaryColor));
	        _invalidated = true;
        }


        private float GetDeltaTime()
        {
            return (_currentTime - _previousTime) / 1000.0f;
        }
        
        public override void UpdateLights(EffectFrame frame)
        {
	        if (_invalidated)
	        {
		        _layer.Fill(Brushes.Transparent);
		        _invalidated = false;
	        }
	        
            _previousTime = _currentTime;
            _currentTime = Time.GetMillisecondsSinceEpoch();

            _effectCfg.speed = Global.Configuration.IdleSpeed;
            _idEffectSecondaryColorBrush.Color = Global.Configuration.IdleEffectSecondaryColor;
            switch (Global.Configuration.IdleType)
            {
                case IdleEffects.Dim:
                    _layer.Fill(_dimBrush);
                    break;
                case IdleEffects.ColorBreathing:
                    _layer.Fill(_idEffectSecondaryColorBrush);

                    var sine = (float)Math.Pow(Math.Sin((double)(_currentTime % 10000L / 10000.0f) * 2 * Math.PI * Global.Configuration.IdleSpeed), 2);

                    _layer.FillOver(Color.FromArgb((byte)(sine * 255), Global.Configuration.IdleEffectPrimaryColor));
                    break;
                case IdleEffects.RainbowShift_Horizontal:
                    _layer.DrawGradient(LayerEffects.RainbowShift_Horizontal, _effectCfg);
                    break;
                case IdleEffects.RainbowShift_Vertical:
	                _layer.DrawGradient(LayerEffects.RainbowShift_Vertical, _effectCfg);
                    break;
                case IdleEffects.StarFall:
                    if (_nextStarSet < _currentTime)
                    {
                        for (var x = 0; x < Global.Configuration.IdleAmount; x++)
                        {
                            var star = _allKeys[_randomizer.Next(_allKeys.Length)];
                            if (_stars.ContainsKey(star))
                                _stars[star] = 1.0f;
                            else
                                _stars.Add(star, 1.0f);
                        }

                        _nextStarSet = _currentTime + (long)(1000L * Global.Configuration.IdleFrequency);
                    }

                    _layer.Fill(_idEffectSecondaryColorBrush);

                    var starsKeys = _stars.Keys.ToArray();

                    foreach (var star in starsKeys)
                    {
                        _layer.Set(star, ColorUtils.BlendColors(Color.Black, Global.Configuration.IdleEffectPrimaryColor, _stars[star]));
                        _stars[star] -= GetDeltaTime() * 0.05f * Global.Configuration.IdleSpeed;
                    }
                    break;
                case IdleEffects.RainFall:
                    if (_nextStarSet < _currentTime)
                    {
                        for (var x = 0; x < Global.Configuration.IdleAmount; x++)
                        {
                            var star = _allKeys[_randomizer.Next(_allKeys.Length)];
                            if (_raindrops.ContainsKey(star))
                                _raindrops[star] = 1.0f;
                            else
                                _raindrops.Add(star, 1.0f);
                        }

                        _nextStarSet = _currentTime + (long)(1000L * Global.Configuration.IdleFrequency);
                    }

                    _layer.Fill(_idEffectSecondaryColorBrush);

                    var raindropsKeys = _raindrops.Keys.ToArray();

                    foreach (var raindrop in raindropsKeys)
                    {
                        var pt = Effects.GetBitmappingFromDeviceKey(raindrop).Center;

                        var transitionValue = 1.0f - _raindrops[raindrop];
                        var radius = transitionValue * Effects.CanvasBiggest;

                        using(var g = _layer.GetGraphics())
                            g.DrawEllipse(new Pen(_dropSpec.GetColorAt(transitionValue), 2),
                                pt.X - radius,
                                pt.Y - radius,
                                2 * radius,
                                2 * radius);

                        _raindrops[raindrop] -= GetDeltaTime() * 0.05f * Global.Configuration.IdleSpeed;
                    }
                    break;
                case IdleEffects.Blackout:
                    _layer.Fill(Brushes.Black);
                    break;
                case IdleEffects.Matrix:
                    if (_nextStarSet < _currentTime)
                    {
                        var darkerPrimary = ColorUtils.MultiplyColorByScalar(Global.Configuration.IdleEffectPrimaryColor, 0.50);

                        for (var x = 0; x < Global.Configuration.IdleAmount; x++)
                        {
                            var widthStart = _randomizer.Next(Effects.CanvasWidth);
                            var delay = _randomizer.Next(550) / 100.0f;
                            var randomId = _randomizer.Next(125536789);

                            //Create animation
                            var matrixLine =
                                new AnimationTrack("Matrix Line (Head) " + randomId, 0.0f).SetFrame(
                                    0.0f * 1.0f / (0.05f * Global.Configuration.IdleSpeed), new AnimationLine(widthStart, -3, widthStart, 0, Global.Configuration.IdleEffectPrimaryColor, 3)).SetFrame(
                                    0.5f * 1.0f / (0.05f * Global.Configuration.IdleSpeed), new AnimationLine(widthStart, Effects.CanvasHeight, widthStart, Effects.CanvasHeight + 3, Global.Configuration.IdleEffectPrimaryColor, 3)).SetShift(
                                    _currentTime % 1000000L / 1000.0f + delay
                                    );

                            var matrixLineTrail =
                                new AnimationTrack("Matrix Line (Trail) " + randomId, 0.0f).SetFrame(
                                    0.0f * 1.0f / (0.05f * Global.Configuration.IdleSpeed), new AnimationLine(widthStart, -12, widthStart, -3, darkerPrimary, 3)).SetFrame(
                                    0.5f * 1.0f / (0.05f * Global.Configuration.IdleSpeed), new AnimationLine(widthStart, Effects.CanvasHeight - 12, widthStart, Effects.CanvasHeight, darkerPrimary, 3)).SetFrame(
                                    0.75f * 1.0f / (0.05f * Global.Configuration.IdleSpeed), new AnimationLine(widthStart, Effects.CanvasHeight, widthStart, Effects.CanvasHeight, darkerPrimary, 3)).SetShift(
                                    _currentTime % 1000000L / 1000.0f + delay
                                    );

                            _matrixLines.AddTrack(matrixLine);
                            _matrixLines.AddTrack(matrixLineTrail);
                        }

                        _nextStarSet = _currentTime + (long)(1000L * Global.Configuration.IdleFrequency);
                    }

                    _layer.Fill(_idEffectSecondaryColorBrush);

                    using (var g = _layer.GetGraphics())
                    {
                        _matrixLines.Draw(g, (_currentTime % 1000000L) / 1000.0f);
                    }
                    break;
                case IdleEffects.RainFallSmooth:
					if (_nextStarSet < _currentTime)
					{
						for (var x = 0; x < Global.Configuration.IdleAmount; x++)
						{
							var star = _allKeys[_randomizer.Next(_allKeys.Length)];
							if (_raindrops.ContainsKey(star))
								_raindrops[star] = 1.0f;
							else
								_raindrops.Add(star, 1.0f);
						}

						_nextStarSet = _currentTime + (long)(1000L * Global.Configuration.IdleFrequency);
					}
					_layer.FillOver(Global.Configuration.IdleEffectSecondaryColor);

					var drops = _raindrops.Keys.ToArray().Select(d =>
					{
						var pt = Effects.GetBitmappingFromDeviceKey(d).Center;
						var transitionValue = 1.0f - _raindrops[d];
						var radius = transitionValue * Effects.CanvasBiggest;
						_raindrops[d] -= GetDeltaTime() * 0.05f * Global.Configuration.IdleSpeed;
						return new Tuple<DeviceKeys, PointF, float, float>(d, pt, transitionValue, radius);

					}).Where(d => d.Item3 <= 1.5).ToArray();

					var circleHalfThickness = 1f;

					foreach (var key in _allKeys)
					{
						var keyInfo = Effects.GetBitmappingFromDeviceKey(key);

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
								_layer.Set(key, (Color)EffectColor.BlendColors(
									new EffectColor(_layer.Get(key)),
									new EffectColor(_dropSpec2.GetColorAt(raindrop.Item3)), percent));
							}
						}
					}
					break;
            }

            frame.AddOverlayLayer(_layer);
        }

        public override void SetGameState(IGameState newGameState)
        {
            //This event does not take a game state
            //UpdateLights(frame);
        }
    }
}
