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
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public class ShortcutAssistantLayerHandlerProperties : LayerHandlerProperties<ShortcutAssistantLayerHandlerProperties>
    {
        public bool? _DimBackground { get; set; }

        [JsonIgnore]
        public bool DimBackground { get { return (Logic._DimBackground ?? _DimBackground) ?? false; } }

        public Color? _DimColor { get; set; }

        [JsonIgnore]
        public Color DimColor { get { return (Logic._DimColor ?? _DimColor) ?? Color.Empty; } }

        public Keys? _HeldKey { get; set; }

        [JsonIgnore]
        public Keys HeldKey { get { return Logic._HeldKey ?? _HeldKey ?? Keys.None;  } }

        /*public Color? _WindowsKeyColor { get; set; }

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
        public KeySequence AltKeySequence { get { return Logic._AltKeySequence ?? _AltKeySequence; } }*/

        public ShortcutAssistantLayerHandlerProperties() : base() { }

        public ShortcutAssistantLayerHandlerProperties(bool empty = false) : base(empty) { }

        public ShortcutAssistantLayerHandlerProperties(System.Windows.Forms.Keys heldKey, bool empty = false) : this(empty)
        {
            this._HeldKey = heldKey;
            this.SetDefaultKeys();
        }

        public override void Default()
        {
            base.Default();
            _DimBackground = true;
            _DimColor = Color.FromArgb(169, 0, 0, 0);
            _HeldKey = Keys.LControlKey;
            this.SetDefaultKeys();
            /*_WindowsKeyColor = Color.Blue;
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
            });*/
        }

        [JsonIgnore]
        public Dictionary<Keys, KeySequence> DefaultKeys = new Dictionary<System.Windows.Forms.Keys, KeySequence>
        {
            { Keys.LControlKey, new KeySequence(new Devices.DeviceKeys[] {
                    Devices.DeviceKeys.C, Devices.DeviceKeys.V, Devices.DeviceKeys.X, Devices.DeviceKeys.Y,
                    Devices.DeviceKeys.LEFT_ALT, Devices.DeviceKeys.RIGHT_ALT, Devices.DeviceKeys.A, Devices.DeviceKeys.Z
            }) },
            { Keys.LWin, new KeySequence(new Devices.DeviceKeys[] {
                    Devices.DeviceKeys.R, Devices.DeviceKeys.E, Devices.DeviceKeys.M, Devices.DeviceKeys.D,
                    Devices.DeviceKeys.ARROW_UP, Devices.DeviceKeys.ARROW_DOWN, Devices.DeviceKeys.ARROW_LEFT, Devices.DeviceKeys.ARROW_RIGHT,
                    Devices.DeviceKeys.TAB
            }) },
            { Keys.LMenu, new KeySequence(new Devices.DeviceKeys[] {
                    Devices.DeviceKeys.F4, Devices.DeviceKeys.E, Devices.DeviceKeys.V, Devices.DeviceKeys.LEFT_CONTROL,
                    Devices.DeviceKeys.RIGHT_CONTROL, Devices.DeviceKeys.TAB
            }) }
        };

        [JsonIgnore]
        public Dictionary<System.Windows.Forms.Keys, Color> DefaultColours = new Dictionary<Keys, Color> {
            {Keys.LControlKey, Color.Red },
            {Keys.LMenu, Color.Yellow},
            {Keys.LWin, Color.Blue }
        };

        internal void SetDefaultKeys()
        {
            Keys key = _HeldKey ?? Keys.None;
            if (DefaultKeys.ContainsKey(key))
                this._Sequence = new KeySequence(this.DefaultKeys[key]);

            if (DefaultColours.ContainsKey(key))
                this._PrimaryColor = DefaultColours[key];
        }
    }

    public class ShortcutAssistantLayerHandler : LayerHandler<ShortcutAssistantLayerHandlerProperties>
    {
        public ShortcutAssistantLayerHandler()
        {
            _ID = "ShortcutAssistant";
        }

        protected override System.Windows.Controls.UserControl CreateControl()
        {
            return new Control_ShortcutAssistantLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer sc_assistant_layer = new EffectLayer("Shortcut Assistant");
            if (Global.held_modified.HasFlag(Properties.HeldKey))
            {
                if (Properties.DimBackground)
                    sc_assistant_layer.Fill(Properties.DimColor);

                sc_assistant_layer.Set(Utils.KeyUtils.GetDeviceKey(Global.held_modified), Properties.PrimaryColor);
                sc_assistant_layer.Set(Properties.Sequence, Properties.PrimaryColor);
            }
            /*else if (Global.held_modified == Keys.LMenu || Global.held_modified == Keys.RMenu)
            {
                if (Properties.DimBackground)
                    sc_assistant_layer.Fill(Properties.DimColor);

                if (Global.held_modified == Keys.LMenu)
                    sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_ALT, Properties.AltKeyColor);
                else
                    sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_ALT, Properties.AltKeyColor);
                sc_assistant_layer.Set(Properties.AltKeySequence, Properties.AltKeyColor);
            }
            else if (Global.held_modified == Keys.LWin || Global.held_modified == Keys.RWin)
            {
                if (Properties.DimBackground)
                    sc_assistant_layer.Fill(Properties.DimColor);

                if (Global.held_modified == Keys.LWin)
                    sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_WINDOWS, Properties.WindowsKeyColor);
                else
                    sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_WINDOWS, Properties.WindowsKeyColor);
                sc_assistant_layer.Set(Properties.WindowsKeySequence, Properties.WindowsKeyColor);
            }*/

            
            return sc_assistant_layer;
        }
    }
}
