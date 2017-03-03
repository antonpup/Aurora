using Aurora;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Devices;
using Aurora.Utils;
using Aurora.Settings;
using System;
using System.Drawing;
using System.Collections.Generic;

public class ExampleEffect : IEffectScript
{
    public KeySequence DefaultKeys = new KeySequence();
    
    public string ID {get; private set;}

	public VariableRegistry Properties {get; private set;}
        
    public ExampleEffect()
    {
        ID = "ExampleCSScript (Multi-Layer)";
        Properties = new VariableRegistry();
        //Create Properties
    }
    
    public object UpdateLights(VariableRegistry settings, IGameState state = null)
    {
        Queue<EffectLayer> layers = new Queue<EffectLayer>();
        
        EffectLayer layer = new EffectLayer(this.ID);
        layer.PercentEffect(Color.Purple, Color.Green, new KeySequence(new[] { DeviceKeys.F12, DeviceKeys.F11,  DeviceKeys.F10, DeviceKeys.F9, DeviceKeys.F8, DeviceKeys.F7, DeviceKeys.F6, DeviceKeys.F5, DeviceKeys.F4, DeviceKeys.F3, DeviceKeys.F2 , DeviceKeys.F1 }), DateTime.Now.Second % 20D, 20D);
        layers.Enqueue(layer);
        
        EffectLayer layer_swirl = new EffectLayer(this.ID + " - Swirl");
        layer_swirl.PercentEffect(Color.Blue, Color.Black, new KeySequence(new[] { DeviceKeys.NUM_ONE, DeviceKeys.NUM_FOUR,  DeviceKeys.NUM_SEVEN, DeviceKeys.NUM_EIGHT, DeviceKeys.NUM_NINE, DeviceKeys.NUM_SIX, DeviceKeys.NUM_THREE, DeviceKeys.NUM_TWO }), DateTime.Now.Millisecond % 500D, 500D, PercentEffectType.Progressive_Gradual);
        layers.Enqueue(layer_swirl);
        
        EffectLayer layer_blinking = new EffectLayer(this.ID + " - Blinking Light");
        
        ColorSpectrum blink_spec = new ColorSpectrum(Color.Red, Color.Black, Color.Red);
        Color blink_color = blink_spec.GetColorAt(DateTime.Now.Millisecond / 1000.0f);
        layer_blinking.Set(DeviceKeys.NUM_FIVE, blink_color);
        layers.Enqueue(layer_blinking);
        
        return layers.ToArray();
    }
}