using Aurora.Settings.Layers;

namespace Aurora.Profiles.DeadCells
{
    public class DeadCells : Application
    {
        public DeadCells()
            : base(new LightEventConfig {
                Name = "Dead Cells",
                ID = "deadcells",
                ProcessNames = new[] { "deadcells.exe", "deadcells_gl.exe" },
                ProfileType = typeof(WrapperProfile),
                OverviewControlType = typeof(Control_DeadCells),
                GameStateType = typeof(GameState_Wrapper),
                Event = new GameEvent_Generic(),
                IconURI = "Resources/deadcells_128x128.png"
            })
        {
            AllowLayer<WrapperLightsLayerHandler>();
        }
    }
}
