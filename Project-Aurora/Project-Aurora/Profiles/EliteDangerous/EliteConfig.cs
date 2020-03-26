using System;
using System.IO;

namespace Aurora.Profiles.EliteDangerous
{
    public static class EliteConfig
    {
        public const int KEY_BLINK_SPEED = 20;
        public static readonly string JOURNAL_API_DIR = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Saved Games",
            "Frontier Developments",
            "Elite Dangerous"
        );
        public static readonly string STATUS_FILE = Path.Combine(JOURNAL_API_DIR, "Status.json");

        public static readonly string BINDINGS_DIR = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Frontier Developments",
            "Elite Dangerous",
            "Options",
            "Bindings"
        );
        public static readonly string BINDINGS_PRESET_FILE = Path.Combine(BINDINGS_DIR, "StartPreset.start");
    }
}