using System;
using System.Collections.Generic;
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
        /// Player is in a multiplayer lobby
        /// </summary>
        [Description("Multiplayer - Lobby")]
        PlayingMP_Lobby,

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
        public Color? StateColor = null;


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
        public GameState_GTA5() { }

        private byte RoundTo5(byte no)
        {
            return (byte)(Math.Round((double)no / 5) * 5);
        }

        /// <summary>
        /// Creates a GameState_GTA5 instance based on the passed json data.
        /// </summary>
        /// <param name="json_data">The passed json data</param>
        public GameState_GTA5(string json_data) : base(json_data)
        {
            Provider.AppID = 271590;


            //Get Current State
            Color state_color = Utils.ColorUtils.GetColorFromInt(Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.ESC / 4]);

            if(stateColors.TryGetValue(state_color, out var newState))
            {
               // Global.logger.Info("Set game state to " + newState);
                CurrentState = newState;
            }
            else
            {
                Global.logger.Debug("Undefined color - " + state_color);
                CurrentState = PlayerState.Undefined;
                StateColor = state_color;
            }


            LeftSirenColor = Utils.ColorUtils.GetColorFromInt(Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F1 / 4]);

            RightSirenColor = Utils.ColorUtils.GetColorFromInt(Sent_Bitmap[(int)Devices.Logitech.Logitech_keyboardBitmapKeys.F12 / 4]);

            HasCops = LeftSirenColor != RightSirenColor;
        }

        private Color JSonToColor(byte a, byte r, byte g, byte b)
        {
            return Color.FromArgb(a, r, g, b);
        }

        private static readonly Dictionary<Color, PlayerState> stateColors = new Dictionary<Color, PlayerState>
        {
            { Color.FromArgb(255, 255, 255), PlayerState.Menu                  },
            { Color.FromArgb(176, 80 , 0  ), PlayerState.PlayingSP_Trevor      },
            { Color.FromArgb(48 , 255, 255), PlayerState.PlayingSP_Michael     },
            { Color.FromArgb(48 , 255, 0  ), PlayerState.PlayingSP_Franklin    },
            { Color.FromArgb(127, 0  , 0  ), PlayerState.PlayingSP_Chop        },
            { Color.FromArgb(106, 191, 212), PlayerState.PlayingMP_Lobby       },
            { Color.FromArgb(194, 80 , 80 ), PlayerState.PlayingMP_Mission     },
            { Color.FromArgb(255, 123, 196), PlayerState.PlayingMP_HeistFinale },
            { Color.FromArgb(0  , 0  , 0  ), PlayerState.PlayingMP_Spectator   },//not sure what to do with this one, but full black is sent from the game sometimes
            { Color.FromArgb(255, 170, 0  ), PlayerState.PlayingRace_Gold      },
            { Color.FromArgb(192, 192, 192), PlayerState.PlayingRace_Silver    },
            { Color.FromArgb(255, 50 , 0  ), PlayerState.PlayingRace_Bronze    }
        };
    }
}
