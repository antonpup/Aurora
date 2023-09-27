using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using Aurora.Devices;
using Aurora.EffectsEngine;
using Aurora.Profiles.GTA5.GSI;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Common.Devices;
using Newtonsoft.Json;

namespace Aurora.Profiles.GTA5.Layers;

public class Gta5PoliceSirenLayerHandlerProperties : LayerHandlerProperties2Color<Gta5PoliceSirenLayerHandlerProperties>
{
    private Color? _leftSirenColor;

    [JsonProperty("_LeftSirenColor")]
    public Color LeftSirenColor
    {
        get => Logic._leftSirenColor ?? _leftSirenColor ?? Color.Empty;
        set => _leftSirenColor = value;
    }

    private Color? _rightSirenColor;

    [JsonProperty("_RightSirenColor")]
    public Color RightSirenColor
    {
        get => Logic._rightSirenColor ?? _rightSirenColor ?? Color.Empty;
        set => _rightSirenColor = value;
    }

    private GTA5_PoliceEffects? _sirenType;

    [JsonProperty("_SirenType")]
    public GTA5_PoliceEffects SirenType
    {
        get => Logic._sirenType ?? _sirenType ?? GTA5_PoliceEffects.Default;
        set => _sirenType = value;
    }

    private KeySequence _leftSirenSequence;

    [JsonProperty("_LeftSirenSequence")]
    public KeySequence LeftSirenSequence
    {
        get => Logic._leftSirenSequence ?? _leftSirenSequence ?? new KeySequence();
        set
        {
            if (_leftSirenSequence != null)
            {
                var oldFreeForm = _leftSirenSequence.Freeform;
                WeakEventManager<FreeFormObject, EventArgs>.RemoveHandler(oldFreeForm, nameof(oldFreeForm.ValuesChanged), OnPropertiesChanged);
            }
            _leftSirenSequence = value;
            var freeForm = _leftSirenSequence.Freeform;
            WeakEventManager<FreeFormObject, EventArgs>.AddHandler(freeForm, nameof(freeForm.ValuesChanged), OnPropertiesChanged);
        }
    }

    private KeySequence _rightSirenSequence;

    [JsonProperty("_RightSirenSequence")]
    public KeySequence RightSirenSequence
    {
        get => Logic._rightSirenSequence ?? _rightSirenSequence ?? new KeySequence();
        set
        {
            if (_rightSirenSequence != null)
            {
                var oldFreeForm = _rightSirenSequence.Freeform;
                WeakEventManager<FreeFormObject, EventArgs>.RemoveHandler(oldFreeForm, nameof(oldFreeForm.ValuesChanged), OnPropertiesChanged);
            }
            _rightSirenSequence = value;
            var freeForm = _rightSirenSequence.Freeform;
            WeakEventManager<FreeFormObject, EventArgs>.AddHandler(freeForm, nameof(freeForm.ValuesChanged), OnPropertiesChanged);
        }
    }

    private bool? _peripheralUse;

    [JsonProperty("_PeripheralUse")]
    public bool PeripheralUse
    {
        get => Logic._peripheralUse ?? _peripheralUse ?? false;
        set => _peripheralUse = value;
    }

    public Gta5PoliceSirenLayerHandlerProperties()
    { }

    public Gta5PoliceSirenLayerHandlerProperties(bool assignDefault = false) : base(assignDefault) { }

    public override void Default()
    {
        base.Default();

        _leftSirenColor = Color.FromArgb(255, 0, 0);
        _rightSirenColor = Color.FromArgb(0, 0, 255);
        _sirenType = GTA5_PoliceEffects.Default;
        LeftSirenSequence = new KeySequence(new[] {
            DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3,
            DeviceKeys.F4, DeviceKeys.F5, DeviceKeys.F6
        });
        RightSirenSequence = new KeySequence(new[] {
            DeviceKeys.F7, DeviceKeys.F8, DeviceKeys.F9,
            DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12
        });
        _peripheralUse = true;
    }

}

public class GTA5PoliceSirenLayerHandler : LayerHandler<Gta5PoliceSirenLayerHandlerProperties>
{
    private Color _leftSirenColor = Color.Empty;
    private Color _rightSirenColor = Color.Empty;
    private int _sirenKeyframe;
    private bool _clear;

