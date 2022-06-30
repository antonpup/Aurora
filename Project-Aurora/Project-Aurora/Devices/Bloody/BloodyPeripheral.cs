using HidSharp;
using HidSharp.Reports.Encodings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices.Bloody
{
    public sealed class BloodyPeripheral : IDisposable
    {
        private static readonly Dictionary<int, PeripheralType> DeviceIds = new()
        {
            [0x37EA] = PeripheralType.MOUSE,
            [0xFA60] = PeripheralType.MOUSEPAD,
            [0x356E] = PeripheralType.MOUSEPAD,
        };
        private static readonly byte[] ColorPacketHeader = { 0x07, 0x03, 0x06, 0x02, 0x00, 0x00, 0x00, 0x00 };

        public PeripheralType PeripheralType => _peripheralType;

        private const int VendorId = 0x09DA;
        private const uint LedUsagePage = 0x000C; //(ff52,0001,000C) //List of UsagePage+UsageID I found from Windows Device Manager(labed there as Uxxxx&&UPxxxx)
        private const uint LedUsage = 0x0001; //(0244,/0080,0001)

        private readonly PeripheralType _peripheralType;

        private readonly byte[] _keyColors = new byte[16 * 3];

        private readonly HidStream _ctrlStream;

        private BloodyPeripheral(HidStream ctrlStream, PeripheralType peripheralType)
        {
            _ctrlStream = ctrlStream;
            _peripheralType = peripheralType;
        }

        public override string ToString()
        {
            return _peripheralType.ToString();
        }

        public static List<BloodyPeripheral> GetDevices()
        {
            List<BloodyPeripheral> devices = new List<BloodyPeripheral>();
            foreach(int productId in DeviceIds.Keys)
            {
                var dev = Initialize(productId);
                if (dev != null)
                    devices.Add(dev);
            }
            return devices;
        }
        public static BloodyPeripheral Initialize(int productId)
        {
            var devices = DeviceList.Local.GetHidDevices(vendorID: VendorId, productID: productId); //Find device with given VID PID

            if (!devices.Any())
            {
                return null;
            }
            try
            {
                GetFromUsages(devices, LedUsagePage, LedUsage);
                HidDevice ctrlDevice = devices.First(d => d.GetMaxFeatureReportLength() > 50);

                HidStream ctrlStream = null;
                if ((bool) ctrlDevice?.TryOpen(out ctrlStream))
                {
                    PeripheralType type;
                    DeviceIds.TryGetValue(productId, out type);
                    BloodyPeripheral bp = new BloodyPeripheral(ctrlStream, type);
                    bp.SetDirect();
                    return bp;
                }
                ctrlStream?.Close();
            }
            catch
            { }
            return null;
        }

        private void SetDirect()
        {
            byte[] a = { 0x07, 0x03, 0x06, 0x01 };
            byte[] b = { 0x07, 0x03, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };

            var packet = new byte[64];
            try
            {
                a.CopyTo(packet, 0);
                _ctrlStream.SetFeature(packet);
                b.CopyTo(packet, 0);
                _ctrlStream.SetFeature(packet);
            }
            catch
            {
                Disconnect();
            }
        }

        #region Set Colors
        /// <summary>
        /// All packets sent to the mouse which invole anything to do with rgb are 64 bytes long.
        /// First Packet of rgb data transfer starts with 07030601 and next bytes are empty.
        /// Next Packets contain actual rgb data and start with 0703060200000000
        /// </summary>
        public void SetColors(Dictionary<BloodyPeripheralLed, Color> keyColors)
        {
            foreach (var key in keyColors)
                SetKeyColor(key.Key, key.Value);
        }

        public void SetColor(Color clr) //Set Color to every key on keyboard
        {
            foreach (BloodyPeripheralLed key in (BloodyPeripheralLed[])Enum.GetValues(typeof(BloodyPeripheralLed)))
                SetKeyColor(key, clr);
        }

        public void SetKeyColor(BloodyPeripheralLed key, Color clr) //Puts data for each key into the byte array _keyColors
        {
            int offset = (int)key * 3;
            _keyColors[offset + 0] = clr.R;
            _keyColors[offset + 1] = clr.G;
            _keyColors[offset + 2] = clr.B;
        }
        public bool Update() => WriteColorBuffer();
        private bool WriteColorBuffer()
        {
            byte[] packet = new byte[64];
            try
            {
                ColorPacketHeader.CopyTo(packet, 0); //Copies Color Header to packet
                Array.Copy(_keyColors, 0, packet, 8, 16 * 3); //Copies rgb bytes to the packet
                _ctrlStream.SetFeature(packet); //Sends packet as additional data in SetReport USBHID packet
                return true;
            }
            catch (Exception e)
            {
                Disconnect();
                return false;
            }
        }
        #endregion
        private static HidDevice GetFromUsages(IEnumerable<HidDevice> devices, uint usagePage, uint usage)
        {
            foreach (var dev in devices) //For each dev in devices under Vid Pid get dev with right UsageID and UsagePage, if found return it.
            {
                try
                {
                    var raw = dev.GetRawReportDescriptor();
                    var usages = EncodedItem.DecodeItems(raw, 0, raw.Length).Where(t => t.TagForGlobal == GlobalItemTag.UsagePage);
                    if (usages.Any(g => g.ItemType == ItemType.Global && g.DataValue == usagePage))
                    {
                        if (usages.Any(l => l.ItemType == ItemType.Local && l.DataValue == usage))
                        {
                            return dev;
                        }
                    }
                }
                catch
                {
                    //failed to get the report descriptor, skip
                }
            }
            return null;
        }

        public void Disconnect()
        {
            _ctrlStream?.Close();
        }

        #region IDisposable Support
        /// <summary>
        /// Disconnects the mouse when disposing
        /// </summary>
        public void Dispose() => Disconnect();
        #endregion
    }

    public enum PeripheralType
    {
        MOUSE,
        MOUSEPAD,
        KEYBOARD,
    }
}
