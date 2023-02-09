using Aurora.Devices;
using Aurora.Devices.Razer;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Aurora.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using Aurora.Modules.Razer;
using Aurora.Settings.Layers.Controls;

namespace Aurora.Settings.Layers
{
    public class RazerLayerHandlerProperties : LayerHandlerProperties<RazerLayerHandlerProperties>
    {
        
        [LogicOverridable("Enable Transparency")] public bool? _TransparencyEnabled { get; set; }
        [JsonIgnore] public bool TransparencyEnabled => Logic._TransparencyEnabled ?? false;

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

        public Dictionary<DeviceKeys, DeviceKeys> _KeyCloneMap { get; set; }
        [JsonIgnore]
        public Dictionary<DeviceKeys, DeviceKeys> KeyCloneMap => Logic._KeyCloneMap ?? _KeyCloneMap ?? new Dictionary<DeviceKeys, DeviceKeys>();

        public RazerLayerHandlerProperties() : base() { }

        public RazerLayerHandlerProperties(bool arg = false) : base(arg) { }

        public override void Default()
        {
            base.Default();

            _ColorPostProcessEnabled = false;
            _BrightnessChange = 0;
            _SaturationChange = 0;
            _HueShift = 0;
            _KeyCloneMap = new Dictionary<DeviceKeys, DeviceKeys>();
        }
    }

    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("_Sequence")]
    [LayerHandlerMeta(Name = "Razer Chroma", IsDefault = true)]
    public class RazerLayerHandler : LayerHandler<RazerLayerHandlerProperties>
    {
        public RazerLayerHandler() : base("Chroma Layer")
        {
        }

        protected override UserControl CreateControl()
        {
            return new Control_RazerLayer(this);
        }

        private bool _empty = true;
        public override EffectLayer Render(IGameState gamestate)
        {
            if (!RzHelper.IsCurrentAppValid())
            {
                return EffectLayer.EmptyLayer;
            }
            _empty = false;
            RzHelper.UpdateIfStale();

            foreach (var key in (DeviceKeys[])Enum.GetValues(typeof(DeviceKeys)))
            {
                if (!TryGetColor(key, out var color))
                    continue;
                
                EffectLayer.Set(key, color);
            }

            if (Properties.KeyCloneMap == null) return EffectLayer;
            foreach (var target in Properties.KeyCloneMap)
                if(TryGetColor(target.Value, out var clr))
                    EffectLayer.Set(target.Key, clr);

            return EffectLayer;
        }

        private bool TryGetColor(DeviceKeys key, out Color color)
        {
            (byte r, byte g, byte b) rColor = (0, 0, 0);
            if (RazerLayoutMap.GenericKeyboard.TryGetValue(key, out var position))
                rColor = RzHelper.KeyboardColors[position[1] + position[0] * 22];
            else if (key >= DeviceKeys.MOUSEPADLIGHT1 && key <= DeviceKeys.MOUSEPADLIGHT15)
                rColor = RzHelper.MousepadColors[DeviceKeys.MOUSEPADLIGHT15 - key];
            else if (RazerLayoutMap.Mouse.TryGetValue(key, out position))
                rColor = RzHelper.MouseColors[position[1] + position[0] * 7];
            else
            {
                color = Color.Transparent;
                return false;
            }

            if (Properties.ColorPostProcessEnabled)
                rColor = PostProcessColor(rColor);
            color = FastTransform(rColor);

            return true;
        }

        private (byte r, byte g, byte b) PostProcessColor((byte r, byte g, byte b) color)
        {
            if (color.r == 0 && color.g == 0 && color.b == 0)
                return color;

            if (Properties.BrightnessChange != 0)
                color = ColorUtils.ChangeBrightness(color, Properties.BrightnessChange);
            if (Properties.SaturationChange != 0)
                color = ColorUtils.ChangeSaturation(color, Properties.SaturationChange);
            if (Properties.HueShift != 0)
                color = ColorUtils.ChangeHue(color, Properties.HueShift);

            return color;
        }

        private Color FastTransform((byte r, byte g, byte b) color)
        {
            return Properties.TransparencyEnabled ?
                ColorUtils.FastColorTransparent(color.r, color.g, color.b) :
                ColorUtils.FastColor(color.r, color.g, color.b);
        }
    }
}
