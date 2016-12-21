using System.ComponentModel;
using System.Drawing;

namespace Aurora.Profiles.GTA5.GSI
{
    /// <summary>
    /// Enum of various player states
    /// </summary>
    public enum PlayerState
    {
        /// <summary>
        /// Undefined
        /// </summary>
        [Description("Undefined")]
        Undefined,

        /// <summary>
        /// Player is in a menu
        /// </summary>
        [Description("Menu")]
        Menu,

        /// <summary>
        /// Player is playing as Trevor
        /// </summary>
        [Description("Singleplayer - Trevor")]
        PlayingSP_Trevor,

        /// <summary>
        /// Player is playing as Michael
        /// </summary>
        [Description("Singleplayer - Michael")]
        PlayingSP_Michael,

        /// <summary>
        /// Player is playing as Franklins
        /// </summary>
        [Description("Singleplayer - Franklin")]
        PlayingSP_Franklin,

        /// <summary>
        /// Player is playing as Chop
        /// </summary>
        [Description("Singleplayer - Chop")]
        PlayingSP_Chop,

        /// <summary>
        /// Player is playing multiplayer
        /// </summary>
        [Description("Multiplayer")]
        PlayingMP,

        /// <summary>
        /// Player is playing a multiplayer mission
        /// </summary>
        [Description("Multiplayer - Mission")]
        PlayingMP_Mission,

        /// <summary>
        /// Player is playing a multiplayer heist finale
        /// </summary>
        [Description("Multiplayer - Heist Finale")]
        PlayingMP_HeistFinale,

        /// <summary>
        /// Player is spectating a multiplayer game
        /// </summary>
        [Description("Multiplayer - Spectator")]
        PlayingMP_Spectator,

        /// <summary>
        /// Player is in a race, in first position
        /// </summary>
        [Description("Race - Platinum")]
        PlayingRace_Platinum,

        /// <summary>
        /// Player is in a race, in second position
        /// </summary>
        [Description("Race - Gold")]
        PlayingRace_Gold,

        /// <summary>
        /// Player is in a race, in third position
        /// </summary>
        [Description("Race - Silver")]
        PlayingRace_Silver,

        /// <summary>
        /// Player is in a race, in fourth or lower position
        /// </summary>
        [Description("Race - Bronze")]
        PlayingRace_Bronze,
    }

    /// <summary>
    /// A class representing various information relating to Grand Theft Auto 5
    /// </summary>
    public class GameState_GTA5 : GameState_Wrapper
    {

        /// <summary>
        /// Current game state
        /// </summary>
        public PlayerState CurrentState;

        /// <summary>
        /// A boolean representing if the player is wanted
        /// </summary>
        public bool HasCops;

        /// <summary>
        /// The current background color
        /// </summary>
        public Color StateColor;


        /// <summary>
        /// The current left siren color (Keys F1 - F6)
        /// </summary>
        public Color LeftSirenColor;

        /// <summary>
        /// The current left siren color (Keys F7 - F12)
        /// </summary>
        public Color RightSirenColor;

        /// <summary>
        /// Creates a default GameState_GTA5 instance.
        /// </summary>
        public GameState_GTA5()
        {
            json = "{}";
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json);
        }

        /// <summary>
        /// Creates a GameState_GTA5 instance based on the passed json data.
        /// </summary>
        /// <param name="json_data">The passed json data</param>
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

            if(state_color == Color.FromArgb(255, 175, 79, 0) || state_color == Color.FromArgb(255, 176, 80, 0))
                CurrentState = PlayerState.PlayingSP_Trevor;
            else if (state_color == Color.FromArgb(255, 48, 255, 255))
                CurrentState = PlayerState.PlayingSP_Michael;
            else if (state_color == Color.FromArgb(255, 48, 255, 0))
                CurrentState = PlayerState.PlayingSP_Franklin;
            else if (state_color == Color.FromArgb(255, 127, 0, 0))
                CurrentState = PlayerState.PlayingSP_Chop;
            else if (state_color == Color.FromArgb(255, 0, 68, 226) || state_color == Color.FromArgb(255, 0, 70, 228))
                CurrentState = PlayerState.PlayingMP;
            else if (state_color == Color.FromArgb(255, 255, 170, 0))
                CurrentState = PlayerState.PlayingRace_Gold;
            else if (state_color == Color.FromArgb(255, 191, 191, 191))
                CurrentState = PlayerState.PlayingRace_Silver;
            else if (state_color == Color.FromArgb(255, 255, 51, 0))
                CurrentState = PlayerState.PlayingRace_Bronze;
            else if (state_color == Color.FromArgb(255, 193, 79, 79))
                CurrentState = PlayerState.PlayingMP_Mission;
            else if (state_color == Color.FromArgb(255, 255, 122, 196) || state_color == Color.FromArgb(255, 156, 110, 175))
                CurrentState = PlayerState.PlayingMP_HeistFinale;
            else if (state_color == Color.FromArgb(255, 142, 127, 153))
                CurrentState = PlayerState.PlayingMP_Spectator;
            else
            {
                CurrentState = PlayerState.Undefined;
                Global.logger.LogLine("Undefined color - " + state_color);
            }

            StateColor = state_color;

            LeftSirenColor = JSonToColor(
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F1 + 3],
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F1 + 2],
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F1 + 1],
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F1]
                );

            RightSirenColor = JSonToColor(
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F12 + 3],
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F12 + 2],
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F12 + 1],
                Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F12]
                );


            HasCops = LeftSirenColor != RightSirenColor;
        }

        /// <summary>
        /// A copy constructor, creates a GameState_GTA5 instance based on the data from the passed GameState instance.
        /// </summary>
        /// <param name="other_state">The passed GameState</param>
        public GameState_GTA5(GameState other_state) : base(other_state)
        {
        }

        private Color JSonToColor(byte a, byte r, byte g, byte b)
        {
            return Color.FromArgb(a, r, g, b);
        }
    }
}
