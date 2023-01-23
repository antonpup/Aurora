namespace Aurora.Profiles.Subnautica;

public class Subnautica : Application {

    public Subnautica() : base(new LightEventConfig
    {
        Name = "Subnautica",
        ID = "subnautica",
        AppID = "264710",
        ProcessNames = new[] { "Subnautica.exe" },
        ProfileType = typeof(SubnauticaProfile),
        OverviewControlType = typeof(Control_Subnautica),
        GameStateType = typeof(GSI.GameState_Subnautica),
        IconURI = "Resources/Subnautica.png"
    })
    { }        
}