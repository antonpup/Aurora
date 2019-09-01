using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Aurora.Settings.Overrides.Logic;
using Aurora.EffectsEngine;
using System.Runtime.Serialization;
using System.Linq;
using Aurora.Devices.Layout.Layouts;

namespace Aurora.Profiles.Skype
{

    public class SkypeProfile : ApplicationProfile
    {
        public SkypeProfile() : base()
        {

        }


        public override void Reset()
        {
            base.Reset();
            OverlayLayers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("Missed Messages", new BreathingLayerHandler()
                {
                    Properties = new BreathingLayerHandlerProperties()
                    {
                        _Sequence = new KeySequence(new KeyboardKeys[] {
                                KeyboardKeys.PRINT_SCREEN, KeyboardKeys.SCROLL_LOCK, KeyboardKeys.PAUSE_BREAK
                            }),
                        _PrimaryColor = Color.Orange,
                        _SecondaryColor = Color.Black,
                        _EffectSpeed = 2.5f
                    },

                }, new Settings.Overrides.Logic.Builder.OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanMathsComparison(new NumberGSINumeric("Data/MissedMessagesCount"), ComparisonOperator.GT, 0))),
                 new Layer("Call Indicator", new BreathingLayerHandler()
                {
                    Properties = new BreathingLayerHandlerProperties()
                    {
                        _Sequence = new KeySequence(new KeyboardKeys[] {
                                KeyboardKeys.INSERT, KeyboardKeys.HOME, KeyboardKeys.PAGE_UP,
                                KeyboardKeys.DELETE, KeyboardKeys.END, KeyboardKeys.PAGE_DOWN
                            }),
                        _PrimaryColor = Color.Green,
                        _SecondaryColor = Color.Black,
                        _EffectSpeed = 2.5f
                    },

                }, new Settings.Overrides.Logic.Builder.OverrideLogicBuilder().SetDynamicBoolean("_Enabled", new BooleanGSIBoolean("Data/IsCalled")))
        };

        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {

        }
    }
}