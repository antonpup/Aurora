using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LEDINT = System.Int16;

namespace Aurora.Devices.Layout
{
    //TODO: Need to make this so that it gets saved as a number rather than an object in json
    //[JsonConverter(typeof(DeviceLEDConverter))]
    public struct DeviceLED
    {
        public static readonly DeviceLED None = new DeviceLED(0, 0, LEDINT.MinValue);
        public static readonly DeviceLED Global = new DeviceLED(0, 0, 0);

        [JsonProperty("type")]
        public byte DeviceTypeID { get; set; }
        [JsonProperty("id")]
        public byte DeviceID { get; set; }
        [JsonProperty("led")]
        public LEDINT LedID { get; set; }
        
        public DeviceLED(byte deviceTypeID, byte deviceID, LEDINT ledID)
        {
            this.DeviceTypeID = deviceTypeID;
            this.DeviceID = deviceID;
            this.LedID = ledID;
        }

        public DeviceLED(uint encoded)
        {
            //Encoded by first byte = DeviceTypeID
            //          second byte = DeviceID
            //          remaining 2 = LedID
            // (left -> right little-endian)

            this.DeviceTypeID = (byte)(encoded >> 24);
            this.DeviceID = (byte)((encoded >> 16) & 0xFF);
            this.LedID = (LEDINT)(encoded & 0xFFFF);
        }

        public uint Encode()
        {
            uint ret = 0;
            //Insert DeviceTypeID at the start then move it along
            ret ^= this.DeviceTypeID;
            ret <<= 8;

            //Insert DeviceID at the start then move it along
            ret ^= this.DeviceID;
            ret <<= 8;

            //Insert LedID at start
            ret ^= (ushort)this.LedID;

            return ret;
        }

        public (byte type, byte id) GetLookupKey()
        {
            return (type: this.DeviceTypeID, id: this.DeviceID);
        }

        public string GetName()
        {
            return GlobalDeviceLayout.Instance.GetDeviceLEDName(this);
        }

        public DeviceLED Sanitize()
        {
            return GlobalDeviceLayout.Instance.SanitizeDeviceLED(this);
        }

        [JsonIgnore]
        public bool IsNone => this.Equals(None);

        public static bool operator ==(DeviceLED lhs, DeviceLED rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(DeviceLED lhs, DeviceLED rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override bool Equals(object obj)
        {
            if (obj is DeviceLED led)
            {
                return this.DeviceTypeID.Equals(led.DeviceTypeID)
                    && this.DeviceID.Equals(led.DeviceID)
                    && this.LedID.Equals(led.LedID);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int)Encode();
        }
    }

    internal class DeviceLEDConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.Equals(typeof(DeviceLED));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            int? val = reader.ReadAsInt32();
            if (val != null)
            {
                return new DeviceLED((uint)val);
            }
            else
            {
                //throw new Exception();
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((DeviceLED)value).Encode());
        }
    }
}
