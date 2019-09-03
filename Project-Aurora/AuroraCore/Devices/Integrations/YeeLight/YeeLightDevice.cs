using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Devices.YeeLight
{
    public class YeeLightDevice : Device
    {
        private string devicename = "YeeLight";

        private bool isConnected;
        private Stopwatch sw = new Stopwatch();

        private Stopwatch watch = new Stopwatch();
        private YeeLightAPI.YeeLight light = new YeeLightAPI.YeeLight();
        private long lastUpdateTime = 0;
        private const int lightListenPort = 55443; // The YeeLight smart bulb listens for commands on this port

        private VariableRegistry default_registry = null;

        public string GetDeviceDetails()
        {
            return devicename + (isConnected ? ": Connected" : ": Not connected");
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
                    Connect(IPAddress.Parse(Global.Configuration.VarRegistry.GetVariable<string>($"{devicename}_IP")));
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
            light.CloseConnection();

            isConnected = false;

            Connect(IPAddress.Parse(Global.Configuration.VarRegistry.GetVariable<string>($"{devicename}_IP")));
            return true;
        }

        public void Reset()
        {
            Reconnect();
        }

        public void Shutdown()
        {
            light.CloseConnection();

            isConnected = false;

            if (sw.IsRunning)
            {
                sw.Stop();
            }
        }

        private int GetFreeTCPPort()
        {
            int freePort;

            // When a TCPListener is created with 0 as port, the TCP/IP stack will asign it a free port
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0); // Create a TcpListener on loopback with 0 as the port
            listener.Start();
            freePort = ((IPEndPoint)listener.LocalEndpoint).Port; // Gets the local port the TcpListener is listening on
            listener.Stop();
            return freePort;
        }

        public void Connect(IPAddress lightIP, DoWorkEventArgs token = null)
        {
            try
            {
                if (!light.isConnected())
                {
                    IPAddress localIP;
                    int localListenPort = GetFreeTCPPort(); // This can be any port

                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                    {
                        socket.Connect(lightIP, lightListenPort);
                        localIP = ((IPEndPoint)socket.LocalEndPoint).Address;
                    }

                    isConnected = light.Connect(lightIP, lightListenPort) && light.SetMusicMode(localIP, localListenPort);
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error($"Device {devicename} encountered an error during Connecting. Exception: {exc}");
                isConnected = false;
            }
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel)
            {
                return false;
            }

            watch.Restart();

            // Connect if needed
            if (!isConnected)
            {
                Connect(IPAddress.Parse(Global.Configuration.VarRegistry.GetVariable<string>($"{devicename}_IP")), e);
            }

            if (e.Cancel)
            {
                return false;
            }

            // Reduce sending based on user config
            if (!sw.IsRunning)
            {
                sw.Start();
            }

            if (e.Cancel)
            {
                return false;
            }

            if (sw.ElapsedMilliseconds > Global.Configuration.VarRegistry.GetVariable<int>($"{devicename}_send_delay"))
            {
                Color targetColor = colorComposition.keyColors.FirstOrDefault(pair =>
                    {
                        return pair.Key == Global.Configuration.VarRegistry.GetVariable<DeviceKeys>($"{devicename}_devicekey");
                    }).Value;

                light.SetColor(targetColor.R, targetColor.G, targetColor.B);
                sw.Restart();
            }

            if (e.Cancel)
            {
                return false;
            }

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return true;
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            throw new NotImplementedException();
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
                default_registry.Register($"{devicename}_devicekey", DeviceKeys.Peripheral, "Key to Use", DeviceKeys.MOUSEPADLIGHT15, DeviceKeys.Peripheral);
                default_registry.Register($"{devicename}_send_delay", 100, "Send delay (ms)");
                default_registry.Register($"{devicename}_IP", "0.0.0.0", "YeeLight IP");
            }

            return default_registry;
        }
    }
}
