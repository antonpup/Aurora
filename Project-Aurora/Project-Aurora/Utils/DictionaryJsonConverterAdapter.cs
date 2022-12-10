using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Utils;

public class ConcurrentDictionaryJsonConverterAdapter : JsonConverter<ConcurrentDictionary<object, object>>
{
    
    public override void WriteJson(JsonWriter writer, ConcurrentDictionary<object, object> value, JsonSerializer serializer)
    {
        serializer.Serialize(writer,value);
    }

    public override ConcurrentDictionary<object, object> ReadJson(JsonReader reader, Type objectType, ConcurrentDictionary<object, object> existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var genericTypes = objectType.GenericTypeArguments;
        var keyType = genericTypes[0];
        var valueType = genericTypes[1];

        var map = existingValue ?? new ConcurrentDictionary<object, object>();
            
        var item = serializer.Deserialize<JObject>(reader);
        foreach (var prop in item.Children<JProperty>())
        {
            if (prop.Name.Equals("$type"))
            {
                continue;
            }

            var key = Convert.ChangeType(prop.Name, keyType, CultureInfo.InvariantCulture);
            var value = serializer.Deserialize(prop.Value.CreateReader(), valueType);
            map.TryAdd(key, value);
        }
        return map;
    }
}

public class DictionaryJsonConverterAdapter : JsonConverter<IDictionary<object, object>>
{
    
    public override void WriteJson(JsonWriter writer, IDictionary<object, object> value, JsonSerializer serializer)
    {
        serializer.Serialize(writer,value);
    }

    public override IDictionary<object, object> ReadJson(JsonReader reader, Type objectType, IDictionary<object, object> existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var genericTypes = objectType.GenericTypeArguments;
        var keyType = genericTypes[0];
        var valueType = genericTypes[1];

        var map = existingValue ?? new Dictionary<object, object>();
            
        var item = serializer.Deserialize<JObject>(reader);
        foreach (var prop in item.Children<JProperty>())
        {
            if (prop.Name.Equals("$type"))
            {
                continue;
            }

            var key = Convert.ChangeType(prop.Name, keyType, CultureInfo.InvariantCulture);
            var value = serializer.Deserialize(prop.Value.CreateReader(), valueType);
            map.TryAdd(key, value);
        }
        return map;
    }
}