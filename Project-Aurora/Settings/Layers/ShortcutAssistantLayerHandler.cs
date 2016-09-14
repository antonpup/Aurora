using Aurora.EffectsEngine;
using Aurora.Profiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aurora.Settings.Layers
{
    public class ShortcutAssistantLayerHandler : LayerHandler
    {
        public bool DimBackground = true;
        public Color CtrlKeyColor = Color.Red;
        public KeySequence CtrlKeySequence = new KeySequence(new Devices.DeviceKeys[] {
                    Devices.DeviceKeys.C, Devices.DeviceKeys.V, Devices.DeviceKeys.X, Devices.DeviceKeys.Y,
                    Devices.DeviceKeys.LEFT_ALT, Devices.DeviceKeys.RIGHT_ALT, Devices.DeviceKeys.A, Devices.DeviceKeys.Z
            });
        public Color WindowsKeyColor = Color.Blue;
        public KeySequence WindowsKeySequence = new KeySequence(new Devices.DeviceKeys[] {
                    Devices.DeviceKeys.R, Devices.DeviceKeys.E, Devices.DeviceKeys.M, Devices.DeviceKeys.D,
                    Devices.DeviceKeys.ARROW_UP, Devices.DeviceKeys.ARROW_DOWN, Devices.DeviceKeys.ARROW_LEFT, Devices.DeviceKeys.ARROW_RIGHT,
                    Devices.DeviceKeys.TAB
            });
        public Color AltKeyColor = Color.Yellow;
        public KeySequence AltKeySequence = new KeySequence(new Devices.DeviceKeys[] {
                    Devices.DeviceKeys.F4, Devices.DeviceKeys.E, Devices.DeviceKeys.V, Devices.DeviceKeys.LEFT_CONTROL,
                    Devices.DeviceKeys.RIGHT_CONTROL, Devices.DeviceKeys.TAB
            });


        public ShortcutAssistantLayerHandler()
        {
            _Control = new Control_ShortcutAssistantLayer(this);

            _Type = LayerType.ShortcutAssistant;
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer sc_assistant_layer = new EffectLayer("Shortcut Assistant");
            if (Global.held_modified == Keys.LControlKey || Global.held_modified == Keys.RControlKey)
            {
                if (DimBackground)
                    sc_assistant_layer.Fill(Color.FromArgb(169, 0, 0, 0));

                if (Global.held_modified == Keys.LControlKey)
                    sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_CONTROL, CtrlKeyColor);
                else
                    sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_CONTROL, CtrlKeyColor);
                sc_assistant_layer.Set(CtrlKeySequence, CtrlKeyColor);
            }
            else if (Global.held_modified == Keys.LMenu || Global.held_modified == Keys.RMenu)
            {
                if (DimBackground)
                    sc_assistant_layer.Fill(Color.FromArgb(169, 0, 0, 0));

                if (Global.held_modified == Keys.LMenu)
                    sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_ALT, AltKeyColor);
                else
                    sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_ALT, AltKeyColor);
                sc_assistant_layer.Set(AltKeySequence, AltKeyColor);
            }
            else if (Global.held_modified == Keys.LWin || Global.held_modified == Keys.RWin)
            {
                if (DimBackground)
                    sc_assistant_layer.Fill(Color.FromArgb(169, 0, 0, 0));

                if (Global.held_modified == Keys.LWin)
                    sc_assistant_layer.Set(Devices.DeviceKeys.LEFT_WINDOWS, WindowsKeyColor);
                else
                    sc_assistant_layer.Set(Devices.DeviceKeys.RIGHT_WINDOWS, WindowsKeyColor);
                sc_assistant_layer.Set(WindowsKeySequence, WindowsKeyColor);
            }


            return sc_assistant_layer;
        }
    }
}
