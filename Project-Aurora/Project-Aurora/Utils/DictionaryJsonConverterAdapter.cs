using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Utils;

public class ConcurrentDictionaryJsonConverterAdapter : JsonConverter<ConcurrentDictionary<object, object>>
{
    
    public override void WriteJson(JsonWriter writer, ConcurrentDictionary<object, object> value, JsonSerializer serializer)
    {
        serializer.Serialize(writer,value);
    }

    public override ConcurrentDictionary<object, object> ReadJson(JsonReader reader, Type objectType, ConcurrentDictionary<object, object>? existingValue,
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

public class SortedDictionaryAdapter : JsonConverter<SortedDictionary<double, Color>>
{
    
    public override void WriteJson(JsonWriter writer, SortedDictionary<double, Color> value, JsonSerializer serializer)
    {
        serializer.Serialize(writer,value);
    }

    public override SortedDictionary<double, Color> ReadJson(JsonReader reader, Type objectType, SortedDictionary<double, Color>? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var genericTypes = objectType.GenericTypeArguments;
        var keyType = genericTypes[0];
        var valueType = genericTypes[1];

        var map = existingValue ?? new SortedDictionary<double, Color>();
        map.Clear();
            
        var item = serializer.Deserialize<JObject>(reader);
        foreach (var prop in item.Children<JProperty>())
        {
            if (prop.Name.Equals("$type"))
            {
                continue;
            }

            var key = Convert.ChangeType(prop.Name, keyType, CultureInfo.InvariantCulture);
            var value = serializer.Deserialize(prop.Value.CreateReader(), valueType);
            map.TryAdd((double)key, (Color)value);
        }
        return map;
    }
}

public class DictionaryJsonConverterAdapter : JsonConverter<IDictionary<dynamic, dynamic>>
{
    
    public override void WriteJson(JsonWriter writer, IDictionary<object, object> value, JsonSerializer serializer)
    {
        serializer.Serialize(writer,value);
    }

    public override IDictionary<dynamic, dynamic> ReadJson(JsonReader reader, Type objectType, IDictionary<dynamic, dynamic>? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var genericTypes = objectType.GenericTypeArguments;
        var keyType = genericTypes[0];
        var valueType = genericTypes[1];

        var map = existingValue ?? (objectType.IsAbstract ?  new Dictionary<dynamic, dynamic>() : Instance(objectType));
            
        var item = serializer.Deserialize<JObject>(reader);
        foreach (var prop in item.Children<JProperty>())
        {
            if (prop.Name.Equals("$type"))
            {
                continue;
            }

            var key = Convert.ChangeType(prop.Name, keyType, CultureInfo.InvariantCulture);
            if (keyType == typeof(Double))
            {
                double.TryParse(prop.Name, out var doubleKey);
                key = doubleKey;
            }
            var value = serializer.Deserialize(prop.Value.CreateReader(), valueType);
            map.TryAdd(key, value);
        }
        return map;
    }

    private static IDictionary<dynamic, dynamic> Instance(Type objectType)
    {
        return (IDictionary<dynamic, dynamic>)Expression.New(objectType.GetConstructor(Type.EmptyTypes));
    }
}