using System;
using Aurora.Settings;

namespace Aurora.Profiles.Witcher3;

public class Witcher3 : Application
{
    public Witcher3()
        : base(new LightEventConfig(new Lazy<LightEvent>(() => new GameEventWitcher3()))
        {
            Name = "The Witcher 3",
            ID = "Witcher3",
            ProcessNames = new[] { "Witcher3.exe" },
            SettingsType = typeof(FirstTimeApplicationSettings),
            ProfileType = typeof(Witcher3Profile),
            OverviewControlType = typeof(Control_Witcher3),
            GameStateType = typeof(GSI.GameStateWitcher3),
            IconURI = "Resources/Witcher3_256x256.png"
        })
    {
        AllowLayer<Layers.Witcher3BackgroundLayerHandler>();
    }
}