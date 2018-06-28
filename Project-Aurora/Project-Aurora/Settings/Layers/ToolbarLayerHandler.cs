using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers {

    public class ToolbarLayerHandlerProperties : LayerHandlerProperties2Color<ToolbarLayerHandlerProperties> {

        public ToolbarLayerHandlerProperties() : base() { }
        public ToolbarLayerHandlerProperties(bool assign_default) : base(assign_default) { }

        public bool? _EnableScroll { get; set; }
        [JsonIgnore]
        public bool EnableScroll { get { return Logic._EnableScroll ?? _EnableScroll ?? false; } }

        public bool? _ScrollLoop { get; set; }
        [JsonIgnore]
        public bool ScrollLoop { get { return Logic._ScrollLoop ?? _ScrollLoop ?? true; } }

        public override void Default() {
            base.Default();
            _EnableScroll = false;
            _ScrollLoop = true;
        }
    }

    /// <summary>
    /// ToolbarLayer as suggested by amahlaka97 on Discord and scroll improvement as suggested by DrMeteor.
    /// When one of the keys it uses is pressed, that key becomes "active". When another key is pressed, that key becomes active instead.
    /// Sort of like a radio button but for keys on the keyboard :)
    /// The mouse scroll wheel can also be used to scroll up and down the active key (if EnableScroll is true).
    /// </summary>
    public class ToolbarLayerHandler : LayerHandler<ToolbarLayerHandlerProperties> {

        private DeviceKeys activeKey = DeviceKeys.NONE;

        public ToolbarLayerHandler() {
            _ID = "Toolbar";

            // Listen for relevant events
            Global.InputEvents.KeyDown += InputEvents_KeyDown;
            Global.InputEvents.Scroll += InputEvents_Scroll;
        }

        public override void Dispose() {
            // Remove listeners on dispose
            Global.InputEvents.KeyDown -= InputEvents_KeyDown;
            Global.InputEvents.Scroll -= InputEvents_Scroll;
            base.Dispose();
        }

        protected override UserControl CreateControl() {
            return new Control_ToolbarLayer(this);
        }
        
        public override EffectLayer Render(IGameState _) {
            EffectLayer layer = new EffectLayer();
            foreach (var key in Properties.Sequence.keys)
                layer.Set(key, key == activeKey ? Properties.SecondaryColor : Properties.PrimaryColor);
            return layer;
        }

        /// <summary>
        /// Handler for when any keyboard button is pressed.
        /// </summary>
        private void InputEvents_KeyDown(object sender, SharpDX.RawInput.KeyboardInputEventArgs e) {
            if (Properties.Sequence.keys.Contains(e.GetDeviceKey()))
                activeKey = e.GetDeviceKey();
        }

        /// <summary>
        /// Handler for the ScrollWheel.
        /// </summary>
        private void InputEvents_Scroll(object sender, SharpDX.RawInput.MouseInputEventArgs e) {
            if (Properties.EnableScroll && Properties.Sequence.keys.Count > 1) {
                // If there's no active key or the ks doesn't contain it (e.g. the sequence was just changed), make the first one active.
                if (activeKey == DeviceKeys.NONE || !Properties.Sequence.keys.Contains(activeKey))
                    activeKey = Properties.Sequence.keys[0];

                // If there's an active key make scroll move up/down
                else {
                    // Target index is the current index +/- 1 depending on the scroll value
                    int idx = Properties.Sequence.keys.IndexOf(activeKey) + (e.WheelDelta > 0 ? -1 : 1);

                    // If scroll loop is enabled, allow the index to wrap around from start to end or end to start.
                    if (Properties.ScrollLoop) {
                        if (idx < 0) // If index is now negative (if first item selected and scrolling down), add the length to loop back
                            idx += Properties.Sequence.keys.Count;
                        idx = idx % Properties.Sequence.keys.Count;

                        // If scroll loop isn't enabled, cap the index so that it stops at either end
                    } else {
                        idx = Math.Max(Math.Min(idx, Properties.Sequence.keys.Count - 1), 0);
                    }

                    activeKey = Properties.Sequence.keys[idx];
                }
            }
        }
    }
}
