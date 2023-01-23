namespace Aurora.Profiles.Discord;

public class Discord : Application {

    public Discord() : base(new LightEventConfig
    {
        Name = "Discord",
        ID = "discord",
        ProcessNames = new[] { "Discord.exe", "DiscordPTB.exe", "DiscordCanary.exe" },
        ProfileType = typeof(DiscordProfile),
        OverviewControlType = typeof(Control_Discord),
        GameStateType = typeof(GSI.GameState_Discord),
        IconURI = "Resources/betterdiscord.png",
        EnableByDefault = false
    }) { }
}