    public GTA5PoliceSirenLayerHandler() : base("GTA 5 - Police Sirens")
    {
    }

    protected override UserControl CreateControl()
    {
        return new Control_GTA5PoliceSirenLayer(this);
    }

    public override EffectLayer Render(IGameState state)
    {
        if (state is not GameState_GTA5 { HasCops: true } gta5State) return EffectLayer.EmptyLayer;

        if (_clear)
        {
            EffectLayer.Clear();
            _clear = false;
        }

        if (_leftSirenColor != gta5State.LeftSirenColor && _rightSirenColor != gta5State.RightSirenColor)
            _sirenKeyframe++;

        _leftSirenColor = gta5State.LeftSirenColor;
        _rightSirenColor = gta5State.RightSirenColor;

        Color lefts = Properties.LeftSirenColor;
        Color rights = Properties.RightSirenColor;

        //Switch sirens
        switch (Properties.SirenType)
        {
            case GTA5_PoliceEffects.Alt_Full:
                switch (_sirenKeyframe % 2)
                {
                    case 1:
                        rights = lefts;
                        break;
                    default:
                        lefts = rights;
                        break;
                }
                _sirenKeyframe %= 2;

                if (Properties.PeripheralUse)
                    EffectLayer.Set(DeviceKeys.Peripheral, lefts);
                break;
            case GTA5_PoliceEffects.Alt_Half:
                switch (_sirenKeyframe % 2)
                {
                    case 1:
                        rights = lefts;
                        lefts = Color.Black;

                        if (Properties.PeripheralUse)
                            EffectLayer.Set(DeviceKeys.Peripheral, rights);
                        break;
                    default:
                        lefts = rights;
                        rights = Color.Black;

                        if (Properties.PeripheralUse)
                            EffectLayer.Set(DeviceKeys.Peripheral, lefts);
                        break;
                }
                _sirenKeyframe %= 2;
                break;
            case GTA5_PoliceEffects.Alt_Full_Blink:
                switch (_sirenKeyframe % 4)
                {
                    case 2:
                        rights = lefts;
                        break;
                    case 0:
                        lefts = rights;
                        break;
                    default:
                        lefts = Color.Black;
                        rights = Color.Black;
                        break;
                }
                _sirenKeyframe %= 4;

                if (Properties.PeripheralUse)
                    EffectLayer.Set(DeviceKeys.Peripheral, lefts);
                break;
            case GTA5_PoliceEffects.Alt_Half_Blink:
                switch (_sirenKeyframe % 8)
                {
                    case 6:
                        rights = lefts;
                        lefts = Color.Black;

                        if (Properties.PeripheralUse)
                            EffectLayer.Set(DeviceKeys.Peripheral, rights);
                        break;
                    case 4:
                        rights = lefts;
                        lefts = Color.Black;

                        if (Properties.PeripheralUse)
                            EffectLayer.Set(DeviceKeys.Peripheral, rights);
                        break;
                    case 2:
                        lefts = rights;
                        rights = Color.Black;

                        if (Properties.PeripheralUse)
                            EffectLayer.Set(DeviceKeys.Peripheral, lefts);
                        break;
                    case 0:
                        lefts = rights;
                        rights = Color.Black;

                        if (Properties.PeripheralUse)
                            EffectLayer.Set(DeviceKeys.Peripheral, lefts);
                        break;
                    default:
                        rights = Color.Black;
                        lefts = Color.Black;

                        if (Properties.PeripheralUse)
                            EffectLayer.Set(DeviceKeys.Peripheral, lefts);
                        break;
                }
                _sirenKeyframe %= 8;
                break;
            default:
                switch (_sirenKeyframe % 2)
                {
                    case 1:
                        Color tempc = rights;
                        rights = lefts;
                        lefts = tempc;
                        break;
                }
                _sirenKeyframe %= 2;

                if (Properties.PeripheralUse)
                    EffectLayer.Set(DeviceKeys.Peripheral, lefts);
                break;
        }

        EffectLayer.Set(Properties.LeftSirenSequence, lefts);
        EffectLayer.Set(Properties.RightSirenSequence, rights);

        return EffectLayer;
    }

    public override void SetApplication(Application profile)
    {
        (Control as Control_GTA5PoliceSirenLayer).SetProfile(profile);
        base.SetApplication(profile);
    }

    protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);

        _clear = true;
    }
}