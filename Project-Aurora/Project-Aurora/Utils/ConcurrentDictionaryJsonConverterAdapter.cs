using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Utils
{
    public class ConcurrentDictionaryJsonConverterAdapter<K, V> : JsonConverter<ConcurrentDictionary<K, V>>
    {
        public override void WriteJson(JsonWriter writer, ConcurrentDictionary<K, V> value, JsonSerializer serializer)
        {
            serializer.Serialize(writer,value);
        }

        public override ConcurrentDictionary<K, V> ReadJson(JsonReader reader, Type objectType, ConcurrentDictionary<K, V> existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var map = new ConcurrentDictionary<K, V>();
            
            var item = serializer.Deserialize<JObject>(reader);
            foreach (var prop in item.Children<JProperty>())
            {
                if (prop.Name.Equals("$type"))
                {
                    continue;
                }
                map.TryAdd((K)Convert.ChangeType(prop.Name, typeof(K), CultureInfo.InvariantCulture), serializer.Deserialize<V>(prop.Value.CreateReader()));
            }
            return map;
        }
    }
}