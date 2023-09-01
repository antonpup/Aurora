using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace Aurora.Profiles.Desktop;

public class Event_Desktop : LightEvent
{
    private readonly EffectLayer _timeBasedDimLayer = new("Time Based Dim", Color.Black, true);

    public override void UpdateLights(EffectFrame frame)
    {
        var layers = Application.Profile.Layers.Where(l => l.Enabled).Reverse().Select(l => l.Render(_game_state));
        frame.AddLayers(layers);
    }

    public override void SetGameState(IGameState newGameState)
    {

    }
}