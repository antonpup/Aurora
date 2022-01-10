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
        private List<YeeLightAPI.YeeLightDevice> lights = new List<YeeLightAPI.YeeLightDevice>();

        public override bool Initialize()
        {
            if (!IsInitialized)
            {
                try
                {
                    lights.Clear();

                    var IPListString = Global.Configuration.VarRegistry.GetVariable<string>($"{DeviceName}_IP");
                    var lightIPList = new List<IPAddress>();

                    //Auto discover a device if the IP is empty and auto-discovery is enabled
                    if (string.IsNullOrWhiteSpace(IPListString) && Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_auto_discovery"))
                    {
                        var devices = DeviceLocator.DiscoverDevices(10000, 2);
                        if (!devices.Any())
                        {
                            throw new Exception("Auto-discovery is enabled but no devices have been located.");
                        }

                        lightIPList.AddRange(devices.Select(v => v.GetLightIPAddressAndPort().ipAddress));
                    }
                    else
                    {
                        lightIPList = IPListString.Split(new[] { ',' }).Select(x => IPAddress.Parse(x.Replace(" ", ""))).ToList();
                        if (lightIPList.Count == 0)
                        {
                            throw new Exception("Device IP list is empty.");
                        }
                    }

                    for (int i = 0; i < lightIPList.Count; i++)
                    {
                        IPAddress ipaddr = lightIPList[i];
                        try
                        {
                            ConnectNewDevice(ipaddr);
                        }
                        catch (Exception exc)
                        {
                            LogError($"Encountered an error while connecting to the {i}. light. Exception: {exc}");
                        }
                    }

                    IsInitialized = lights.All(x => x.IsConnected());
                }
                catch (Exception exc)
                {
                    LogError($"Encountered an error while initializing. Exception: {exc}");
                    IsInitialized = false;

                    return false;
                }
            }

            return IsInitialized;
        }

        public override void Shutdown()
        {
            foreach (var light in lights.Where(x => x.IsConnected()))
            {
                light.CloseConnection();
            }

            lights.Clear();

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
                lights.ForEach(x => x.SetColor(targetColor.R, targetColor.G, targetColor.B));
                updateDelayStopWatch.Restart();
            }

            return true;
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            var devKeysEnumAsEnumerable = Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>();

            variableRegistry.Register($"{DeviceName}_devicekey", DeviceKeys.Peripheral_Logo, "Key to Use", devKeysEnumAsEnumerable.Max(), devKeysEnumAsEnumerable.Min());
            variableRegistry.Register($"{DeviceName}_send_delay", 35, "Send delay (ms)");
            variableRegistry.Register($"{DeviceName}_IP", "", "YeeLight IP(s)", null, null, "Comma separated IPv4 or IPv6 addresses.");
            variableRegistry.Register($"{DeviceName}_auto_discovery", false, "Auto-discovery", null, null, "Enable this and empty out the IP field to auto-discover lights.");
        }

        private void ConnectNewDevice(IPAddress lightIP)
        {
            if (lights.Any(x => x.IsConnected() && x.GetLightIPAddressAndPort().ipAddress == lightIP))
            {
                return;
            }

            var light = new YeeLightAPI.YeeLightDevice();
            light.SetLightIPAddressAndPort(lightIP, YeeLightAPI.YeeLightConstants.Constants.DefaultCommandPort);

            LightConnectAndEnableMusicMode(light);

            lights.Add(light);
        }

        private void LightConnectAndEnableMusicMode(YeeLightAPI.YeeLightDevice light)
        {
            int localMusicModeListenPort = GetFreeTCPPort(); // This can be any free port

            IPAddress localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                var lightIP = light.GetLightIPAddressAndPort().ipAddress;
                socket.Connect(lightIP, lightListenPort);
                localIP = ((IPEndPoint)socket.LocalEndPoint).Address;
            }

            light.Connect();
            light.SetMusicMode(localIP, (ushort)localMusicModeListenPort, true);
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
