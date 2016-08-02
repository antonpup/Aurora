using Aurora;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Devices;
using Aurora.Settings;
using System;
using System.Drawing;

public class ExampleEffect
{
    public string ID = "ExampleCSScript";

    public Color ForegroundColour = Color.Red;
    
    public Color BackgroundColour = Color.Black;
    
    public KeySequence DefaultKeys = new KeySequence(new[] { DeviceKeys.TAB, DeviceKeys.Q,  DeviceKeys.W, DeviceKeys.E, DeviceKeys.R, DeviceKeys.T, DeviceKeys.Y, DeviceKeys.U, DeviceKeys.I, DeviceKeys.O, DeviceKeys.P });
    
    public EffectLayer UpdateLights(ScriptSettings settings, GameState state = null)
    {
        EffectLayer layer = new EffectLayer(this.ID);
        layer.PercentEffect(ForegroundColour, BackgroundColour, settings.Keys, DateTime.Now.Second, 60D);
        return layer;
    }
}