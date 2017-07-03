using Aurora;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Devices;
using Aurora.Utils;
using Aurora.Settings;
using System;
using System.Drawing;

public class ExampleEffect : IEffectScript
{
    public string ID {get; private set;}

	public VariableRegistry Properties {get; private set;}
    
    public KeySequence DefaultKeys = new KeySequence(new[] { DeviceKeys.TAB, DeviceKeys.Q,  DeviceKeys.W, DeviceKeys.E, DeviceKeys.R, DeviceKeys.T, DeviceKeys.Y, DeviceKeys.U, DeviceKeys.I, DeviceKeys.O, DeviceKeys.P });
    
    public ExampleEffect()
    {
        ID = "ExampleCSScript";
        Properties = new VariableRegistry();
        Properties.Register("keys", DefaultKeys, "Main Keys");
        Properties.Register("foregroundColour", new RealColor(Color.Red), "Foreground Colour");
        Properties.Register("backgroundColour", new RealColor(Color.Black), "Background Colour");
    }
    
    public object UpdateLights(VariableRegistry settings, IGameState state = null)
    {
        EffectLayer layer = new EffectLayer(this.ID);
        layer.PercentEffect(settings.GetVariable<RealColor>("foregroundColour").GetDrawingColor(), settings.GetVariable<RealColor>("backgroundColour").GetDrawingColor(), settings.GetVariable<KeySequence>("keys") ?? DefaultKeys, DateTime.Now.Second, 60D);
        return layer;
    }
}