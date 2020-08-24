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
using YeeLightAPI;

namespace Aurora.Devices.YeeLight
{
    public class YeeLightDevice : DefaultDevice
    {
        public override string DeviceName => "YeeLight";

        private const int lightListenPort = 55443;
        private readonly Stopwatch updateDelayStopWatch = new Stopwatch();
        private YeeLightAPI.YeeLightDevice light = new YeeLightAPI.YeeLightDevice();

        public override bool Initialize()
        {
            if (!IsInitialized)
            {
                try
                {
                    Connect(IPAddress.Parse(Global.Configuration.VarRegistry.GetVariable<string>($"{DeviceName}_IP")));
                }
                catch (Exception exc)
                {
                    LogError($"Encountered an error while connecting. Exception: {exc}");
                    IsInitialized = false;

                    return false;
                }
            }

            return IsInitialized;
        }

        public override void Shutdown()
        {
            if (light.IsConnected())
            {
                light.CloseConnection();
            }

            IsInitialized = false;

            if (updateDelayStopWatch.IsRunning)
            {
                updateDelayStopWatch.Stop();
            }
        }

        public override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            // Reduce sending based on user config
            if (!updateDelayStopWatch.IsRunning)
            {
                updateDelayStopWatch.Start();
            }

            if (updateDelayStopWatch.ElapsedMilliseconds <= Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_send_delay"))
                return false;

            var targetKey = Global.Configuration.VarRegistry.GetVariable<DeviceKeys>($"{DeviceName}_devicekey");
            if (!keyColors.TryGetValue(targetKey, out var targetColor))
                return false;

            if ((targetColor.R + targetColor.G + targetColor.B) > 0)
            {
                light.SetColor(targetColor.R, targetColor.G, targetColor.B);
                updateDelayStopWatch.Restart();
            }

            return true;
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            var devKeysEnumAsEnumerable = Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>();

            variableRegistry.Register($"{DeviceName}_devicekey", DeviceKeys.Peripheral_Logo, "Key to Use", devKeysEnumAsEnumerable.Max(), devKeysEnumAsEnumerable.Min());
            variableRegistry.Register($"{DeviceName}_send_delay", 35, "Send delay (ms)");
            variableRegistry.Register($"{DeviceName}_IP", "0.0.0.0", "YeeLight IP", null, null, "If set to 0.0.0.0 and auto-discovery is enabled, it will try to discover a YeeLight and connect to it.");
            variableRegistry.Register($"{DeviceName}_auto_discovery", false, "Auto-discovery", null, null, "Enable this and set IP to 0.0.0.0 to auto-discover a light.");
        }

        private void Connect(IPAddress lightIP)
        {
            if (light.IsConnected())
            {
                return;
            }

            //Can't reconnect using same device class instance
            light = new YeeLightAPI.YeeLightDevice();

            //Auto discover a device if the IP is 0.0.0.0 and auto-discovery is enabled
            if (lightIP.Equals(IPAddress.Parse("0.0.0.0")) && Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_auto_discovery"))
            {
                var devices = DeviceLocator.DiscoverDevices(10000, 2);
                if (devices.Any())
                {
                    light = devices.First();
                    lightIP = light.GetLightIPAddressAndPort().ipAddress;
                    Global.Configuration.VarRegistry.SetVariable($"{DeviceName}_IP", lightIP.ToString());
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
            IsInitialized = light.IsConnected();
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
    }
}
