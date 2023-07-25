using System;
using System.IO;
using Aurora.Modules;

namespace Aurora.Profiles.Logitech;

public class LogitechApplication : Application
{
    public LogitechApplication() : base(new LightEventConfig
    {
        Name = "Logitech Lightsync",
        ID = "logitech",
        ProcessNames = Array.Empty<string>(),
        ProfileType = typeof(LogitechProfile),
        GameStateType = typeof(GameState_Wrapper),
        OverviewControlType = typeof(Control_Logitech),
        IconURI = "Resources/G-sync.png",
        EnableByDefault = true,
    })
    {
        LogitechSdkModule.LogitechSdkListener.ApplicationChanged += LogitechSdkListenerOnApplicationChanged;
    }

    private void LogitechSdkListenerOnApplicationChanged(object? sender, string? e)
    {
        Config.ProcessNames = e == null ? Array.Empty<string>() : new []{ Path.GetFileName(e) };
    }
}