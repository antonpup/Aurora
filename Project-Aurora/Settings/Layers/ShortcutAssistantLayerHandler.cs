using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aurora.Settings.Layers
{
    public class ShortcutAssistantLayerHandlerProperties : LayerHandlerProperties<ShortcutAssistantLayerHandlerProperties>
    {
        public bool? _DimBackground { get; set; }

        [JsonIgnore]
        public bool DimBackground { get { return (Logic._DimBackground ?? _DimBackground) ?? false; } }

        public Color? _CtrlKeyColor { get; set; }

        [JsonIgnore]
        public Color CtrlKeyColor { get { return (Logic._CtrlKeyColor ?? _CtrlKeyColor) ?? Color.Empty; } }

        public KeySequence _CtrlKeySequence { get; set; }

        [JsonIgnore]
        public KeySequence CtrlKeySequence { get { return Logic._CtrlKeySequence ?? _CtrlKeySequence; } }

        public Color? _WindowsKeyColor { get; set; }

        [JsonIgnore]
        public Color WindowsKeyColor { get { return Logic._WindowsKeyColor ?? _WindowsKeyColor ?? Color.Empty; } }

        public KeySequence _WindowsKeySequence { get; set; }

        [JsonIgnore]
        public KeySequence WindowsKeySequence { get { return Logic._WindowsKeySequence ?? _WindowsKeySequence; } }

        public Color? _AltKeyColor { get; set; }

        [JsonIgnore]
        public Color AltKeyColor { get { return (Logic._AltKeyColor ?? _AltKeyColor) ?? Color.Empty; } }

        public KeySequence _AltKeySequence { get; set; }

        [JsonIgnore]
        public KeySequence AltKeySequence { get { return Logic._AltKeySequence ?? _AltKeySequence; } }

        public ShortcutAssistantLayerHandlerProperties() : base() { }

        public ShortcutAssistantLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            _DimBackground = true;
            _CtrlKeyColor = Color.Red;
            _WindowsKeyColor = Color.Blue;
            _AltKeyColor = Color.Yellow;
            _CtrlKeySequence = new KeySequence(new Devices.DeviceKeys[] {
                    Devices.DeviceKeys.C, Devices.DeviceKeys.V, Devices.DeviceKeys.X, Devices.DeviceKeys.Y,
                    Devices.DeviceKeys.LEFT_ALT, Devices.DeviceKeys.RIGHT_ALT, Devices.DeviceKeys.A, Devices.DeviceKeys.Z
            });
            _WindowsKeySequence = new KeySequence(new Devices.DeviceKeys[] {
                    Devices.DeviceKeys.R, Devices.DeviceKeys.E, Devices.DeviceKeys.M, Devices.DeviceKeys.D,
                    Devices.DeviceKeys.ARROW_UP, Devices.DeviceKeys.ARROW_DOWN, Devices.DeviceKeys.ARROW_LEFT, Devices.DeviceKeys.ARROW_RIGHT,
                    Devices.DeviceKeys.TAB
            });
            _AltKeySequence = new KeySequence(new Devices.DeviceKeys[] {
                    Devices.DeviceKeys.F4, Devices.DeviceKeys.E, Devices.DeviceKeys.V, Devices.DeviceKeys.LEFT_CONTROL,
                    Devices.DeviceKeys.RIGHT_CONTROL, Devices.DeviceKeys.TAB
            });
        }
    }

    public class ShortcutAssistantLayerHandler : LayerHandler<ShortcutAssistantLayerHandlerProperties>
    {
        public ShortcutAssistantLayerHandler()
        {
            _Control = new Control_ShortcutAssistantLayer(this);

            _Type = LayerType.ShortcutAssistant;
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            Color DimColor = Color.FromArgb(169, 0, 0, 0); // Color.FromArgb((int)(byte.MaxValue / 2) + 1, 0, 0, 0);

            EffectLayer sc_assistant_layer = new EffectLayer("Shortcut Assistant");
            if (Global.held_modified == Keys.LControlKey || Global.held_modified == Keys.RControlKey)
            {
                if (Properties.DimBackground)
                    sc_assistant_layer.Fill(DimColor);

                if (Global.held_modified == Keys.LControlKey)
                    sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_CONTROL, Properties.CtrlKeyColor);
                else
                    sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_CONTROL, Properties.CtrlKeyColor);
                sc_assistant_layer.Set(Properties.CtrlKeySequence, Properties.CtrlKeyColor);
            }
            else if (Global.held_modified == Keys.LMenu || Global.held_modified == Keys.RMenu)
            {
                if (Properties.DimBackground)
                    sc_assistant_layer.Fill(DimColor);

                if (Global.held_modified == Keys.LMenu)
                    sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_ALT, Properties.AltKeyColor);
                else
                    sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_ALT, Properties.AltKeyColor);
                sc_assistant_layer.Set(Properties.AltKeySequence, Properties.AltKeyColor);
            }
            else if (Global.held_modified == Keys.LWin || Global.held_modified == Keys.RWin)
            {
                if (Properties.DimBackground)
                    sc_assistant_layer.Fill(DimColor);

                if (Global.held_modified == Keys.LWin)
                    sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_WINDOWS, Properties.WindowsKeyColor);
                else
                    sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_WINDOWS, Properties.WindowsKeyColor);
                sc_assistant_layer.Set(Properties.WindowsKeySequence, Properties.WindowsKeyColor);
            }

            
            return sc_assistant_layer;
        }
    }
}
