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
        public override string DeviceName => "YeeLight";

        private const int LightListenPort = 55443;
        private readonly Stopwatch _updateDelayStopWatch = new();
        private readonly List<YeeLightAPI.YeeLightDevice> _lights = new();

        protected override string DeviceInfo => String.Join(
            ", ",
            _lights.Select(light => light.GetLightIPAddressAndPort().ipAddress + ":" + light.GetLightIPAddressAndPort().port + "w:" + _whiteCounter + (light.IsMusicMode() ? "(m)" : ""))
        );

        public override bool Initialize()
        {
            if (IsInitialized) return IsInitialized;
            try
            {
                _lights.Clear();

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

                _updateDelayStopWatch.Start();
                IsInitialized = _lights.All(x => x.IsConnected());
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
            foreach (var light in _lights.Where(x => x.IsConnected()))
            {
                light.CloseConnection();
            }

            _lights.Clear();

            IsInitialized = false;

            if (_updateDelayStopWatch.IsRunning)
            {
                _updateDelayStopWatch.Stop();
            }
        }

        private Color _previousColor = Color.Empty;
        private int _whiteCounter = 10;
        protected override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                return TryUpdate(keyColors);
            }catch(Exception excp)
            {
                Reset();
                return TryUpdate(keyColors);
            }
        }

        private bool TryUpdate(IReadOnlyDictionary<DeviceKeys, Color> keyColors)
        {
            var sendDelay = Math.Max(5, Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_send_delay"));
            if (_updateDelayStopWatch.ElapsedMilliseconds <= sendDelay)
                return false;

            var targetKey = Global.Configuration.VarRegistry.GetVariable<DeviceKeys>($"{DeviceName}_devicekey");
            if (!keyColors.TryGetValue(targetKey, out var targetColor))
                return false;
            if (_previousColor.Equals(targetColor))
                return ProceedSameColor(targetColor);
            _previousColor = targetColor;

            if (IsWhiteTone(targetColor))
            {
                return ProceedDifferentWhiteColor(targetColor);
            }
            _whiteCounter = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_white_delay");
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
            if (IsWhiteTone(targetColor))
            {
                return ProceedWhiteColor(targetColor);
            }

            if (ShouldSendKeepAlive())
            {
                return ProceedColor(targetColor);
            }
            _updateDelayStopWatch.Restart();
            return true;
        }

        private bool ProceedWhiteColor(Color targetColor)
        {
            if (_whiteCounter == 0)
            {
                if (ShouldSendKeepAlive())
                {
                    _lights.ForEach(x =>
                    {
                        x.SetTemperature(6500);
                        x.SetBrightness(targetColor.R * 100 / 255);
                    });
                }
                _updateDelayStopWatch.Restart();
                return true;
            }
            if (_whiteCounter == 1)
            {
                _lights.ForEach(x =>
                {
                    x.SetTemperature(6500);
                    x.SetBrightness(targetColor.R * 100 / 255);
                });
            }
            _whiteCounter--;
            _updateDelayStopWatch.Restart();
            return true;
        }

        private bool ProceedDifferentWhiteColor(Color targetColor)
        {
            if (_whiteCounter > 0)
            {
                _whiteCounter--;
                return ProceedColor(targetColor);
            }
            _lights.ForEach(x =>
            {
                x.SetTemperature(6500);
                x.SetBrightness(targetColor.R * 100 / 255);
            });
            _updateDelayStopWatch.Restart();
            return true;
        }

        private bool ProceedColor(Color targetColor)
        {
            _lights.ForEach(x =>
            {
                x.SetColor(targetColor.R, targetColor.G, targetColor.B);
                x.SetBrightness(Math.Max(targetColor.R, Math.Max(targetColor.G, Math.Max(targetColor.B, (short)1))) * 100 / 255);
            });
            _updateDelayStopWatch.Restart();
            return true;
        }

        private const int KeepAliveCounter = 500;
        private int _keepAlive = KeepAliveCounter;
        private bool ShouldSendKeepAlive()
        {
            if (_keepAlive-- != 0) return false;
            _keepAlive = KeepAliveCounter;
            return true;
        }

        private bool IsWhiteTone(Color color)
        {
            return color.R == color.G && color.G == color.B;
        }

        private void ConnectNewDevice(IPAddress lightIp)
        {
            if (_lights.Any(x => x.IsConnected() && Equals(x.GetLightIPAddressAndPort().ipAddress, lightIp)))
            {
                return;
            }

            var light = new YeeLightAPI.YeeLightDevice();
            light.SetLightIPAddressAndPort(lightIp, Constants.DefaultCommandPort);

            LightConnectAndEnableMusicMode(light);

            _lights.Add(light);
        }

        private int _connectionTries;
        private void LightConnectAndEnableMusicMode(YeeLightAPI.YeeLightDevice light)
        {
            var localMusicModeListenPort = GetFreeTCPPort(); // This can be any free port

            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            var lightIp = light.GetLightIPAddressAndPort().ipAddress;
            socket.Connect(lightIp, LightListenPort);
            var localIp = ((IPEndPoint)socket.LocalEndPoint).Address;

            light.Connect();
            _connectionTries = 100;
            do
            {
                Thread.Sleep(500);
            } while (!light.IsConnected() && --_connectionTries > 0);

            try
            {
                light.SetMusicMode(localIp, (ushort)localMusicModeListenPort, true);
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        private int GetFreeTCPPort()
        {
            // When a TCPListener is created with 0 as port, the TCP/IP stack will assign it a free port
            var listener = new TcpListener(IPAddress.Loopback, 0); // Create a TcpListener on loopback with 0 as the port
            listener.Start();
            var freePort = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return freePort;
        }
    }
}
