using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public class GlitchLayerHandlerProperties<TProperty> : LayerHandlerProperties2Color<TProperty> where TProperty : GlitchLayerHandlerProperties<TProperty>
    {
        public double? _UpdateInterval { get; set; }

        [JsonIgnore]
        public double UpdateInterval { get { return Logic._UpdateInterval ?? _UpdateInterval ?? 1.0; } }

        public bool? _AllowTransparency { get; set; }

        [JsonIgnore]
        public bool AllowTransparency { get { return Logic._AllowTransparency ?? _AllowTransparency ?? false; } }

        public GlitchLayerHandlerProperties() : base() { }

        public GlitchLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._UpdateInterval = 1.0;
        }
    }

    public class GlitchLayerHandlerProperties : GlitchLayerHandlerProperties<GlitchLayerHandlerProperties>
    {
        public GlitchLayerHandlerProperties() : base() { }

        public GlitchLayerHandlerProperties(bool empty = false) : base(empty) { }
    }

    public class GlitchLayerHandler<TProperty> : LayerHandler<TProperty> where TProperty : GlitchLayerHandlerProperties<TProperty>
    {
        private static Random randomizer = new Random();

        private Dictionary<Devices.DeviceKeys, Color> _GlitchColors = new Dictionary<Devices.DeviceKeys, Color>();

        private long previoustime = 0;
        private long currenttime = 0;

        private float getDeltaTime()
        {
            return (currenttime - previoustime) / 1000.0f;
        }

        public override EffectLayer Render(IGameState state)
        {
            currenttime = Utils.Time.GetMillisecondsSinceEpoch();

            if(previoustime + (Properties.UpdateInterval * 1000L) <= currenttime)
            {
                previoustime = currenttime;

                foreach(Devices.DeviceKeys key in Enum.GetValues(typeof(Devices.DeviceKeys)))
                {
                    Color clr = (Properties.AllowTransparency ? (randomizer.Next() % 2 == 0 ? Color.Transparent : Utils.ColorUtils.GenerateRandomColor()) : Utils.ColorUtils.GenerateRandomColor());

                    if (_GlitchColors.ContainsKey(key))
                        _GlitchColors[key] = clr;
                    else
                        _GlitchColors.Add(key, clr);
                }
            }

            EffectLayer _GlitchLayer = new EffectLayer();

            foreach (var kvp in _GlitchColors)
            {
                _GlitchLayer.Set(kvp.Key, kvp.Value);
            }

            return _GlitchLayer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_GlitchLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }

    public class GlitchLayerHandler : GlitchLayerHandler<GlitchLayerHandlerProperties>
    {
        public GlitchLayerHandler() : base()
        {
            _ID = "Glitch";
        }

        protected override UserControl CreateControl()
        {
            return new Control_GlitchLayer(this);
        }
    }
}
