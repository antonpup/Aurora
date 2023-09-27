using System;
using System.Drawing;
using Common.Utils;

namespace Aurora.Profiles;

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
    [GameStateIgnore]
    public Provider_Wrapper Provider => _Provider ??= new Provider_Wrapper(_ParsedData["provider"]?.ToString() ?? "");

    /// <summary>
    /// The sent wrapper command
    /// </summary>
    [GameStateIgnore]
    public string Command
    {
        get
        {
            if (_Command != null) return _Command;
            _Command = _ParsedData.TryGetValue("command", out var value) ? value.ToString() : "";
            return _Command;
        }
    }

    /// <summary>
    /// Data related to the passed command
    /// </summary>
    [GameStateIgnore]
    public Command_Wrapper Command_Data => _Command_Data ??= new Command_Wrapper(_ParsedData["command_data"]?.ToString() ?? "");

    /// <summary>
    /// The bitmap sent from the wrapper
    /// </summary>
    [GameStateIgnore]
    public int[] Sent_Bitmap
    {
        get
        {
            return _Bitmap ??= _ParsedData.TryGetValue("bitmap", out var value)
                ? value.ToObject<int[]>()
                : Array.Empty<int>();
        }
    }

    /// <summary>
    /// Lighting information for extra keys that are not part of the bitmap
    /// </summary>
    [GameStateIgnore]
    public Extra_Keys_Wrapper Extra_Keys => _Extra_Keys ??= new Extra_Keys_Wrapper(_ParsedData["extra_keys"]?.ToString() ?? "");

    /// <summary>
    /// Creates a default GameState_Wrapper instance.
    /// </summary>
    public GameState_Wrapper() { }

    /// <summary>
    /// Creates a GameState_Wrapper instance based on the passed json data.
    /// </summary>
    /// <param name="json_data">The passed json data</param>
    public GameState_Wrapper(string json_data) : base(json_data) { }
}

/// <summary>
/// Class representing provider information for the wrapper
/// </summary>
public class Provider_Wrapper : Node
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
public class Command_Wrapper : Node
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
public class Extra_Keys_Wrapper : Node
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
        peripheral = CommonColorUtils.GetColorFromInt(GetInt("peripheral"));
        logo = CommonColorUtils.GetColorFromInt( GetInt("logo"));
        mousepad1 = CommonColorUtils.GetColorFromInt(GetInt("mousepad0"));
        mousepad2 = CommonColorUtils.GetColorFromInt(GetInt("mousepad1"));
        mousepad3 = CommonColorUtils.GetColorFromInt(GetInt("mousepad2"));
        mousepad4 = CommonColorUtils.GetColorFromInt(GetInt("mousepad3"));
        mousepad5 = CommonColorUtils.GetColorFromInt(GetInt("mousepad4"));
        mousepad6 = CommonColorUtils.GetColorFromInt(GetInt("mousepad5"));
        mousepad7 = CommonColorUtils.GetColorFromInt(GetInt("mousepad6"));
        mousepad8 = CommonColorUtils.GetColorFromInt(GetInt("mousepad7"));
        mousepad9 = CommonColorUtils.GetColorFromInt(GetInt("mousepad8"));
        mousepad10 = CommonColorUtils.GetColorFromInt(GetInt("mousepad9"));
        mousepad11 = CommonColorUtils.GetColorFromInt(GetInt("mousepad10"));
        mousepad12 = CommonColorUtils.GetColorFromInt(GetInt("mousepad11"));
        mousepad13 = CommonColorUtils.GetColorFromInt(GetInt("mousepad12"));
        mousepad14 = CommonColorUtils.GetColorFromInt(GetInt("mousepad13"));
        mousepad15 = CommonColorUtils.GetColorFromInt(GetInt("mousepad14"));
        badge = CommonColorUtils.GetColorFromInt( GetInt("badge"));
        G1 = CommonColorUtils.GetColorFromInt( GetInt("G1"));
        G2 = CommonColorUtils.GetColorFromInt( GetInt("G2"));
        G3 = CommonColorUtils.GetColorFromInt( GetInt("G3"));
        G4 = CommonColorUtils.GetColorFromInt( GetInt("G4"));
        G5 = CommonColorUtils.GetColorFromInt( GetInt("G5"));
        G6 = CommonColorUtils.GetColorFromInt( GetInt("G6"));
        G7 = CommonColorUtils.GetColorFromInt( GetInt("G7"));
        G8 = CommonColorUtils.GetColorFromInt( GetInt("G8"));
        G9 = CommonColorUtils.GetColorFromInt( GetInt("G9"));
        G10 = CommonColorUtils.GetColorFromInt( GetInt("G10"));
        G11 = CommonColorUtils.GetColorFromInt( GetInt("G11"));
        G12 = CommonColorUtils.GetColorFromInt( GetInt("G12"));
        G13 = CommonColorUtils.GetColorFromInt( GetInt("G13"));
        G14 = CommonColorUtils.GetColorFromInt( GetInt("G14"));
        G15 = CommonColorUtils.GetColorFromInt( GetInt("G15"));
        G16 = CommonColorUtils.GetColorFromInt( GetInt("G16"));
        G17 = CommonColorUtils.GetColorFromInt( GetInt("G17"));
        G18 = CommonColorUtils.GetColorFromInt( GetInt("G18"));
        G19 = CommonColorUtils.GetColorFromInt( GetInt("G19"));
        G20 = CommonColorUtils.GetColorFromInt( GetInt("G20"));
    }
}