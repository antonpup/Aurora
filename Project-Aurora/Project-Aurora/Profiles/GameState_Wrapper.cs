using Newtonsoft.Json.Linq;
using System;
using System.Drawing;

namespace Aurora.Profiles
{
    /// <summary>
    /// A class representing various lighting information retaining to the wrapper.
    /// </summary>
    public class GameState_Wrapper : GameState
    {
        private Provider_Wrapper _Provider;
        private string _Command;
        private Command_Wrapper _Command_Data;
        private int[] _Bitmap;
        private Extra_Keys_Wrapper _Extra_Keys;

        /// <summary>
        /// Information about the provider of this GameState
        /// </summary>
        [GameStateIgnoreAttribute]
        public Provider_Wrapper Provider
        {
            get
            {
                if (_Provider == null)
                {
                    _Provider = new Provider_Wrapper(_ParsedData["provider"]?.ToString() ?? "");
                }

                return _Provider;
            }
        }

        /// <summary>
        /// The sent wrapper command
        /// </summary>
        [GameStateIgnoreAttribute]
        public string Command
        {
            get
            {
                if (_Command == null)
                {
                    Newtonsoft.Json.Linq.JToken value;

                    if (_ParsedData.TryGetValue("command", out value))
                        _Command = value.ToString();
                    else
                        _Command = "";
                }

                return _Command;
            }
        }

        /// <summary>
        /// Data related to the passed command
        /// </summary>
        [GameStateIgnoreAttribute]
        public Command_Wrapper Command_Data
        {
            get
            {
                if (_Command_Data == null)
                {
                    _Command_Data = new Command_Wrapper(_ParsedData["command_data"]?.ToString() ?? "");
                }

                return _Command_Data;
            }
        }

        /// <summary>
        /// The bitmap sent from the wrapper
        /// </summary>
        [GameStateIgnoreAttribute]
        public int[] Sent_Bitmap
        {
            get
            {
                if (_Bitmap == null)
                {
                    Newtonsoft.Json.Linq.JToken value;

                    if (_ParsedData.TryGetValue("bitmap", out value))
                        _Bitmap = value.ToObject<int[]>();
                    else
                        _Bitmap = new int[] { };
                }

                return _Bitmap;
            }
        }

        /// <summary>
        /// Lighting information for extra keys that are not part of the bitmap
        /// </summary>
        [GameStateIgnoreAttribute]
        public Extra_Keys_Wrapper Extra_Keys
        {
            get
            {
                if (_Extra_Keys == null)
                {
                    _Extra_Keys = new Extra_Keys_Wrapper(_ParsedData["extra_keys"]?.ToString() ?? "");
                }

                return _Extra_Keys;
            }
        }

        /// <summary>
        /// Creates a default GameState_Wrapper instance.
        /// </summary>
        public GameState_Wrapper()
        {
            json = "{}";
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json);
        }

        /// <summary>
        /// Creates a GameState_Wrapper instance based on the passed json data.
        /// </summary>
        /// <param name="json_data">The passed json data</param>
        public GameState_Wrapper(string json_data) : base(json_data)
        {
            if (String.IsNullOrWhiteSpace(json_data))
                json_data = "{}";

            json = json_data;
            _ParsedData = JObject.Parse(json_data);
        }

