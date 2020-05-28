using Aurora.Settings.Layers;
using Aurora.Utils;
using System;

namespace Aurora.Profiles.Move_or_Die
{
    public class MoD : Application
    {
        public MoD() : base(new LightEventConfig {
            Name = "Move or Die",
            ID = "MoD",
            ProcessNames = new[] { "love.exe" },
            ProfileType = typeof(MoDProfile),
            OverviewControlType = typeof(Control_MoD),
            GameStateType = typeof(GameState_Wrapper),
            Event = new GameEvent_Generic(),
            IconURI = "Resources/MoD.png"
        })
        {
            AllowLayer<WrapperLightsLayerHandler>();
            binder = new MoDSerializationBinder();
        }
    }

    public class MoDSerializationBinder : AuroraSerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (typeName == "Aurora.Profiles.WrapperProfile")
                return typeof(MoDProfile);

            return base.BindToType(assemblyName, typeName);
        }
    }
}
