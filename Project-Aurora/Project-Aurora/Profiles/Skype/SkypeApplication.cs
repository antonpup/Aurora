using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Skype
{
    public class Skype : Application
    {
        public Skype()
            : base(new LightEventConfig { Name = "Skype", ID = "skype", ProcessNames = new[] { "skype.exe" }, ProfileType = typeof(SkypeProfile), OverviewControlType = typeof(Control_Skype), GameStateType = typeof(State_SkypeOverlay), Event = new GameEvent_Generic(), IconURI = "Resources/Skype_300x300.png" })
        {

        }

        public override bool Initialize()
        {
            bool ret = base.Initialize();
            if (ret)
            {
                if (this.IsEnabled)
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = Path.Combine(Global.ExecutingDirectory, "Aurora-SkypeIntegration.exe");
                        Process.Start(startInfo);
                    }
                    catch (Exception exc)
                    {
                        Global.logger.Error("Could not start Skype Integration. Error: " + exc);
                    }
                }
            }

            return ret;
        }
    }
}
