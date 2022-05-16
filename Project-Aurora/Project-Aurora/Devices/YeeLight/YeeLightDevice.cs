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
using static YeeLightAPI.YeeLightExceptions.Exceptions;

namespace Aurora.Devices.YeeLight
{
    public class YeeLightDevice : DefaultDevice
    {
        private static readonly object syncLock = new object();

        public override string DeviceName => "YeeLight";

        private const int lightListenPort = 55443;
        private readonly Stopwatch updateDelayStopWatch = new Stopwatch();
        private List<YeeLightAPI.YeeLightDevice> lights = new List<YeeLightAPI.YeeLightDevice>();

        protected override string DeviceInfo => String.Join(
            ", ",
            lights.Select(light => light.GetLightIPAddressAndPort().ipAddress + ":" + light.GetLightIPAddressAndPort().port + "w:" + whiteCounter + (light.IsMusicMode() ? "(m)" : ""))
        );

        public override bool Initialize()
        {
            if (IsInitialized) return IsInitialized;
            try
            {
                lights.Clear();

                var ipListString = Global.Configuration.VarRegistry.GetVariable<string>($"{DeviceName}_IP");
                var lightIpList = new List<IPAddress>();

                //Auto discover a device if the IP is empty and auto-discovery is enabled
                if (string.IsNullOrWhiteSpace(ipListString) && Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_auto_discovery"))
                {
                    var devices = DeviceLocator.DiscoverDevices(10000, 2);
                    if (!devices.Any())
                    {
                        throw new Exception("Auto-discovery is enabled but no devices have been located.");
                    }

                    lightIpList.AddRange(devices.Select(v => v.GetLightIPAddressAndPort().ipAddress));
                }
                else
                {
                    lightIpList = ipListString.Split(',').Select(x => IPAddress.Parse(x.Replace(" ", ""))).ToList();
                    if (lightIpList.Count == 0)
                    {
                        throw new Exception("Device IP list is empty.");
                    }
                }

                for (var i = 0; i < lightIpList.Count; i++)
                {
                    var ipaddr = lightIpList[i];
                    try
                    {
                        ConnectNewDevice(ipaddr);
                    }
                    catch (Exception exc)
                    {
                        LogError($"Encountered an error while connecting to the {i}. light. Exception: {exc}");
                    }
                }

                updateDelayStopWatch.Start();
                IsInitialized = lights.All(x => x.IsConnected());
            }
            catch (Exception exc)
            {
                LogError($"Encountered an error while initializing. Exception: {exc}");
                IsInitialized = false;

                return false;
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

        private Color previousColor = Color.Empty;
        private int whiteCounter = 10;
        protected override bool UpdateDevice(Dictionary<int, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                return TryUpdate(keyColors);
            }catch(Exception excp)
            {
                Reset();
                return TryUpdate(keyColors);
            }
            return false;
        }

        private bool TryUpdate(Dictionary<int, Color> keyColors)
        {
            var sendDelay = Math.Max(5, Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_send_delay"));
            if (updateDelayStopWatch.ElapsedMilliseconds <= sendDelay)
                return false;

            var targetKey = Global.Configuration.VarRegistry.GetVariable<DeviceKeys>($"{DeviceName}_devicekey");
            if (!keyColors.TryGetValue((int)targetKey, out var targetColor))
                return false;
            if (previousColor.Equals(targetColor))
                return ProceedSameColor(targetColor);
            previousColor = targetColor;

            if (isWhiteTone(targetColor))
            {
                return ProceedDifferentWhiteColor(targetColor);
            }
            whiteCounter = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_white_delay");
            return ProceedColor(targetColor);
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            var devKeysEnumAsEnumerable = Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>();

            variableRegistry.Register($"{DeviceName}_devicekey", DeviceKeys.Peripheral_Logo, "Key to Use", devKeysEnumAsEnumerable.Max(), devKeysEnumAsEnumerable.Min());
            variableRegistry.Register($"{DeviceName}_send_delay", 35, "Send delay (ms)");
            variableRegistry.Register($"{DeviceName}_IP", "", "YeeLight IP(s)", null, null, "Comma separated IPv4 or IPv6 addresses.");
            variableRegistry.Register($"{DeviceName}_auto_discovery", false, "Auto-discovery", null, null, "Enable this and empty out the IP field to auto-discover lights.");
            variableRegistry.Register($"{DeviceName}_white_delay", 10, "White mode delay(ticks)", null, null, "How many ticks should happen before white mode is activated.");
        }

        private bool ProceedSameColor(Color targetColor)
        {
            if (isWhiteTone(targetColor))
            {
                return ProceedWhiteColor(targetColor);
            }

            if (ShouldSendKeepAlive())
            {
                return ProceedColor(targetColor);
            }
            updateDelayStopWatch.Restart();
            return true;
        }

        private bool ProceedWhiteColor(Color targetColor)
        {
            if (whiteCounter == 0)
            {
                if (ShouldSendKeepAlive())
                {
                    lights.ForEach(x =>
                    {
                        x.SetTemperature(6500);
                        x.SetBrightness(targetColor.R * 100 / 255);
                    });
                }
                updateDelayStopWatch.Restart();
                return true;
            }
            if (whiteCounter == 1)
            {
                lights.ForEach(x =>
                {
                    x.SetTemperature(6500);
                    x.SetBrightness(targetColor.R * 100 / 255);
                });
            }
            whiteCounter--;
            updateDelayStopWatch.Restart();
            return true;
        }

        private bool ProceedDifferentWhiteColor(Color targetColor)
        {
            if (whiteCounter > 0)
            {
                whiteCounter--;
                return ProceedColor(targetColor);
            }
            lights.ForEach(x =>
            {
                x.SetTemperature(6500);
                x.SetBrightness(targetColor.R * 100 / 255);
            });
            updateDelayStopWatch.Restart();
            return true;
        }

        private bool ProceedColor(Color targetColor)
        {
            lights.ForEach(x =>
            {
                x.SetColor(targetColor.R, targetColor.G, targetColor.B);
                x.SetBrightness(Math.Max(targetColor.R, Math.Max(targetColor.G, Math.Max(targetColor.B, (short)1))) * 100 / 255);
            });
            updateDelayStopWatch.Restart();
            return true;
        }

        private const int KeepAliveCounter = 500;
        private int _keepAlive = KeepAliveCounter;
        private bool ShouldSendKeepAlive()
        {
            if (_keepAlive-- == 0)
            {
                _keepAlive = KeepAliveCounter;
                return true;
            }

            return false;
        }

            private bool isWhiteTone(Color color)
        {
            return color.R == color.G && color.G == color.B;
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

        private int _connectionTries;
        private void LightConnectAndEnableMusicMode(YeeLightAPI.YeeLightDevice light)
        {
            var localMusicModeListenPort = GetFreeTCPPort(); // This can be any free port

            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            var lightIp = light.GetLightIPAddressAndPort().ipAddress;
            socket.Connect(lightIp, lightListenPort);
            var localIp = ((IPEndPoint)socket.LocalEndPoint).Address;

            light.Connect();
            _connectionTries = 1000;
            Thread.Sleep(500);
            while (!light.IsConnected() && --_connectionTries > 0)
            {
                Thread.Sleep(500);
            }
            light.SetMusicMode(localIp, (ushort)localMusicModeListenPort, true);
        }

        private int GetFreeTCPPort()
        {
            // When a TCPListener is created with 0 as port, the TCP/IP stack will asign it a free port
            var listener = new TcpListener(IPAddress.Loopback, 0); // Create a TcpListener on loopback with 0 as the port
            listener.Start();
            var freePort = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return freePort;
        }
    }
}
