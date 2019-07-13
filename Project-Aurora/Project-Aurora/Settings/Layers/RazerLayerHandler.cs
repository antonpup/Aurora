using Aurora.Devices;
using Aurora.Devices.Razer;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Newtonsoft.Json;
using RazerSdkWrapper;
using RazerSdkWrapper.Data;
using RazerSdkWrapper.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public class RazerLayerHandlerProperties : LayerHandlerProperties<RazerLayerHandlerProperties>
    {
        // Color Enhancing
        public bool? _ColorPostProcessEnabled { get; set; }
        [JsonIgnore]
        public bool ColorPostProcessEnabled => Logic._ColorPostProcessEnabled ?? _ColorPostProcessEnabled ?? false;

        public double? _BrightnessBoost { get; set; }
        [JsonIgnore]
        public double BrightnessBoost => Logic._BrightnessBoost ?? _BrightnessBoost ?? 0;

        public double? _SaturationBoost { get; set; }
        [JsonIgnore]
        public double SaturationBoost => Logic._SaturationBoost ?? _SaturationBoost ?? 0;

        public double? _HueShift { get; set; }
        [JsonIgnore]
        public double HueShift => Logic._HueShift ?? _HueShift ?? 0;

        public Dictionary<DeviceKeys, DeviceKeys> _KeyCloneMap { get; set; }
        [JsonIgnore]
        public Dictionary<DeviceKeys, DeviceKeys> KeyCloneMap => Logic._KeyCloneMap ?? _KeyCloneMap ?? new Dictionary<DeviceKeys, DeviceKeys>();

        public RazerLayerHandlerProperties() : base() { }

        public RazerLayerHandlerProperties(bool arg = false) : base(arg) { }

        public override void Default()
        {
            base.Default();

            _ColorPostProcessEnabled = false;
            _BrightnessBoost = 0;
            _SaturationBoost = 0;
            _HueShift = 0;
            _KeyCloneMap = new Dictionary<DeviceKeys, DeviceKeys>();
        }
    }

    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("_Sequence")]
    public class RazerLayerHandler : LayerHandler<RazerLayerHandlerProperties>
    {
        private static RzManager _manager;
        private static int _instances;

        private Color[] _keyboardColors;
        private Color[] _mousepadColors;
        private Color _mouseColor;
        private string _currentAppExecutable;
        private int _currentAppPid;

        public RazerLayerHandler()
        {
            _ID = "Razer";
            _keyboardColors = new Color[22 * 6];
            _mousepadColors = new Color[16];

            _instances++;
            if (_instances > 0 && _manager == null)
            {
                if (RzHelper.GetSdkVersion() != new RzSdkVersion(3, 5, 6))
                {
                    Global.logger.Warn("Currently installed razer sdk version \"{0}\" is not supported!", RzHelper.GetSdkVersion());
                    return;
                }

                try
                {
                    _manager = new RzManager()
                    {
                        KeyboardEnabled = true,
                        MouseEnabled = true,
                        MousepadEnabled = true,
                        AppListEnabled = true,
                    };

                    Global.logger.Info("RzManager loaded successfully!");
                }
                catch (Exception e)
                {
                    Global.logger.Fatal(e, "RzManager failed to load!");
                }
            }

            if (_manager != null)
            {
                _manager.DataUpdated += OnDataUpdated;

                var appList = _manager.GetDataProvider<RzAppListDataProvider>();
                appList.Update();
                _currentAppExecutable = appList.CurrentAppExecutable;
                _currentAppPid = appList.CurrentAppPid;
            }
        }

        protected override UserControl CreateControl()
        {
            return new Control_RazerLayer(this);
        }

        private void OnDataUpdated(object s, EventArgs e)
        {
            if (!(s is AbstractDataProvider provider))
                return;

            provider.Update();
            if (provider is RzKeyboardDataProvider keyboard)
            {
                for (var i = 0; i < keyboard.ZoneGrid.Height * keyboard.ZoneGrid.Width; i++)
                    _keyboardColors[i] = keyboard.GetZoneColor(i);
            }
            else if (provider is RzMouseDataProvider mouse)
            {
                _mouseColor = mouse.GetZoneColor(55);
            }
            else if (provider is RzMousepadDataProvider mousePad)
            {
                for (var i = 0; i < mousePad.ZoneGrid.Height * mousePad.ZoneGrid.Width; i++)
                    _mousepadColors[i] = mousePad.GetZoneColor(i);
            }
            else if(provider is RzAppListDataProvider appList)
            {
                _currentAppExecutable = appList.CurrentAppExecutable;
                _currentAppPid = appList.CurrentAppPid;
            }
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            var layer = new EffectLayer();

            if (string.IsNullOrEmpty(_currentAppExecutable) || string.Compare(_currentAppExecutable, "Aurora.exe", true) == 0)
            {
                return layer;
            }

            foreach (var key in (DeviceKeys[])Enum.GetValues(typeof(DeviceKeys)))
            {
                Color color;
                if( RazerLayoutMap.GenericKeyboard.TryGetValue(key, out var position))
                    color = PostProcessColor(_keyboardColors[position[1] + position[0] * 22]);
                else if (key >= DeviceKeys.MOUSEPADLIGHT1 && key <= DeviceKeys.MOUSEPADLIGHT15)
                    color = PostProcessColor(_mousepadColors[DeviceKeys.MOUSEPADLIGHT15 - key]);
                else if (key == DeviceKeys.Peripheral)
                    color = PostProcessColor(_mouseColor);
                else
                    continue;

                layer.Set(key, color);

                if (Properties.KeyCloneMap != null)
                    foreach(var target in Properties.KeyCloneMap.Where(x => x.Value == key).Select(x => x.Key))
                        layer.Set(target, color);
            }

            return layer;
        }

        private Color PostProcessColor(Color color)
        {
            if (!Properties.ColorPostProcessEnabled)
                return color;
            if (color.R == 0 && color.G == 0 && color.B == 0)
                return color;

            ToHsv(color, out var hue, out var saturation, out var value);

            if (Properties.BrightnessBoost > 0)
                value = Math.Pow(value, 1 - Properties.BrightnessBoost);
            if (Properties.SaturationBoost > 0 && saturation > 0)
                saturation = Math.Pow(saturation, 1 - Properties.SaturationBoost);
            if (Properties.HueShift > 0)
                hue = (hue + Properties.HueShift) % 360;

            return FromHsv(hue, saturation, value);
        }

        private void ToHsv(Color color, out double hue, out double saturation, out double value)
        {
            var max = Math.Max(color.R, Math.Max(color.G, color.B));
            var min = Math.Min(color.R, Math.Min(color.G, color.B));

            var delta = max - min;

            hue = 0d;
            if (delta != 0)
            {
                if (color.R == max) hue = (color.G - color.B) / (double)delta;
                else if (color.G == max) hue = 2d + (color.B - color.R) / (double)delta;
                else if (color.B == max) hue = 4d + (color.R - color.G) / (double)delta;
            }

            hue *= 60;
            if (hue < 0.0) hue += 360;

            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        private Color FromHsv(double hue, double saturation, double value)
        {
            saturation = Math.Max(Math.Min(saturation, 1), 0);
            value = Math.Max(Math.Min(value, 1), 0);

            var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            var f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            var v = (byte)(value);
            var p = (byte)(value * (1 - saturation));
            var q = (byte)(value * (1 - f * saturation));
            var t = (byte)(value * (1 - (1 - f) * saturation));

            switch (hi)
            {
                case 0: return Color.FromArgb(v, t, p);
                case 1: return Color.FromArgb(q, v, p);
                case 2: return Color.FromArgb(p, v, t);
                case 3: return Color.FromArgb(p, q, v);
                case 4: return Color.FromArgb(t, p, v);
                default: return Color.FromArgb(v, p, q);
            }
        }

        public override void Dispose()
        {
            _instances--;
            if (_manager != null)
            {
                _manager.DataUpdated -= OnDataUpdated;
                if (_instances == 0)
                {
                    _manager.Dispose();
                    _manager = null;
                }
            }

            if (_instances < 0)
                _instances = 0;

            base.Dispose();
        }
    }
}
