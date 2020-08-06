using Aurora.Profiles;
using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Aurora.Service
{
    public static class ProfileSwitcher
    {
        public static string APIKey = "1713f463732b4f10909b015251dcaa108ebc0e95b2534807a01b0e666d8d430e60c268c413cb495d996fc76b8b676aac";
        public static void Init()
        {
            UpdateSettings();
            WebSocketClient.Connect(Global.Configuration.ClientID);
        }
        internal static void Shutdown()
        {
            WebSocketClient.DisconnectAsync();

        }
        public static void UpdateSettings()
        {
            if (Global.Configuration.ClientID.Length < 2)
            {
                    GetNewClientID();
            }
        }
        public static string GetNewClientID()
        {
            string id = Guid.NewGuid().ToString();
            Global.Configuration.ClientID = id;
            
            ConfigManager.Save(Global.Configuration);
            WebSocketClient.Connect(id);
            return id;
        }

        private static Profiles.Application previousApplication = null;
        private static ApplicationProfile previousProfile = null;
        private static Profiles.Application nextApplication = null;
        private static ApplicationProfile nextProfile = null;
        public static void Switch(string profile)
        {
            if (profile == "")
            {
                //revert to previous profile.
                nextApplication = previousApplication;
                nextProfile = previousProfile;
                previousApplication = null;
                previousProfile = null;
                nextApplication.SwitchToProfile(nextProfile);
            }
            else
            {
                foreach (var item in Global.LightingStateManager.Events.Values)
                {
                    if (item is Profiles.Application)
                    {
                        foreach (var p in (item as Profiles.Application).Profiles)
                        {

                            if (p.ProfileName == profile)
                            {
                               
                                previousApplication = item as Profiles.Application;
                                previousProfile = previousApplication.Profile;
                               
                                nextApplication = (item as Profiles.Application);
                                nextProfile = p;

                                nextApplication.SwitchToProfile(nextProfile);
                                return;
                            }
                        }

                    }
                }
            }           
        }
        public static void Switch(ProfileSwitcherCommand p)
        {
            if (p.Payload == null) return;
            if (p.APIKey != APIKey)
            {
                //should never happen
                return;
            }
            if (p.Payload.ClientID != Global.Configuration.ClientID)
            {
                //could happen
                return;
            }

            Switch(p.Payload.Profile);
         //   if (p.Payload.ProfileEnd != "")
            {
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    if (p.Payload.Duration > 0)
                        Thread.Sleep(p.Payload.Duration);
                    Switch(p.Payload.ProfileEnd);
                });
            }
          //  else
            {
                //there is no ending profile, revert to default if there is 
            }
        }


    }
}
