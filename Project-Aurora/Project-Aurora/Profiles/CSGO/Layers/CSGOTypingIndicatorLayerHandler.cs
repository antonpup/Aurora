using Aurora.EffectsEngine;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Profiles.CSGO.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.CSGO.Layers
{
    public class CSGOTypingIndicatorLayerHandlerProperties : LayerHandlerProperties2Color<CSGOTypingIndicatorLayerHandlerProperties>
    {
        public Color? _TypingKeysColor { get; set; }

        [JsonIgnore]
        public Color TypingKeysColor { get { return Logic._TypingKeysColor ?? _TypingKeysColor ?? Color.Empty; } }

        public CSGOTypingIndicatorLayerHandlerProperties() : base() { }

        public CSGOTypingIndicatorLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            this._Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.TILDE, Devices.DeviceKeys.ONE, Devices.DeviceKeys.TWO, Devices.DeviceKeys.THREE, Devices.DeviceKeys.FOUR, Devices.DeviceKeys.FIVE, Devices.DeviceKeys.SIX, Devices.DeviceKeys.SEVEN, Devices.DeviceKeys.EIGHT, Devices.DeviceKeys.NINE, Devices.DeviceKeys.ZERO, Devices.DeviceKeys.MINUS, Devices.DeviceKeys.EQUALS, Devices.DeviceKeys.BACKSPACE,
                                                    Devices.DeviceKeys.TAB, Devices.DeviceKeys.Q, Devices.DeviceKeys.W, Devices.DeviceKeys.E, Devices.DeviceKeys.R, Devices.DeviceKeys.T, Devices.DeviceKeys.Y, Devices.DeviceKeys.U, Devices.DeviceKeys.I, Devices.DeviceKeys.O, Devices.DeviceKeys.P, Devices.DeviceKeys.CLOSE_BRACKET, Devices.DeviceKeys.OPEN_BRACKET, Devices.DeviceKeys.BACKSLASH,
                                                    Devices.DeviceKeys.CAPS_LOCK, Devices.DeviceKeys.A, Devices.DeviceKeys.S, Devices.DeviceKeys.D, Devices.DeviceKeys.F, Devices.DeviceKeys.G, Devices.DeviceKeys.H, Devices.DeviceKeys.J, Devices.DeviceKeys.K, Devices.DeviceKeys.L, Devices.DeviceKeys.SEMICOLON, Devices.DeviceKeys.APOSTROPHE, Devices.DeviceKeys.HASHTAG, Devices.DeviceKeys.ENTER,
                                                    Devices.DeviceKeys.LEFT_SHIFT, Devices.DeviceKeys.BACKSLASH_UK, Devices.DeviceKeys.Z, Devices.DeviceKeys.X, Devices.DeviceKeys.C, Devices.DeviceKeys.V, Devices.DeviceKeys.B, Devices.DeviceKeys.N, Devices.DeviceKeys.M, Devices.DeviceKeys.COMMA, Devices.DeviceKeys.PERIOD, Devices.DeviceKeys.FORWARD_SLASH, Devices.DeviceKeys.RIGHT_SHIFT,
                                                    Devices.DeviceKeys.LEFT_CONTROL, Devices.DeviceKeys.LEFT_WINDOWS, Devices.DeviceKeys.LEFT_ALT, Devices.DeviceKeys.SPACE, Devices.DeviceKeys.RIGHT_ALT, Devices.DeviceKeys.RIGHT_WINDOWS, Devices.DeviceKeys.APPLICATION_SELECT, Devices.DeviceKeys.RIGHT_CONTROL,
                                                    Devices.DeviceKeys.ARROW_UP, Devices.DeviceKeys.ARROW_LEFT, Devices.DeviceKeys.ARROW_DOWN, Devices.DeviceKeys.ARROW_RIGHT, Devices.DeviceKeys.ESC
                                                  });
            this._TypingKeysColor = Color.FromArgb(0, 255, 0);
        }
    }

    [Obsolete("This layer is obselete and has been replaced by the Overrides system.")]
    public class CSGOTypingIndicatorLayerHandler : LayerHandler<CSGOTypingIndicatorLayerHandlerProperties>
    {
        protected override UserControl CreateControl()
        {
            return new Control_CSGOTypingIndicatorLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer typing_keys_layer = new EffectLayer("CSGO - Typing Keys");

            if (state is GameState_CSGO)
            {
                GameState_CSGO csgostate = state as GameState_CSGO;

                //Update Typing Keys
                if (csgostate.Player.Activity == PlayerActivity.TextInput)
                    typing_keys_layer.Set(Properties.Sequence, Properties.TypingKeysColor);
            }

            return typing_keys_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_CSGOTypingIndicatorLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}