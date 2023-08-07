using System;
using System.IO;

namespace Aurora.Profiles.EliteDangerous;

public static class EliteConfig
{
    public const int KeyBlinkSpeed = 20;
    public static readonly string JournalApiDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Saved Games",
        "Frontier Developments",
        "Elite Dangerous"
    );
    public static readonly string StatusFile = Path.Combine(JournalApiDir, "Status.json");

    public static readonly string BindingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Frontier Developments",
        "Elite Dangerous",
        "Options",
        "Bindings"
    );
    public static readonly string BindingsPresetFile = Path.Combine(BindingsDir, "StartPreset.4.start");
}