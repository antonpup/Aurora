using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aurora.Utils
{
    public static class JSONUtils
    {
        private static AuroraSerializationBinder _serialBinder = new AuroraSerializationBinder();

        public static AuroraSerializationBinder SerializationBinder 
        {
            get
            {
                return _serialBinder;
            }
        }
    }

    public class AuroraSerializationBinder : DefaultSerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            switch (typeName)
            {
                case "System.Collections.Generic.List`1[[System.Drawing.Color, System.Drawing]]":
                    return typeof(List<System.Drawing.Color>);
                case "System.Collections.Generic.SortedDictionary`2[[System.Single, mscorlib],[System.Drawing.Color, System.Drawing]]":
                    return typeof(SortedDictionary<float, System.Drawing.Color>);
                case "System.Collections.Generic.Queue`1[[System.Windows.Forms.Keys, System.Windows.Forms]]":
                    return typeof(Queue<System.Windows.Forms.Keys>);
                case "System.Collections.Generic.Dictionary`2[[Aurora.Devices.DeviceKeys, Aurora],[System.Drawing.Color, System.Drawing]]":
                    return typeof(Dictionary<Devices.DeviceKeys, System.Drawing.Color>);
                    //Resolve typo'd AbilityLayerHandler type
                case "Aurora.Profiles.Dota_2.Layers.Dota2AbiltiyLayerHandler":
                    return typeof(Profiles.Dota_2.Layers.Dota2AbilityLayerHandler);
                case "Aurora.Profiles.Dota_2.Layers.Dota2HeroAbiltiyEffectsLayerHandler":
                    return typeof(Profiles.Dota_2.Layers.Dota2HeroAbilityEffectsLayerHandler);
                case "Aurora.Profiles.Dota_2.Layers.Dota2HeroAbiltiyEffectsLayerHandlerProperties":
                    return typeof(Profiles.Dota_2.Layers.Dota2HeroAbilityEffectsLayerHandlerProperties);
                case "Aurora.Profiles.TheDivision.TheDivisionSettings":
                    return typeof(Settings.ApplicationProfile);
                case "Aurora.Profiles.Overwatch.OverwatchProfile":
                case "Aurora.Profiles.WormsWMD.WormsWMDProfile":
                case "Aurora.Profiles.Blade_and_Soul.BnSProfile":
                case "Aurora.Profiles.Magic_Duels_2012.MagicDuels2012Profile":
                case "Aurora.Profiles.ColorEnhanceProfile":
                    return typeof(Profiles.WrapperProfile);
                case "Aurora.Devices.SteelSeriesHID.SteelSeriesHIDDevice":
                    return typeof(Aurora.Devices.UnifiedHID.UnifiedHIDDevice);
                default:
                    if (!typeName.Contains("Overlays") && new Regex(@"Aurora.Profiles.\w+.\w+Settings").IsMatch(typeName))
                        return base.BindToType(assemblyName, typeName.Replace("Settings", "Profile"));
                    return base.BindToType(assemblyName, typeName);
            }
        }
    }
}
