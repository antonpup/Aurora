using Aurora.EffectsEngine;
using System.Linq;

namespace Aurora.Profiles.Desktop;

public class Event_Desktop : LightEvent
{
    public override void UpdateLights(EffectFrame frame)
    {
        var layers = Application.Profile.Layers.Where(l => l.Enabled).Reverse().Select(l => l.Render(_game_state));
        frame.AddLayers(layers);
    }

    public override void SetGameState(IGameState newGameState)
    {

    }
}