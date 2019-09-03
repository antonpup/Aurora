using System;
using System.Diagnostics;
using System.IO;

namespace Aurora.Profiles.Skype
{
    public class Skype : Application
    {
        public Skype()
            : base(new LightEventConfig { Name = "Skype", ID = "skype", ProcessNames = new[] { "skype.exe" }, ProfileType = typeof(SkypeProfile), GameStateType = typeof(State_SkypeOverlay), Event = new GameEvent_Generic(), IconURI = "Resources/Skype_300x300.png" })
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
                        startInfo.FileName = Path.Combine(Const.ExecutingDirectory, "Aurora-SkypeIntegration.exe");
                        Process.Start(startInfo);
                    }
                    catch (Exception exc)
                    {
                        AuroraCore.logger.Error("Could not start Skype Integration. Error: " + exc);
                    }
                }
            }

            return ret;
        }
    }
}
