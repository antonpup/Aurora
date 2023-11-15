using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Serialization;

namespace Aurora.Utils;

public static class TypeExtensions
{
    public static object TryClone(this object self, bool deep = false, ISerializationBinder? binder = null)
    {
        if (self is ICloneable o)
            return o.Clone();
        if (deep) {
            var settings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                TypeNameHandling = TypeNameHandling.All,
                SerializationBinder = binder ?? new AuroraSerializationBinder()
            };
            var json = JsonConvert.SerializeObject(self, Formatting.None, settings);
            return JsonConvert.DeserializeObject(json, self.GetType(), settings);
        }
        return self;
    }
}