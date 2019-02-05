using System.Collections.Generic;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Drawing;
using System.Runtime.Serialization;
using System.Linq;
using Aurora.Devices.Layout.Layouts;
using Aurora.Devices.Layout;

namespace Aurora.Profiles.Blacklight
{
    public class BLightProfile : ApplicationProfile
    {
        public BLightProfile() : base()
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
                        _PrimaryColor = Color.Orange,
                        _Sequence = new KeySequence((new List<KeyboardKeys> { KeyboardKeys.W, KeyboardKeys.A, KeyboardKeys.S, KeyboardKeys.D }).ConvertAll(s => s.GetDeviceLED()))
                    }
                }),
                new Layer("Wrapper Lighting", new Aurora.Settings.Layers.WrapperLightsLayerHandler()),
            };
        }
    }
}
