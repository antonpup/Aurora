using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.ObjectModel;
using Aurora.Utils;

namespace Aurora.Profiles.Logitech;

public class LogitechProfile : ApplicationProfile
{
    public override void Reset()
    {
        base.Reset();
        var solidFillLayerHandler = new SolidFillLayerHandler();
        solidFillLayerHandler.Properties._PrimaryColor = ColorUtils.FastColor(255, 255, 255, 24);
        Layers = new ObservableCollection<Layer>
        {
            new("Logitech Lightsync", new LogitechLayerHandler()),
        };
    }
}