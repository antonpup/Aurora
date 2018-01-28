using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Aurora.Profiles.LeagueOfLegends
{
    public class LoLProfile : ApplicationProfile
    {
        public LoLProfile() : base()
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
                new Layer("Actions", new SolidColorLayerHandler()
                {
                    Properties = new LayerHandlerProperties()
                    {
                        _PrimaryColor = System.Drawing.Color.Blue,
                        _Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.Q, Devices.DeviceKeys.W, Devices.DeviceKeys.E, Devices.DeviceKeys.R })
                    }
                }),
                new Layer("Wrapper Lighting", new Aurora.Settings.Layers.WrapperLightsLayerHandler()),
            };
        }
    }
}
