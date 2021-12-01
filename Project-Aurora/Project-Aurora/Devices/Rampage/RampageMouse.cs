using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using HidSharp;
using HidSharp.Reports.Encodings;

namespace Aurora.Devices.Rampage
{
    public class RampageMouse : IDisposable
    {
        private const int VendorId = 0x258A;
        private const int ProductId = 0x1007;
        private static readonly byte[] ColorPacketHeader = new byte[23] { 0x04, 0x00, 0x06, 0xb0, 0x01, 0x09, 0x13, 0x1d, 0x27, 0x37, 0x3b, 0x81, 0x81, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x62, 0x00 };
        private static readonly byte[] ColorPacketFooter = new byte[15] { 0x01, 0x03, 0x02, 0x06, 0x05, 0x04, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        private readonly byte[] _keyColors = new byte[7*3];

        private readonly HidStream _ctrlStream;

        private RampageMouse(HidStream ctrlStream)
        {
            _ctrlStream = ctrlStream;
        }


        public static RampageMouse Initialize()
        {
            var devices = DeviceList.Local.GetHidDevices(VendorId, ProductId); //Find device with given VID PID

            if (!devices.Any())
            {
                return null;
            }
            try
            {
                HidDevice ctrlDevice = devices.First(d => d.GetMaxFeatureReportLength() > 50);

                HidStream ctrlStream = null;
                if (ctrlDevice?.TryOpen(out ctrlStream) ?? false)
                {
                    RampageMouse rm = new RampageMouse(ctrlStream);
                    return rm;
                }
                else
                {
                    ctrlStream?.Close();
                }
            }
            catch(Exception e)
            {
                return null;
            }
            return null;
        }

        #region Set Colors
        /// <summary>
        /// All packets sent to the mouse which invole anything to do with rgb are 64 bytes long.
        /// First Packet of rgb data transfer starts with 07030601 and next bytes are empty.
        /// Next Packets contain actual rgb data and start with 0703060200000000
        /// </summary>
        public void SetColors(Dictionary<RampageKey, Color> keyColors) 
        {
            foreach (var key in keyColors)
                SetKeyColor(key.Key, key.Value);
        }

        public void SetColor(Color clr) //Set Color to every key on keyboard
        {
            foreach (RampageKey key in (RampageKey[])Enum.GetValues(typeof(RampageKey))) 
                SetKeyColor(key, clr);
        }

        public void SetKeyColor(RampageKey key, Color clr) //Puts data for each key into the byte array _keyColors
        {
            int offset = (int)key*3;
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
                Array.Copy(_keyColors, 0, packet, ColorPacketHeader.Length, _keyColors.Length); //Copies rgb bytes to the packet
                ColorPacketFooter.CopyTo(packet, ColorPacketHeader.Length + _keyColors.Length);
                _ctrlStream.SetFeature(packet); //Sends packet as additional data in SetReport USBHID packet
                //_ctrlStream.SetFeature(endp1); //Sends packet as additional data in SetReport USBHID packet
                return true;
            }
            catch(Exception e)
            {
               Disconnect();
               return false;
            }
        }
        #endregion

        public void Disconnect()
        {
            _ctrlStream?.Close();
        }

        #region IDisposable Support
        /// <summary>
        /// Disconnects the mouse when disposing
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// </summary>
        protected virtual void Dispose(bool b)
        {
            Disconnect();
        }

        #endregion
    }
}