using Aurora.Devices.Layout;
using Aurora.Devices.Layout.Layouts;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace Aurora.Profiles.QuantumConumdrum
{
    public class QuantumConumdrumProfile : ApplicationProfile
    {
        public QuantumConumdrumProfile() : base()
        {
            
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if (!Layers.Any(lyr => lyr.Handler.GetType().Equals(typeof(Aurora.Settings.Layers.WrapperLightsLayerHandler))))
                Layers.Add(new Layer("Wrapper Lighting", new Aurora.Settings.Layers.WrapperLightsLayerHandler()));
        }


        public override void Reset()
        {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("Movement", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.DodgerBlue,
                        _Sequence = new KeySequence(new List<KeyboardKeys> { KeyboardKeys.W, KeyboardKeys.A, KeyboardKeys.S, KeyboardKeys.D, KeyboardKeys.SPACE})
                    }
                }
                ),
                new Layer("Fluffy", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Pink,
                        _Sequence = new KeySequence(new List<KeyboardKeys> { KeyboardKeys.Q})
                    }
                }
                ),
                new Layer("Heavy", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Red,
                        _Sequence = new KeySequence(new List<KeyboardKeys> { KeyboardKeys.E})
                    }
                }
                ),
                new Layer("Slow", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Yellow,
                        _Sequence = new KeySequence(new List<KeyboardKeys> { KeyboardKeys.ONE})
                    }
                }
                ),
                new Layer("Reverse", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Green,
                        _Sequence = new KeySequence(new List<KeyboardKeys> { KeyboardKeys.THREE})
                    }
                }
                ),
                new Layer("Wrapper Lighting", new Aurora.Settings.Layers.WrapperLightsLayerHandler())
            };
        }
    }
}
