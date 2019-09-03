using Aurora.Devices;
using Aurora.Devices.Layout;
using Aurora.Devices.Layout.Layouts;
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

        public double? _BrightnessChange { get; set; }
        [JsonIgnore]
        public double BrightnessChange => Logic._BrightnessChange ?? _BrightnessChange ?? 0;

        public double? _SaturationChange { get; set; }
        [JsonIgnore]
        public double SaturationChange => Logic._SaturationChange ?? _SaturationChange ?? 0;

        public double? _HueShift { get; set; }
        [JsonIgnore]
        public double HueShift => Logic._HueShift ?? _HueShift ?? 0;

        public Dictionary<DeviceLED, DeviceLED> _KeyCloneMap { get; set; }
        [JsonIgnore]
        public Dictionary<DeviceLED, DeviceLED> KeyCloneMap => Logic._KeyCloneMap ?? _KeyCloneMap ?? new Dictionary<DeviceLED, DeviceLED>();

        public RazerLayerHandlerProperties() : base() { }

        public RazerLayerHandlerProperties(bool arg = false) : base(arg) { }

        public override void Default()
        {
            base.Default();

            _ColorPostProcessEnabled = false;
            _BrightnessChange = 0;
            _SaturationChange = 0;
            _HueShift = 0;
            _KeyCloneMap = new Dictionary<DeviceLED, DeviceLED>();
        }
    }

    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("_Sequence")]
    public class RazerLayerHandler : LayerHandler<RazerLayerHandlerProperties>
    {
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

            if (App.Core.razerManager != null)
            {
                App.Core.razerManager.DataUpdated += OnDataUpdated;

                var appList = App.Core.razerManager.GetDataProvider<RzAppListDataProvider>();
                appList.Update();
                _currentAppExecutable = appList.CurrentAppExecutable;
                _currentAppPid = appList.CurrentAppPid;
            }
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
            var root = App.LogsDirectory;
            if (!Directory.Exists(root))
                return false;

            var path = $@"{root}\RazerLayer";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            foreach (var file in Directory.EnumerateFiles(path, "*.bin", SearchOption.TopDirectoryOnly))
                File.Delete(file);

            App.logger.Info("RazerLayerHandler started dumping data");
            _isDumping = true;
            return true;
        }

        public void StopDumpingData()
        {
            App.logger.Info("RazerLayerHandler stopped dumping data");
            _isDumping = false;
        }

        public void DumpData(AbstractDataProvider provider)
        {
            var path = Path.Combine(App.LogsDirectory, "RazerLayer");
            var filename = $"{provider.GetType().Name}_{Environment.TickCount}.bin";
            using (var file = File.Open($@"{path}\{filename}", FileMode.Create)) {
                var data = provider.ReadData();
                file.Write(data, 0, data.Length);
            }
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            var layer = new EffectLayer();

            if (!IsCurrentAppValid())
                return layer;
            foreach (var key in GlobalDeviceLayout.Instance.AllLeds)
            {
                Color color;
                if (key.DeviceTypeID == KeyboardDeviceLayout.DeviceTypeID && RazerLayoutMap.GenericKeyboard.TryGetValue((KeyboardKeys)key.LedID, out var position))
                    color = _keyboardColors[position[1] + position[0] * 22];
                //TODO: Fix this
                /*else if (key >= DeviceKeys.MOUSEPADLIGHT1 && key <= DeviceKeys.MOUSEPADLIGHT15)
                    color = _mousepadColors[DeviceKeys.MOUSEPADLIGHT15 - key];
                else if (key == DeviceKeys.Peripheral)
                    color = _mouseColor;*/
                else
                    continue;

                if (Properties.ColorPostProcessEnabled)
                    color = PostProcessColor(color);

                layer.Set(key, color);

                if (Properties.KeyCloneMap != null)
                    foreach (var target in Properties.KeyCloneMap.Where(x => x.Value == key).Select(x => x.Key))
                        layer.Set(target, color);
            }

            return layer;
        }

        private bool IsCurrentAppValid() 
            => !string.IsNullOrEmpty(_currentAppExecutable)
            && string.Compare(_currentAppExecutable, "Aurora.exe", true) != 0;

        private Color PostProcessColor(Color color)
        {
            if (color.R == 0 && color.G == 0 && color.B == 0)
                return color;

            if (Properties.BrightnessChange != 0)
                color = ColorUtils.ChangeBrightness(color, Properties.BrightnessChange);
            if (Properties.SaturationChange != 0)
                color = ColorUtils.ChangeSaturation(color, Properties.SaturationChange);
            if (Properties.HueShift != 0)
                color = ColorUtils.ChangeHue(color, Properties.HueShift);

            return color;
        }

        public override void Dispose()
        {
            if(App.Core.razerManager != null)
                App.Core.razerManager.DataUpdated -= OnDataUpdated;

            base.Dispose();
        }
    }
}
