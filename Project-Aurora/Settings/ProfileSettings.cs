using System.Collections.Generic;

namespace Aurora.Settings
{
    public class ProfileSettings
    {
        public bool isEnabled { get; set; }

        public HashSet<string> EnabledScripts { get; set; }

        public ProfileSettings()
        {
            isEnabled = true;
            EnabledScripts = new HashSet<string>();
        }
    }
}
