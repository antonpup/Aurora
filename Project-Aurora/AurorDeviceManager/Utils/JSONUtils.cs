using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Color = System.Drawing.Color;

namespace AurorDeviceManager.Utils;

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