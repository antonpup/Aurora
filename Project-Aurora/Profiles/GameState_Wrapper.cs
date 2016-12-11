using Newtonsoft.Json.Linq;
using System;

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
        private byte[] _Bitmap;
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
        public byte[] Sent_Bitmap
        {
            get
            {
                if (_Bitmap == null)
                {
                    Newtonsoft.Json.Linq.JToken value;

                    if (_ParsedData.TryGetValue("bitmap", out value))
                        _Bitmap = value.ToObject<byte[]>();
                    else
                        _Bitmap = new byte[] { };
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
        [Range(0, 3)]
        public int[] peripheral;
        [Range(0, 3)]
        public int[] logo;
        [Range(0, 3)]
        public int[] badge;
        [Range(0, 3)]
        public int[] G1;
        [Range(0, 3)]
        public int[] G2;
        [Range(0, 3)]
        public int[] G3;
        [Range(0, 3)]
        public int[] G4;
        [Range(0, 3)]
        public int[] G5;
        [Range(0, 3)]
        public int[] G6;
        [Range(0, 3)]
        public int[] G7;
        [Range(0, 3)]
        public int[] G8;
        [Range(0, 3)]
        public int[] G9;
        [Range(0, 3)]
        public int[] G10;
        [Range(0, 3)]
        public int[] G11;
        [Range(0, 3)]
        public int[] G12;
        [Range(0, 3)]
        public int[] G13;
        [Range(0, 3)]
        public int[] G14;
        [Range(0, 3)]
        public int[] G15;
        [Range(0, 3)]
        public int[] G16;
        [Range(0, 3)]
        public int[] G17;
        [Range(0, 3)]
        public int[] G18;
        [Range(0, 3)]
        public int[] G19;
        [Range(0, 3)]
        public int[] G20;

        internal Extra_Keys_Wrapper(string JSON)
            : base(JSON)
        {
            peripheral = GetArray<int>("peripheral");
            logo = GetArray<int>("logo");
            badge = GetArray<int>("badge");
            G1 = GetArray<int>("G1");
            G2 = GetArray<int>("G2");
            G3 = GetArray<int>("G3");
            G4 = GetArray<int>("G4");
            G5 = GetArray<int>("G5");
            G6 = GetArray<int>("G6");
            G7 = GetArray<int>("G7");
            G8 = GetArray<int>("G8");
            G9 = GetArray<int>("G9");
            G10 = GetArray<int>("G10");
            G11 = GetArray<int>("G11");
            G12 = GetArray<int>("G12");
            G13 = GetArray<int>("G13");
            G14 = GetArray<int>("G14");
            G15 = GetArray<int>("G15");
            G16 = GetArray<int>("G16");
            G17 = GetArray<int>("G17");
            G18 = GetArray<int>("G18");
            G19 = GetArray<int>("G19");
            G20 = GetArray<int>("G20");
        }
    }

}
