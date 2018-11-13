using System;
using System.Drawing;
using Aurora.Applications.Application;
using Aurora.EffectsEngine;
using Aurora.Settings;
using Newtonsoft.Json;

namespace Aurora.Applications.Layers
{
    public interface ILayerHandler
    {
        IStringProperty Properties { get; }
        
        IEffectLayer Render(GameState gs);

    }
}