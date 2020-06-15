using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Aurora.Settings.Overrides.Logic;

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
                    return typeof(List<Color>);
                case "System.Collections.Generic.SortedDictionary`2[[System.Single, mscorlib],[System.Drawing.Color, System.Drawing]]":
                    return typeof(SortedDictionary<float, Color>);
                case "System.Collections.Generic.Queue`1[[System.Windows.Forms.Keys, System.Windows.Forms]]":
                    return typeof(Queue<System.Windows.Forms.Keys>);
                case "System.Collections.Generic.Dictionary`2[[Aurora.Devices.DeviceKeys, Aurora],[System.Drawing.Color, System.Drawing]]":
                    return typeof(Dictionary<Devices.DeviceKeys, Color>);
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
                    return typeof(Devices.UnifiedHID.UnifiedHIDDevice);
                case "Aurora.Settings.Overrides.Logic.IEvaluatableBoolean":
                case "Aurora.Settings.Overrides.Logic.IEvaluatable`1[[System.Boolean, mscorlib]]":
                    return typeof(Evaluatable<bool>);
                case "Aurora.Settings.Overrides.Logic.IEvaluatableNumber":
                case "Aurora.Settings.Overrides.Logic.IEvaluatable`1[[System.Double, mscorlib]]":
                    return typeof(Evaluatable<double>);
                case "Aurora.Settings.Overrides.Logic.IEvaluatableString":
                case "Aurora.Settings.Overrides.Logic.IEvaluatable`1[[System.String, mscorlib]]":
                    return typeof(Evaluatable<string>);
                case "System.Collections.ObjectModel.ObservableCollection`1[[Aurora.Settings.Overrides.Logic.IEvaluatableBoolean, Aurora]]":
                case "System.Collections.ObjectModel.ObservableCollection`1[[Aurora.Settings.Overrides.Logic.IEvaluatable`1[[System.Boolean, mscorlib]], Aurora]]":
                    return typeof(ObservableCollection<Evaluatable<bool>>);
                case "System.Collections.ObjectModel.ObservableCollection`1[[Aurora.Settings.Overrides.Logic.IEvaluatableNumber, Aurora]]":
                case "System.Collections.ObjectModel.ObservableCollection`1[[Aurora.Settings.Overrides.Logic.IEvaluatable`1[[System.Double, mscorlib]], Aurora]]":
                    return typeof(ObservableCollection<Evaluatable<double>>);
                case "System.Collections.ObjectModel.ObservableCollection`1[[Aurora.Settings.Overrides.Logic.IEvaluatableString, Aurora]]":
                case "System.Collections.ObjectModel.ObservableCollection`1[[Aurora.Settings.Overrides.Logic.IEvaluatable`1[[System.String, mscorlib]], Aurora]]":
                    return typeof(ObservableCollection<Evaluatable<string>>);
                case "Aurora.Settings.Overrides.Logic.Boolean.Boolean_Latch":
                    return typeof(Settings.Overrides.Logic.Boolean.Boolean_FlipFlopSR);
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
            // If this is a null token, then the original value was null.
            if (reader.TokenType == JsonToken.Null) return null;

            // Read "$type" property
            reader.Read(); // Property name
            reader.Read(); // Property value
            var typeName = reader.Value.ToString(); // Find the type based on the fully-qualified name from the $type. This will be null if the original value was null

            // Read "$value" property
            reader.Read(); // Property name
            reader.Read(); // Property value
            var value = reader.Value.ToString();

            // Read end of object '}' (if this is not done, it breaks the next converter)
            reader.Read();

            return JsonConvert.DeserializeObject(value.ToString(), Type.GetType(typeName)); // The $value is a JSON-encoded string, so decode as the requested type;
        }
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            writer.WriteStartObject();
            writer.WritePropertyName("$type"); writer.WriteValue(value.GetType().AssemblyQualifiedName);
            writer.WritePropertyName("$value"); writer.WriteValue(JsonConvert.SerializeObject(value));
            writer.WriteEndObject();
        }
    }
}
