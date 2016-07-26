using System.ComponentModel;
using System.Drawing;

namespace Aurora.Profiles.GTA5.GSI
{
    public enum PlayerState
    {
        [Description("Undefined")]
        Undefined,
        [Description("Menu")]
        Menu,
        [Description("Singleplayer")]
        PlayingSP,
        [Description("Singleplayer - Trevor")]
        PlayingSP_Trevor, //004FAF
        [Description("Singleplayer - Michael")]
        PlayingSP_Michael,
        [Description("Singleplayer - Franklin")]
        PlayingSP_Franklin,
        [Description("Singleplayer - Chop")]
        PlayingSP_Chop,
        [Description("Multiplayer")]
        PlayingMP, //E24400
        [Description("Multiplayer - Mission")]
        PlayingMP_Mission,
        [Description("Multiplayer - Heist Finale")]
        PlayingMP_HeistFinale,
        [Description("Multiplayer - Spectator")]
        PlayingMP_Spectator,
        [Description("Race - Platinum")]
        PlayingRace_Platinum,
        [Description("Race - Gold")]
        PlayingRace_Gold,
        [Description("Race - Silver")]
        PlayingRace_Silver,
        [Description("Race - Bronze")]
        PlayingRace_Bronze,
    }

    public class GameState_GTA5 : GameState_Wrapper
    {
        private PlayerState _CurrentState;
        private bool _HasCops;
        private Color _StateColor;
        private Color _LeftSirenColor;
        private Color _RightSirenColor;


        public PlayerState CurrentState
        {
            get
            {
                return _CurrentState;
            }
        }

        public bool HasCops
        {
            get
            {
                return _HasCops;
            }
        }

        public Color StateColor
        {
            get
            {
                return _StateColor;
            }
        }

        public Color LeftSirenColor
        {
            get
            {
                return _LeftSirenColor;
            }
        }

        public Color RightSirenColor
        {
            get
            {
                return _RightSirenColor;
            }
        }


        public GameState_GTA5()
        {
            json = "{}";
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json);
        }

        public GameState_GTA5(string json_data) : base(json_data)
        {
            Provider.AppID = 271590;

            //Get Current State
            Color state_color = JSonToColor(
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.ESC + 3],
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.ESC + 2],
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.ESC + 1],
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.ESC]
                );

            if(state_color == Color.FromArgb(255, 175, 79, 0))
                _CurrentState = PlayerState.PlayingSP_Trevor;
            else if (state_color == Color.FromArgb(255, 48, 255, 255))
                _CurrentState = PlayerState.PlayingSP_Michael;
            else if (state_color == Color.FromArgb(255, 48, 255, 0))
                _CurrentState = PlayerState.PlayingSP_Franklin;
            else if (state_color == Color.FromArgb(255, 127, 0, 0))
                _CurrentState = PlayerState.PlayingSP_Chop;
            else if (state_color == Color.FromArgb(255, 0, 68, 226))
                _CurrentState = PlayerState.PlayingMP;
            else if (state_color == Color.FromArgb(255, 255, 170, 0))
                _CurrentState = PlayerState.PlayingRace_Gold;
            else if (state_color == Color.FromArgb(255, 191, 191, 191))
                _CurrentState = PlayerState.PlayingRace_Silver;
            else if (state_color == Color.FromArgb(255, 255, 51, 0))
                _CurrentState = PlayerState.PlayingRace_Bronze;
            else if (state_color == Color.FromArgb(255, 193, 79, 79))
                _CurrentState = PlayerState.PlayingMP_Mission;
            else if (state_color == Color.FromArgb(255, 255, 122, 196))
                _CurrentState = PlayerState.PlayingMP_HeistFinale;
            else if (state_color == Color.FromArgb(255, 142, 127, 153))
                _CurrentState = PlayerState.PlayingMP_Spectator;
            else
            {
                _CurrentState = PlayerState.Undefined;
                Global.logger.LogLine("Undefined color - " + state_color);
            }

            _StateColor = state_color;

            _LeftSirenColor = JSonToColor(
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F1 + 3],
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F1 + 2],
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F1 + 1],
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F1]
                );

            _RightSirenColor = JSonToColor(
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F12 + 3],
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F12 + 2],
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F12 + 1],
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F12]
                );


            _HasCops = _LeftSirenColor != _RightSirenColor;
        }

        public GameState_GTA5(GameState other_state) : base(other_state)
        {
        }

        private Color JSonToColor(byte a, byte r, byte g, byte b)
        {
            return Color.FromArgb(a, r, g, b);
        }
    }
}
