using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aurora.Utils
{
    public static class JSONUtils
    {
        public static AuroraSerializationBinder SerializationBinder { get; } = new AuroraSerializationBinder();
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

    /// <summary>
    /// JsonConvert that serialize/deserialize any object and adds a type parameter to it.
    /// <para>This is useful as it allows serialization/deserialization of unknown structs. When (some?/all? idk) structs are serialized, they are
    /// simply converted to a string. This means that the JSON converter would need to look at a class member definition to find out what type of
    /// struct it was and convert it back that way. Unfortunately, this then breaks if the member is defined as an object. For example: if an object
    /// property 'MyProp' was defined and filled with a struct (such as System.Drawing.Color), when deserializing this object would now contain the
    /// string representation of the colour (e.g. "255, 128, 0, 0") instead of an instance the actual color struct. This converter resolves that by
    /// always wrapping the value of the instance in a JSON object that also contains the type of the instance, so even in the case of "object"
    /// proprties, it will be converted properly. Annoyingly this just seems like something the library should've handled itself... ¯\_(ツ)_/¯</para>
    /// </summary>
    public class TypeAnnotatedObjectConverter : JsonConverter {
        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            // TODO: add some sort of error handling, e.g. in case of the targetType being inaccessible?

            reader.Read(); // Read "$type" property name
            reader.Read(); // Read "$type" value
            var targetType = Type.GetType(reader.Value.ToString()); // Find the type based on the fully-qualified name from the $type
            reader.Read(); // Read "$value" property name
            reader.Read(); // Read "$value" value
            var value = JsonConvert.DeserializeObject(reader.Value.ToString(), targetType); // The $value is a JSON-encoded string, so decode as the requested type
            reader.Read(); // Read end of object (if this is not done, it breaks the next converter)

            return value;
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            writer.WriteStartObject();
            writer.WritePropertyName("$type"); writer.WriteValue(value.GetType().AssemblyQualifiedName);
            writer.WritePropertyName("$value"); writer.WriteValue(JsonConvert.SerializeObject(value));
            writer.WriteEndObject();
        }
    }
}
