using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Utils;

public class ConcurrentListJsonConverterAdapter<T> : JsonConverter<ObservableCollection<T>>
{
    public override void WriteJson(JsonWriter writer, ObservableCollection<T> value, JsonSerializer serializer)
    {
        serializer.Serialize(writer,value);
    }

    public override ObservableCollection<T> ReadJson(JsonReader reader, Type objectType, ObservableCollection<T> existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        if (!hasExistingValue)
        {
            existingValue = new ObservableCollection<T>();
        }
        
        switch (reader.TokenType)
        {
            case JsonToken.StartArray:
                var array = serializer.Deserialize<JArray>(reader);
                foreach (var prop in array.Children<JProperty>())
                {
                    if (prop.Name.Equals("$type"))
                    {
                        continue;
                    }

                    foreach (var jToken in array.Children())
                    {
                        existingValue.Add(serializer.Deserialize<T>(jToken.CreateReader()));
                    }
                }
                break;
            case JsonToken.StartObject:
                var item = serializer.Deserialize<JObject>(reader);
                foreach (var prop in item.Children<JProperty>())
                {
                    if (prop.Name.Equals("$type"))
                    {
                        continue;
                    }

                    if (prop.Name.Equals("$values"))
                    {
                        foreach (var jToken in prop.Value.Children())
                        {
                            var deserialize = serializer.Deserialize<T>(jToken.CreateReader());
                            existingValue.Add(deserialize);
                        }
                    }

                }
                break;
        }
        return existingValue;
    }
}