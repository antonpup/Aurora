using Aurora.Devices;
using Aurora.Devices.Razer;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Aurora.Utils;
using Newtonsoft.Json;
using RazerSdkWrapper;
using RazerSdkWrapper.Data;
using RazerSdkWrapper.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        public bool Loaded { get; private set; }

        private static RzManager _manager;
        private static int _instances;

        private Color[] _keyboardColors;
        private Color[] _mousepadColors;
        private Color _mouseColor;
        private string _currentAppExecutable;
        private int _currentAppPid;
        private bool _isDumping;

        public RazerLayerHandler()
        {
            _ID = "Razer";
            _keyboardColors = new Color[22 * 6];
            _mousepadColors = new Color[16];

            _instances++;
            Global.logger.Debug("RazerLayerHandler intance count: {0}", _instances);
            if (_instances > 0 && _manager == null)
            {
                if (!RzHelper.IsSdkVersionSupported(RzHelper.GetSdkVersion()))
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
                    Global.logger.Fatal("RzManager failed to load!");
                    Global.logger.Fatal(e.ToString());
                }
            }

            if (_manager != null)
            {
                _manager.DataUpdated += OnDataUpdated;

                var appList = _manager.GetDataProvider<RzAppListDataProvider>();
                appList.Update();
                _currentAppExecutable = appList.CurrentAppExecutable;
                _currentAppPid = appList.CurrentAppPid;

                Loaded = true;
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

            if (_isDumping)
                DumpData(provider);

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
            else if (provider is RzAppListDataProvider appList)
            {
                _currentAppExecutable = appList.CurrentAppExecutable;
                _currentAppPid = appList.CurrentAppPid;
            }
        }

        public bool StartDumpingData()
        {
            var root = Global.LogsDirectory;
            if (!Directory.Exists(root))
                return false;

            var path = $@"{root}\RazerLayer";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            foreach (var file in Directory.EnumerateFiles(path, "*.bin", SearchOption.TopDirectoryOnly))
                File.Delete(file);

            _isDumping = true;
            return true;
        }

        public void StopDumpingData()
        {
            _isDumping = false;
        }

        public void DumpData(AbstractDataProvider provider)
        {
            var path = Path.Combine(Global.LogsDirectory, "RazerLayer");
            var filename = $"{provider.GetType().Name}_{Environment.TickCount}.bin";
            using (var file = File.Open($@"{path}\{filename}", FileMode.Create)) {
                var data = provider.ReadData();
                file.Write(data, 0, data.Length);
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
                    color = _keyboardColors[position[1] + position[0] * 22];
                else if (key >= DeviceKeys.MOUSEPADLIGHT1 && key <= DeviceKeys.MOUSEPADLIGHT15)
                    color = _mousepadColors[DeviceKeys.MOUSEPADLIGHT15 - key];
                else if (key == DeviceKeys.Peripheral)
                    color = _mouseColor;
                else
                    continue;

                if (Properties.ColorPostProcessEnabled)
                    color = PostProcessColor(color);

                layer.Set(key, color);

                if (Properties.KeyCloneMap != null)
                    foreach(var target in Properties.KeyCloneMap.Where(x => x.Value == key).Select(x => x.Key))
                        layer.Set(target, color);
            }

            return layer;
        }

        private Color PostProcessColor(Color color)
        {
            if (color.R == 0 && color.G == 0 && color.B == 0)
                return color;

            ColorUtils.ToHsv(color, out var hue, out var saturation, out var value);

            if (Properties.BrightnessBoost > 0)
                value = Math.Pow(value, 1 - Properties.BrightnessBoost);
            if (Properties.SaturationBoost > 0 && saturation > 0)
                saturation = Math.Pow(saturation, 1 - Properties.SaturationBoost);
            if (Properties.HueShift > 0)
                hue = (hue + Properties.HueShift) % 360;

            return ColorUtils.FromHsv(hue, saturation, value);
        }

        public override void Dispose()
        {
            _instances--;
            Global.logger.Debug("RazerLayerHandler intance count: {0}", _instances);
            if (_manager != null)
            {
                _manager.DataUpdated -= OnDataUpdated;
                if (_instances == 0)
                {
                    try
                    {
                        _manager.Dispose();
                    }
                    catch (Exception e)
                    {
                        Global.logger.Fatal("RzManager failed to dispose!");
                        Global.logger.Fatal(e.ToString());
                    }
                    _manager = null;
                }
            }

            if (_instances < 0)
                _instances = 0;

            base.Dispose();
        }
    }
}
