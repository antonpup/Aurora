using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Devices.AtmoOrbDevice
{
    public class AtmoOrbDevice : Device
    {
        private string devicename = "AtmoOrb";
        private Socket socket;
        private IPEndPoint ipClientEndpoint;
        private bool isConnected;
        private bool isConnecting;
        private Stopwatch sw = new Stopwatch();

        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        private VariableRegistry default_registry = null;

        public string GetDeviceDetails()
        {
            if (isConnected)
                return devicename + ": Connected";
            else
                return devicename + ": Not connected";
        }

        public string GetDeviceName()
        {
            return devicename;
        }

        public bool Initialize()
        {
            if (!isConnected)
            {
                try
                {
                    Connect();
                }
                catch (Exception exc)
                {
                    Global.logger.Error($"Device {devicename} encountered an error during Connecting. Exception: {exc}");
                    isConnected = false;

                    return false;
                }
            }

            return isConnected;
        }

        public bool IsConnected()
        {
            return isConnected;
        }

        public bool IsInitialized()
        {
            return IsConnected();
        }

        public bool IsKeyboardConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsPeripheralConnected()
        {
            throw new NotImplementedException();
        }

        public bool Reconnect()
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
                ipClientEndpoint = null;
            }

            isConnected = false;

            Connect();
            return true;
        }

        public void Reset()
        {
            Reconnect();
        }

        public void Shutdown()
        {
            if (socket != null)
            {
                // Set color to black
                SendColorsToOrb(0, 0, 0);

                // Close all connections
                socket.Close();
                socket = null;
                ipClientEndpoint = null;
            }

            isConnected = false;

            if (sw.IsRunning)
                sw.Stop();
        }

        public void Connect(DoWorkEventArgs token = null)
        {
            try
            {
                if (isConnecting)
                    return;

                isConnecting = true;
                var multiCastIp = IPAddress.Parse("239.15.18.2");
                var port = 49692;

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                ipClientEndpoint = new IPEndPoint(multiCastIp, port);
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                    new MulticastOption(multiCastIp));
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);

                socket.Connect(ipClientEndpoint);

                isConnected = true;
                isConnecting = false;
            }
            catch (Exception)
            {
                Thread.Sleep(2500);
                isConnecting = false;
            }
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel) return false;

            watch.Restart();

            // Connect if needed
            if (!isConnected)
                Connect(e);

            if (e.Cancel) return false;

            // Reduce sending based on user config
            if (!sw.IsRunning)
                sw.Start();

            if (e.Cancel) return false;

            if (sw.ElapsedMilliseconds >
                Global.Configuration.VarRegistry.GetVariable<int>($"{devicename}_send_delay"))
            {
                Color averageColor;
                lock (colorComposition.bitmapLock)
                {
                    //Fix conflict with debug bitmap
                    lock (colorComposition.keyBitmap)
                    {

                        averageColor = Utils.BitmapUtils.GetRegionColor(
                            (Bitmap)colorComposition.keyBitmap,
                            new BitmapRectangle(0, 0, colorComposition.keyBitmap.Width,
                                colorComposition.keyBitmap.Height)
                        );
                    }
                }

                SendColorsToOrb(averageColor.R, averageColor.G, averageColor.B, e);
                sw.Restart();
            }

            if (e.Cancel) return false;

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return true;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            throw new NotImplementedException();
        }

        public void SendColorsToOrb(byte red, byte green, byte blue, DoWorkEventArgs e = null)
        {
            if (e?.Cancel ?? false) return;
            if (!isConnected)
            {
                Reconnect();
                return;
            }

            List<string> orbIDs = new List<string>();
            try
            {
                string orb_ids = Global.Configuration.VarRegistry.GetVariable<string>($"{devicename}_orb_ids") ?? "";
                orbIDs = orb_ids.Split(',').ToList();
            }
            catch (Exception exc)
            {
                orbIDs = new List<string>() { "1" };
            }

            if (e?.Cancel ?? false) return;

            foreach (var orbID in orbIDs)
            {
                if (e?.Cancel ?? false) return;
                if (String.IsNullOrWhiteSpace(orbID))
                    continue;

                try
                {
                    byte[] bytes = new byte[5 + 24 * 3];

                    // Command identifier: C0FFEE
                    bytes[0] = 0xC0;
                    bytes[1] = 0xFF;
                    bytes[2] = 0xEE;

                    // Options parameter: 
                    // 1 = force off
                    // 2 = use lamp smoothing and validate by Orb ID
                    // 4 = validate by Orb ID

                    if (Global.Configuration.VarRegistry.GetVariable<bool>($"{devicename}_use_smoothing"))
                        bytes[3] = 2;
                    else
                        bytes[3] = 4;

                    // Orb ID
                    bytes[4] = byte.Parse(orbID);

                    // RED / GREEN / BLUE
                    bytes[5] = red;
                    bytes[6] = green;
                    bytes[7] = blue;

                    if (e?.Cancel ?? false) return;
                    socket.Send(bytes, bytes.Length, SocketFlags.None);
                }
                catch (Exception)
                {
                }
            }
        }

        public string GetDeviceUpdatePerformance()
        {
            return (IsConnected() ? lastUpdateTime + " ms" : "");
        }

        public VariableRegistry GetRegisteredVariables()
        {
            if (default_registry == null)
            {
                default_registry = new VariableRegistry();
                default_registry.Register($"{devicename}_use_smoothing", true, "Use Smoothing");
                default_registry.Register($"{devicename}_send_delay", 50, "Send delay (ms)");
                default_registry.Register($"{devicename}_orb_ids", "1", "Orb IDs", null, null, "For multiple IDs separate with comma");
            }

            return default_registry;
        }
    }
}