using Aurora.Devices.Layout.Layouts;
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

            this._Sequence = new KeySequence(new KeyboardKeys[] { KeyboardKeys.TILDE, KeyboardKeys.ONE, KeyboardKeys.TWO, KeyboardKeys.THREE, KeyboardKeys.FOUR, KeyboardKeys.FIVE, KeyboardKeys.SIX, KeyboardKeys.SEVEN, KeyboardKeys.EIGHT, KeyboardKeys.NINE, KeyboardKeys.ZERO, KeyboardKeys.MINUS, KeyboardKeys.EQUALS, KeyboardKeys.BACKSPACE,
                                                    KeyboardKeys.TAB, KeyboardKeys.Q, KeyboardKeys.W, KeyboardKeys.E, KeyboardKeys.R, KeyboardKeys.T, KeyboardKeys.Y, KeyboardKeys.U, KeyboardKeys.I, KeyboardKeys.O, KeyboardKeys.P, KeyboardKeys.CLOSE_BRACKET, KeyboardKeys.OPEN_BRACKET, KeyboardKeys.BACKSLASH,
                                                    KeyboardKeys.CAPS_LOCK, KeyboardKeys.A, KeyboardKeys.S, KeyboardKeys.D, KeyboardKeys.F, KeyboardKeys.G, KeyboardKeys.H, KeyboardKeys.J, KeyboardKeys.K, KeyboardKeys.L, KeyboardKeys.SEMICOLON, KeyboardKeys.APOSTROPHE, KeyboardKeys.HASH, KeyboardKeys.ENTER,
                                                    KeyboardKeys.LEFT_SHIFT, KeyboardKeys.BACKSLASH_UK, KeyboardKeys.Z, KeyboardKeys.X, KeyboardKeys.C, KeyboardKeys.V, KeyboardKeys.B, KeyboardKeys.N, KeyboardKeys.M, KeyboardKeys.COMMA, KeyboardKeys.PERIOD, KeyboardKeys.FORWARD_SLASH, KeyboardKeys.RIGHT_SHIFT,
                                                    KeyboardKeys.LEFT_CONTROL, KeyboardKeys.LEFT_WINDOWS, KeyboardKeys.LEFT_ALT, KeyboardKeys.SPACE, KeyboardKeys.RIGHT_ALT, KeyboardKeys.RIGHT_WINDOWS, KeyboardKeys.APPLICATION_SELECT, KeyboardKeys.RIGHT_CONTROL,
                                                    KeyboardKeys.ARROW_UP, KeyboardKeys.ARROW_LEFT, KeyboardKeys.ARROW_DOWN, KeyboardKeys.ARROW_RIGHT, KeyboardKeys.ESC
                                                  });
            this._TypingKeysColor = Color.FromArgb(0, 255, 0);
        }
    }

    public class CSGOTypingIndicatorLayerHandler : LayerHandler<CSGOTypingIndicatorLayerHandlerProperties>
    {
        public CSGOTypingIndicatorLayerHandler() : base()
        {
            _ID = "CSGOTyping";
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
    }
}