using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles
{
    public class RazerChromaProfile : ApplicationProfile
    {
        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if (!Layers.Any(lyr => lyr.Handler.GetType().Equals(typeof(RazerLayerHandler))))
                Layers.Add(new Layer("Chroma Lighting", new RazerLayerHandler()));
        }

        public override void Reset()
        {
            base.Reset();
            Layers = new ObservableCollection<Layer>()
            {
                new Layer("Chroma Lighting", new RazerLayerHandler()),
            };
        }
    }
}
