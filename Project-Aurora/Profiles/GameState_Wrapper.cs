using Newtonsoft.Json.Linq;
using System;

namespace Aurora.Profiles
{
    public class GameState_Wrapper : GameState
    {
        private Provider_Wrapper _Provider;
        private string _Command;
        private Command_Wrapper _Command_Data;
        private byte[] _Bitmap;
        private Extra_Keys_Wrapper _Extra_Keys;

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

        public GameState_Wrapper()
        {
            json = "{}";
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json);
        }

        public GameState_Wrapper(string json_data) : base(json_data)
        {
            if (String.IsNullOrWhiteSpace(json_data))
                json_data = "{}";

            json = json_data;
            _ParsedData = JObject.Parse(json_data);
        }

        public GameState_Wrapper(GameState other_state) : base(other_state)
        {
        }
    }

    public class Provider_Wrapper : Node
    {
        public readonly string Name;
        public int AppID;

        internal Provider_Wrapper(string JSON)
            : base(JSON)
        {
            Name = GetString("name");
            AppID = GetInt("appid");
        }
    }

    public class Command_Wrapper : Node
    {
        public readonly int red_start;
        public readonly int green_start;
        public readonly int blue_start;
        public readonly int red_end;
        public readonly int green_end;
        public readonly int blue_end;
        public readonly int duration;
        public readonly int interval;
        public readonly string effect_type;
        public readonly int key;
        public readonly int custom_mode;

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
            key = GetInt("key");
            custom_mode = GetInt("custom_mode");
        }
    }

    public class Extra_Keys_Wrapper : Node
    {
        public readonly int[] peripheral;
        public readonly int[] logo;
        public readonly int[] badge;
        public readonly int[] G1;
        public readonly int[] G2;
        public readonly int[] G3;
        public readonly int[] G4;
        public readonly int[] G5;
        public readonly int[] G6;
        public readonly int[] G7;
        public readonly int[] G8;
        public readonly int[] G9;
        public readonly int[] G10;
        public readonly int[] G11;
        public readonly int[] G12;
        public readonly int[] G13;
        public readonly int[] G14;
        public readonly int[] G15;
        public readonly int[] G16;
        public readonly int[] G17;
        public readonly int[] G18;
        public readonly int[] G19;
        public readonly int[] G20;

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
