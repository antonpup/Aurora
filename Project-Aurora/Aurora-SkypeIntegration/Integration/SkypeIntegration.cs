using System.Collections.Generic;

namespace Aurora_SkypeIntegration.Integration
{
    public class SkypeIntegration
    {
        public readonly Dictionary<string, string> provider = new Dictionary<string, string>()
        {
            { "name", "skype.exe" },
            { "appid", "0" },
            { "version", "1" }
        };

        public int MissedMessagesCount;
        public bool IsCalled;

        public SkypeIntegration()
        {
            MissedMessagesCount = 0;
            IsCalled = false;
        }
    }
}
