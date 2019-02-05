using Aurora.Devices.Layout;
using Aurora.Devices.Layout.Layouts;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace Aurora.Profiles.TheTalosPrinciple
{
    public class TalosPrincipleProfile : ApplicationProfile
    {
        public TalosPrincipleProfile() : base()
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
                        _PrimaryColor = Color.LightBlue,
                        _Sequence = new KeySequence(new List<KeyboardKeys> { KeyboardKeys.W, KeyboardKeys.A, KeyboardKeys.S, KeyboardKeys.D }.ConvertAll(s => s.GetDeviceLED()))
                    }
                }
                ),
                new Layer("Other Actions", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = Color.Purple,
                        _Sequence = new KeySequence(new List<KeyboardKeys> { KeyboardKeys.SPACE, KeyboardKeys.LEFT_SHIFT, KeyboardKeys.H, KeyboardKeys.X, KeyboardKeys.TAB }.ConvertAll(s => s.GetDeviceLED()))
                    }
                }),
                new Layer("Wrapper Lighting", new Aurora.Settings.Layers.WrapperLightsLayerHandler()),
            };
        }
    }
}
