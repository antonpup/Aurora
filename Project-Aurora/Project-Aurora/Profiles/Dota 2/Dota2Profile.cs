using Aurora.Devices.Layout.Layouts;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                        _Sequence = new KeySequence(new KeyboardKeys[] {
                            KeyboardKeys.F1, KeyboardKeys.F2, KeyboardKeys.F3, KeyboardKeys.F4,
                            KeyboardKeys.F5, KeyboardKeys.F6, KeyboardKeys.F7, KeyboardKeys.F8,
                            KeyboardKeys.F9, KeyboardKeys.F10, KeyboardKeys.F11, KeyboardKeys.F12
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Hero/Health",
                        _MaxVariablePath = "Hero/MaxHealth"
                    },
                })
                {
                    Logics = new System.Collections.ObjectModel.ObservableCollection<LogicItem>()
                    {
                    }
                },
                new Layer("Mana Indicator", new PercentLayerHandler()
                {
                    Properties = new PercentLayerHandlerProperties()
                    {
                        _PrimaryColor =  Color.FromArgb(0, 125, 255),
                        _SecondaryColor = Color.FromArgb(0, 0, 60),
                        _PercentType = PercentEffectType.Progressive_Gradual,
                        _Sequence = new KeySequence(new KeyboardKeys[] {
                            KeyboardKeys.ONE, KeyboardKeys.TWO, KeyboardKeys.THREE, KeyboardKeys.FOUR,
                            KeyboardKeys.FIVE, KeyboardKeys.SIX, KeyboardKeys.SEVEN, KeyboardKeys.EIGHT,
                            KeyboardKeys.NINE, KeyboardKeys.ZERO, KeyboardKeys.MINUS, KeyboardKeys.EQUALS
                        }),
                        _BlinkThreshold = 0.0,
                        _BlinkDirection = false,
                        _VariablePath = "Hero/Mana",
                        _MaxVariablePath = "Hero/MaxMana"
                    },
                })
                {
                    Logics = new System.Collections.ObjectModel.ObservableCollection<LogicItem>()
                    {
                    }
                },
                new Layer("Dota 2 Ability Keys", new Layers.Dota2AbilityLayerHandler()),
                new Layer("Dota 2 Item Keys", new Layers.Dota2ItemLayerHandler()),
                new Layer("Dota 2 Hero Ability Effects", new Layers.Dota2HeroAbilityEffectsLayerHandler()),
                new Layer("Dota 2 Killstreaks", new Layers.Dota2KillstreakLayerHandler()),
                new Layer("Dota 2 Background", new Layers.Dota2BackgroundLayerHandler())
            };
        }
    }
}