        /// <summary>
        /// A copy constructor, creates a GameState_Wrapper instance based on the data from the passed GameState instance.
        /// </summary>
        /// <param name="other_state">The passed GameState</param>
        public GameState_Wrapper(GameState other_state) : base(other_state)
        {
        }
    }

    /// <summary>
    /// Class representing provider information for the wrapper
    /// </summary>
    public class Provider_Wrapper : Node<Provider_Wrapper>
    {
        /// <summary>
        /// Name of the program
        /// </summary>
        public string Name;

        /// <summary>
        /// AppID of the program (for wrappers, always 0)
        /// </summary>
        public int AppID;

        internal Provider_Wrapper(string JSON)
            : base(JSON)
        {
            Name = GetString("name");
            AppID = GetInt("appid");
        }
    }

    /// <summary>
    /// Class for additional wrapper command data such as effects and colors
    /// </summary>
    public class Command_Wrapper : Node<Command_Wrapper>
    {
        public int red_start;
        public int green_start;
        public int blue_start;
        public int red_end;
        public int green_end;
        public int blue_end;
        public int duration;
        public int interval;
        public string effect_type;
        public string effect_config;
        public int key;
        public int custom_mode;

        internal Command_Wrapper(string JSON)
            : base(JSON)
        {
            red_start = GetInt("red_start");
            green_start = GetInt("green_start");
            blue_start = GetInt("blue_start");
            red_end = GetInt("red_end");
            green_end = GetInt("green_end");
            blue_end = GetInt("blue_end");
            duration = GetInt("duration");
            interval = GetInt("interval");
            effect_type = GetString("effect_type");
            effect_config = GetString("effect_config");
            key = GetInt("key");
            custom_mode = GetInt("custom_mode");
        }
    }

    /// <summary>
    /// Class for additional wrapper keys
    /// </summary>
    public class Extra_Keys_Wrapper : Node<Extra_Keys_Wrapper>
    {
        public Color peripheral;
        public Color logo;
        public Color mousepad1;
        public Color mousepad2;
        public Color mousepad3;
        public Color mousepad4;
        public Color mousepad5;
        public Color mousepad6;
        public Color mousepad7;
        public Color mousepad8;
        public Color mousepad9;
        public Color mousepad10;
        public Color mousepad11;
        public Color mousepad12;
        public Color mousepad13;
        public Color mousepad14;
        public Color mousepad15;
        public Color badge;
        public Color G1;
        public Color G2;
        public Color G3;
        public Color G4;
        public Color G5;
        public Color G6;
        public Color G7;
        public Color G8;
        public Color G9;
        public Color G10;
        public Color G11;
        public Color G12;
        public Color G13;
        public Color G14;
        public Color G15;
        public Color G16;
        public Color G17;
        public Color G18;
        public Color G19;
        public Color G20;

        internal Extra_Keys_Wrapper(string JSON)
            : base(JSON)
        {
            peripheral = Utils.ColorUtils.GetColorFromInt(GetInt("peripheral"));
            logo = Utils.ColorUtils.GetColorFromInt( GetInt("logo"));
            mousepad1 = Utils.ColorUtils.GetColorFromInt(GetInt("mousepad0"));
            mousepad2 = Utils.ColorUtils.GetColorFromInt(GetInt("mousepad1"));
            mousepad3 = Utils.ColorUtils.GetColorFromInt(GetInt("mousepad2"));
            mousepad4 = Utils.ColorUtils.GetColorFromInt(GetInt("mousepad3"));
            mousepad5 = Utils.ColorUtils.GetColorFromInt(GetInt("mousepad4"));
            mousepad6 = Utils.ColorUtils.GetColorFromInt(GetInt("mousepad5"));
            mousepad7 = Utils.ColorUtils.GetColorFromInt(GetInt("mousepad6"));
            mousepad8 = Utils.ColorUtils.GetColorFromInt(GetInt("mousepad7"));
            mousepad9 = Utils.ColorUtils.GetColorFromInt(GetInt("mousepad8"));
            mousepad10 = Utils.ColorUtils.GetColorFromInt(GetInt("mousepad9"));
            mousepad11 = Utils.ColorUtils.GetColorFromInt(GetInt("mousepad10"));
            mousepad12 = Utils.ColorUtils.GetColorFromInt(GetInt("mousepad11"));
            mousepad13 = Utils.ColorUtils.GetColorFromInt(GetInt("mousepad12"));
            mousepad14 = Utils.ColorUtils.GetColorFromInt(GetInt("mousepad13"));
            mousepad15 = Utils.ColorUtils.GetColorFromInt(GetInt("mousepad14"));
            badge = Utils.ColorUtils.GetColorFromInt( GetInt("badge"));
            G1 = Utils.ColorUtils.GetColorFromInt( GetInt("G1"));
            G2 = Utils.ColorUtils.GetColorFromInt( GetInt("G2"));
            G3 = Utils.ColorUtils.GetColorFromInt( GetInt("G3"));
            G4 = Utils.ColorUtils.GetColorFromInt( GetInt("G4"));
            G5 = Utils.ColorUtils.GetColorFromInt( GetInt("G5"));
            G6 = Utils.ColorUtils.GetColorFromInt( GetInt("G6"));
            G7 = Utils.ColorUtils.GetColorFromInt( GetInt("G7"));
            G8 = Utils.ColorUtils.GetColorFromInt( GetInt("G8"));
            G9 = Utils.ColorUtils.GetColorFromInt( GetInt("G9"));
            G10 = Utils.ColorUtils.GetColorFromInt( GetInt("G10"));
            G11 = Utils.ColorUtils.GetColorFromInt( GetInt("G11"));
            G12 = Utils.ColorUtils.GetColorFromInt( GetInt("G12"));
            G13 = Utils.ColorUtils.GetColorFromInt( GetInt("G13"));
            G14 = Utils.ColorUtils.GetColorFromInt( GetInt("G14"));
            G15 = Utils.ColorUtils.GetColorFromInt( GetInt("G15"));
            G16 = Utils.ColorUtils.GetColorFromInt( GetInt("G16"));
            G17 = Utils.ColorUtils.GetColorFromInt( GetInt("G17"));
            G18 = Utils.ColorUtils.GetColorFromInt( GetInt("G18"));
            G19 = Utils.ColorUtils.GetColorFromInt( GetInt("G19"));
            G20 = Utils.ColorUtils.GetColorFromInt( GetInt("G20"));
        }
    }

}
