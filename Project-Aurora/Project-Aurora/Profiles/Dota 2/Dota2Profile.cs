using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Devices;

namespace Aurora.Profiles.Dota_2
{
    public class Dota2Profile : ApplicationProfile
    {
        public Dota2Profile() : base()
        {
            
        }

        public override void Reset()
        {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {

                new Layer("Dota 2 Respawn", new Layers.Dota2RespawnLayerHandler()),
                new Layer("Health Indicator", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.FromArgb(0, 255, 0),
                        _SecondaryColor = Color.FromArgb(0, 60, 0),
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new DeviceKeys[] {
                            DeviceKeys.F1, DeviceKeys.F2, DeviceKeys.F3, DeviceKeys.F4,
                            DeviceKeys.F5, DeviceKeys.F6, DeviceKeys.F7, DeviceKeys.F8,
                            DeviceKeys.F9, DeviceKeys.F10, DeviceKeys.F11, DeviceKeys.F12
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Hero/Health",
                        _MaxVariablePath = "Hero/MaxHealth"
                    },
                }),
                new Layer("Mana Indicator", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.FromArgb(0, 125, 255),
                        _SecondaryColor = Color.FromArgb(0, 0, 60),
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new DeviceKeys[] {
                            DeviceKeys.ONE, DeviceKeys.TWO, DeviceKeys.THREE, DeviceKeys.FOUR,
                            DeviceKeys.FIVE, DeviceKeys.SIX, DeviceKeys.SEVEN, DeviceKeys.EIGHT,
                            DeviceKeys.NINE, DeviceKeys.ZERO, DeviceKeys.MINUS, DeviceKeys.EQUALS
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Hero/Mana",
                        _MaxVariablePath = "Hero/MaxMana"
                    },
                }),
                new Layer("Dota 2 Ability Keys", new Layers.Dota2AbilityLayerHandler()),
                new Layer("Dota 2 Item Keys", new Layers.Dota2ItemLayerHandler()),
                new Layer("Dota 2 Hero Ability Effects", new Layers.Dota2HeroAbilityEffectsLayerHandler()),
                new Layer("Dota 2 Killstreaks", new Layers.Dota2KillstreakLayerHandler()),
                new Layer("Dota 2 Background", new Layers.Dota2BackgroundLayerHandler())
            };
        }
    }
}
