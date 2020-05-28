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
using YeeLightAPI.YeeLightDeviceLocator;
using YeeLightAPI.YeeLightConstants;
using YeeLightAPI.YeeLightExceptions;

namespace Aurora.Devices.YeeLight
{
    public class YeeLightDevice : Device
    {
        private string devicename = "YeeLight";

        private bool isConnected;
        private Stopwatch updateDelayStopWatch = new Stopwatch();
        private Stopwatch updatePerfStopwatch = new Stopwatch();
        private YeeLightAPI.YeeLightDevice light = new YeeLightAPI.YeeLightDevice();
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
                    Global.logger.Error($"Device {devicename} encountered an error while connecting. Exception: {exc}");
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
            Shutdown();
            return Initialize();
        }

        public void Reset()
        {
            Reconnect();
        }

        public void Shutdown()
        {
            if (light.IsConnected())
            {
                light.CloseConnection();
            }

            isConnected = false;

            if (updateDelayStopWatch.IsRunning)
            {
                updateDelayStopWatch.Stop();
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
            if (light.IsConnected())
            {
                return;
            }

            //Can't reconnect using same device class instance
            light = new YeeLightAPI.YeeLightDevice();

            //Auto discover a device if the IP is 0.0.0.0 and auto-discovery is enabled
            if (lightIP.Equals(IPAddress.Parse("0.0.0.0")) && default_registry.GetVariable<bool>($"{devicename}_auto_discovery"))
            {
                var devices = DeviceLocator.DiscoverDevices(10000, 2);
                if (devices.Any())
                {
                    light = devices.First();
                    lightIP = light.GetLightIPAddressAndPort().ipAddress;
                    default_registry.SetVariable($"{devicename}_IP", lightIP.ToString());
                }
                else
                {
                    throw new Exception("IP address is set to 0.0.0.0 and auto-discovery is enabled but no devices have been located.");
                }
            }
            //If it isn't then set the IP to the provided IP address
            else
            {
                light.SetLightIPAddressAndPort(lightIP, YeeLightAPI.YeeLightConstants.Constants.DefaultCommandPort);
            }

            IPAddress localIP;
            int localMusicModeListenPort = GetFreeTCPPort(); // This can be any free port

            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect(lightIP, lightListenPort);
                localIP = ((IPEndPoint)socket.LocalEndPoint).Address;
            }

            light.Connect();
            light.SetMusicMode(localIP, (ushort)localMusicModeListenPort, true);
            isConnected = light.IsConnected();
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            if (e.Cancel)
            {
                return false;
            }

            updatePerfStopwatch.Restart();

            // Reduce sending based on user config
            if (!updateDelayStopWatch.IsRunning)
            {
                updateDelayStopWatch.Start();
            }

            if (updateDelayStopWatch.ElapsedMilliseconds > Global.Configuration.VarRegistry.GetVariable<int>($"{devicename}_send_delay"))
            {
                var targetKey = Global.Configuration.VarRegistry.GetVariable<DeviceKeys>($"{devicename}_devicekey");
                if (!colorComposition.keyColors.TryGetValue(targetKey, out var targetColor))
                    return false;

                if ((targetColor.R + targetColor.G + targetColor.B) > 0)
                {
                    light.SetColor(targetColor.R, targetColor.G, targetColor.B);
                }

                updatePerfStopwatch.Stop();
                lastUpdateTime = updatePerfStopwatch.ElapsedMilliseconds + updateDelayStopWatch.ElapsedMilliseconds;
                updateDelayStopWatch.Restart();

                if (e.Cancel)
                {
                    return false;
                }
            }

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
                var devKeysEnumAsEnumerable = Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>();

                default_registry = new VariableRegistry();
                default_registry.Register($"{devicename}_devicekey", DeviceKeys.Peripheral_Logo, "Key to Use", devKeysEnumAsEnumerable.Max(), devKeysEnumAsEnumerable.Min());
                default_registry.Register($"{devicename}_send_delay", 35, "Send delay (ms)");
                default_registry.Register($"{devicename}_IP", "0.0.0.0", "YeeLight IP", null, null, "If set to 0.0.0.0 and auto-discovery is enabled, it will try to discover a YeeLight and connect to it.");
                default_registry.Register($"{devicename}_auto_discovery", false, "Auto-discovery", null, null, "Enable this and set IP to 0.0.0.0 to auto-discover a light.");
            }

            return default_registry;
        }
    }
}
