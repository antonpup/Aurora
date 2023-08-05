using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Linq;
using Aurora.Settings.Overrides.Logic;
using Newtonsoft.Json.Linq;

namespace Aurora.Utils;

public static class JSONUtils
{
    public static AuroraSerializationBinder SerializationBinder { get; } = new();
}

public class AuroraSerializationBinder : DefaultSerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        switch (typeName)
        {
            case "System.Collections.Generic.List`1[[System.Drawing.Color, System.Drawing]]":
                return typeof(List<Color>);
            case
                "System.Collections.Generic.SortedDictionary`2[[System.Single, mscorlib],[System.Drawing.Color, System.Drawing]]"
                :
                return typeof(SortedDictionary<float, Color>);
            case "System.Collections.Generic.Queue`1[[System.Windows.Forms.Keys, System.Windows.Forms]]":
                return typeof(Queue<System.Windows.Forms.Keys>);
            case
                "System.Collections.Generic.Dictionary`2[[Aurora.Devices.DeviceKeys, Aurora],[System.Drawing.Color, System.Drawing]]"
                :
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
            case "Aurora.Devices.Corsair.CorsairDevice":
                return typeof(Devices.RGBNet.CorsairRgbNetDevice);
            case "Aurora.Settings.Overrides.Logic.IEvaluatableBoolean":
            case "Aurora.Settings.Overrides.Logic.IEvaluatable`1[[System.Boolean, mscorlib]]":
                return typeof(Evaluatable<bool>);
            case "Aurora.Settings.Overrides.Logic.IEvaluatableNumber":
            case "Aurora.Settings.Overrides.Logic.IEvaluatable`1[[System.Double, mscorlib]]":
                return typeof(Evaluatable<double>);
            case "Aurora.Settings.Overrides.Logic.IEvaluatableString":
            case "Aurora.Settings.Overrides.Logic.IEvaluatable`1[[System.String, mscorlib]]":
                return typeof(Evaluatable<string>);
            case
                "System.Collections.ObjectModel.ObservableCollection`1[[Aurora.Settings.Overrides.Logic.IEvaluatableBoolean, Aurora]]"
                :
            case
                "System.Collections.ObjectModel.ObservableCollection`1[[Aurora.Settings.Overrides.Logic.IEvaluatable`1[[System.Boolean, mscorlib]], Aurora]]"
                :
                return typeof(ObservableCollection<Evaluatable<bool>>);
            case
                "System.Collections.ObjectModel.ObservableCollection`1[[Aurora.Settings.Overrides.Logic.IEvaluatableNumber, Aurora]]"
                :
            case
                "System.Collections.ObjectModel.ObservableCollection`1[[Aurora.Settings.Overrides.Logic.IEvaluatable`1[[System.Double, mscorlib]], Aurora]]"
                :
                return typeof(ObservableCollection<Evaluatable<double>>);
            case
                "System.Collections.ObjectModel.ObservableCollection`1[[Aurora.Settings.Overrides.Logic.IEvaluatableString, Aurora]]"
                :
            case
                "System.Collections.ObjectModel.ObservableCollection`1[[Aurora.Settings.Overrides.Logic.IEvaluatable`1[[System.String, mscorlib]], Aurora]]"
                :
                return typeof(ObservableCollection<Evaluatable<string>>);
            case "Aurora.Settings.Overrides.Logic.Boolean.Boolean_Latch":
                return typeof(Settings.Overrides.Logic.Boolean.Boolean_FlipFlopSR);
            case "System.Drawing.Color":
                return typeof(System.Drawing.Color);
            default:
                if (!typeName.Contains("Overlays") && new Regex(@"Aurora.Profiles.\w+.\w+Settings").IsMatch(typeName))
                    return base.BindToType(assemblyName, typeName.Replace("Settings", "Profile"));
                return base.BindToType(assemblyName, typeName);
        }
    }
}

public class EnumConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType.IsEnum;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("$type");
        writer.WriteValue(value.GetType().AssemblyQualifiedName);
        writer.WritePropertyName("$value");
        serializer.Serialize(writer, value);
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // If this is a null token, then the original value was null.
        var readerTokenType = reader.TokenType;
        switch (readerTokenType)
        {
            case JsonToken.Null:
                return null;
            case JsonToken.Integer:
                return Enum.ToObject(objectType, reader.Value);
            case JsonToken.StartObject:
                var item = serializer.Deserialize<JObject>(reader);
                var typeName = item["$type"].Value<string>();
                return JsonConvert.DeserializeObject(item["$value"].ToString(), Type.GetType(typeName));
        }

        return existingValue;
    }
}

public class OverrideTypeConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType.FullName == typeof(Type).FullName;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("$type");
        writer.WriteValue(value.GetType().AssemblyQualifiedName);
        writer.WritePropertyName("$value");
        serializer.Serialize(writer, value);
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // If this is a null token, then the original value was null.
        var readerTokenType = reader.TokenType;
        switch (readerTokenType)
        {
            case JsonToken.String:
                return Type.GetType(reader.Value.ToString());
            case JsonToken.StartObject:
                var item = serializer.Deserialize<JObject>(reader);
                foreach (var prop in item.Children<JProperty>())
                {
                    switch (prop.Name)
                    {
                        case "$type":
                            return Type.GetType(prop.Value.ToString());
                    }
                }

                break;
            case JsonToken.Null:
                return objectType;
            default:
                return objectType;
        }

        return existingValue;
    }
}

public class TypeAnnotatedObjectConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType.FullName == typeof(Color).FullName ||
                                                        objectType.IsAssignableFrom(typeof(object));

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("$type");
        writer.WriteValue(value.GetType().AssemblyQualifiedName);
        writer.WritePropertyName("$value");
        serializer.Serialize(writer, value);
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // If this is a null token, then the original value was null.
        var readerTokenType = reader.TokenType;
        switch (readerTokenType)
        {
            case JsonToken.String:
                return reader.Value.ToString().StartsWith("\"")
                    ? JsonConvert.DeserializeObject(reader.Value.ToString(), objectType)
                    : JsonConvert.DeserializeObject("\"" + reader.Value + "\"", objectType);
            case JsonToken.StartObject:
                var item = serializer.Deserialize<JObject>(reader);
                var type = serializer.Deserialize<Type>(item["$type"].CreateReader());
                var value = item["$value"];
                if (value == null)
                {
                    return serializer.Deserialize(item.CreateReader(), type);
                }

                var valueReader = value.CreateReader();
                switch (valueReader.TokenType)
                {
                    case JsonToken.StartObject:
                        return serializer.Deserialize(valueReader, type);
                    case JsonToken.String:
                    default:
                        var s = value.ToString();
                        if (objectType.FullName != typeof(Color).FullName && type?.FullName != typeof(Color).FullName)
                            return JsonConvert.DeserializeObject(s, type);
                        if (s.StartsWith("\""))
                        {
                            return JsonConvert.DeserializeObject(s, type);
                        }

                        return JsonConvert.DeserializeObject("\"" + value + "\"", type);
                }
            case JsonToken.Boolean:
                return reader.Value;
            case JsonToken.Null:
                return existingValue;
        }

        return JsonConvert.DeserializeObject(reader.Value.ToString(), objectType);
    }
}

public class SingleToDoubleConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Double);

    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException("Unnecessary because CanWrite is false. The type will skip the converter.");
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.Float:
            case JsonToken.Integer:
                return (double)reader.Value;
            case JsonToken.String:

                double.TryParse(reader.ReadAsString(), out var value);
                return value;
        }

        throw new NotImplementedException("Unnecessary because CanWrite is false. The type will skip the converter.");
    }
}