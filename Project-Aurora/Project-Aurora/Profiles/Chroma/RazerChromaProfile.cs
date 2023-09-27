using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Common.Utils;

namespace Aurora.Profiles;

public class RazerChromaProfile : ApplicationProfile
{
    [OnDeserialized]
    void OnDeserialized(StreamingContext context)
    {
        if (Layers.Any(lyr => lyr.Handler.GetType().Equals(typeof(RazerLayerHandler)))) return;
        Layers.Add(new Layer("Chroma Lighting", new RazerLayerHandler()));
        var solidFillLayerHandler = new SolidFillLayerHandler();
        solidFillLayerHandler.Properties._PrimaryColor = CommonColorUtils.FastColor(255, 255, 255, 24);
        Layers.Add(new("Background", solidFillLayerHandler));
    }

    public override void Reset()
    {
        base.Reset();
        var solidFillLayerHandler = new SolidFillLayerHandler();
        solidFillLayerHandler.Properties._PrimaryColor = CommonColorUtils.FastColor(255, 255, 255, 24);
        Layers = new ObservableCollection<Layer>
        {
            new("Chroma Lighting", new RazerLayerHandler()),
            new("Background", solidFillLayerHandler),
        };
    }
}