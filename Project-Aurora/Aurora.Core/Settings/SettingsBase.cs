using Newtonsoft.Json;
using System;

namespace Aurora.Core.Settings
{
    public abstract class SettingsBase : NotifyPropertyChangedEx, ICloneable
    {
        public object Clone()
        {
            string str = JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }); //, Binder = Aurora.Utils.JSONUtils.SerializationBinder

            return JsonConvert.DeserializeObject(
                    str,
                    this.GetType(),
                    new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace, TypeNameHandling = TypeNameHandling.All } //, Binder = Aurora.Utils.JSONUtils.SerializationBinder
                    );
        }

        public SettingsBase()
        {
        }
    }
}
