using Aurora.Applications.Application;
using Aurora.EffectsEngine;
using Aurora.Settings;

namespace Aurora.Applications.Layers
{
    public interface IPostProcess
    {
        IStringProperty Properties { get; }
        
        IEffectLayer Process(IEffectLayer layer);
        
        IEffectLayer Process(GameState gs, IEffectLayer layer);
    }
}