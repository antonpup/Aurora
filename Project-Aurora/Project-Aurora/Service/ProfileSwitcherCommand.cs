using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Service
{
    public class ProfileSwitcherCommand
    {
        public string APIKey{ get; set; }
        public DataPayload Payload { get; set; }

        public class DataPayload
        {
            public string ClientID { get; set; }
            public string Profile { get; set; }
            public string ProfileEnd { get; set; }
            public int Duration { get; set; }
        }

        public static ProfileSwitcherCommand Generate(ApplicationProfile profile, string clientID, ApplicationProfile profileEnd, int duration)
        {
            ProfileSwitcherCommand p = new ProfileSwitcherCommand();
            p.APIKey = ProfileSwitcher.APIKey;
            p.Payload = new DataPayload();
            p.Payload.Profile = profile.ProfileName;
            p.Payload.ProfileEnd = profileEnd.ProfileName;
            p.Payload.Duration = duration;
            p.Payload.ClientID = clientID;
            return p;
        }

        public static ProfileSwitcherCommand Generate(string profile, string clientID, string profileEnd, int duration)
        {
            ProfileSwitcherCommand p = new ProfileSwitcherCommand();
            p.APIKey = ProfileSwitcher.APIKey;
            p.Payload = new DataPayload();
            p.Payload.Profile = profile;
            p.Payload.ProfileEnd = profileEnd;
            p.Payload.Duration = duration;
            p.Payload.ClientID = clientID;
            return p;
        }
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }
    }

}