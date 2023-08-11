using HidSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

        private static BloodyPeripheral Initialize(int productId)
        {
            var devices = DeviceList.Local.GetHidDevices(vendorID: VendorId, productID: productId); //Find device with given VID PID

            var hidDevices = devices.ToList();
            if (!hidDevices.Any())
            {
                return null;
            }
            try
            {
                HidDevice ctrlDevice = hidDevices.First(d => d.GetMaxFeatureReportLength() > 50);

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

        public void SetKeyColor(BloodyPeripheralLed key, Color clr) //Puts data for each key into the byte array _keyColors
        {
            int offset = (int)key * 3;
            _keyColors[offset + 0] = clr.R;
            _keyColors[offset + 1] = clr.G;
            _keyColors[offset + 2] = clr.B;
        }
        public void Update()
        {
            WriteColorBuffer();
        }

        private void WriteColorBuffer()
        {
            byte[] packet = new byte[64];
            try
            {
                ColorPacketHeader.CopyTo(packet, 0); //Copies Color Header to packet
                Array.Copy(_keyColors, 0, packet, 8, 16 * 3); //Copies rgb bytes to the packet
                _ctrlStream.SetFeature(packet); //Sends packet as additional data in SetReport USBHID packet
            }
            catch (Exception e)
            {
                Disconnect();
            }
        }
        #endregion

        public void Disconnect()
        {
            _ctrlStream?.Close();
            _ctrlStream?.Dispose();
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
