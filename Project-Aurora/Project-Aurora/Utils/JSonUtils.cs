using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils
{
    public static class JSONUtils
    {
        private static AuroraSerializationBinder _serialBinder = new AuroraSerializationBinder();

        public static AuroraSerializationBinder SerializationBinder 
        {
            get
            {
                return _serialBinder;
            }
        }
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
                default:
                    return base.BindToType(assemblyName, typeName);
            }

        }
    }
}
