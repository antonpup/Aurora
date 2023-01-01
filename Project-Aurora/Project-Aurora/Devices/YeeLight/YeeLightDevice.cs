using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Aurora.Devices.YeeLight
{
    public class YeeLightDevice : DefaultDevice
    {
        public override string DeviceName => "YeeLight";

        private readonly Stopwatch _updateDelayStopWatch = new();
        private readonly List<YeeLightAPI.YeeLightDevice> _lights = new();

        private IYeeLightState _yeeLightState;

        protected override string DeviceInfo => String.Join(
            ", ",
            _lights.Select(light => light.GetLightIPAddressAndPort().ipAddress + ":" + light.GetLightIPAddressAndPort().port + (light.IsMusicMode() ? "(m)" : ""))
        );

        private UdpClient _listener;
        private IPEndPoint _discoveryEndpoint = new(IPAddress.Parse("239.255.255.250"), 1982);

        public override bool Initialize()
        {
            if (IsInitialized) return IsInitialized;
            try
            {
                var ipListString = Global.Configuration.VarRegistry.GetVariable<string>($"{DeviceName}_IP");
                var autoDiscover = Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_auto_discovery");
                YeeLightConnector.PopulateDevices(_lights, autoDiscover ? null : ipListString);

                InitiateState();
                _updateDelayStopWatch.Start();
                IsInitialized = true;
                StartListeningConnections();
            }
            catch (Exception exc)
            {
                LogError($"Encountered an error while initializing. Exception: {exc}");
                IsInitialized = false;

                return false;
            }

            return IsInitialized;
        }

        private void StartListeningConnections()
        {
            if (_listener != null)
            {
                return;
            }
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 1982);

            _listener = new UdpClient();
            _listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listener.Client.Bind(localEndPoint);
            _listener.JoinMulticastGroup(_discoveryEndpoint.Address, IPAddress.Any);
            _listener.MulticastLoopback = true;

            _listener.BeginReceive(OnReceive, _listener);
            Global.logger.Info("Listening for Yeelight broadcast messages on port 1982...");
        }

        private void StopListeningConnections()
        {
            _listener.Close();
            _listener = null;
        }
        
        void OnReceive(IAsyncResult result)
        {
            if (_listener == null)
                return;
            // End the async receive operation and store the received data in a byte array
            byte[] data = _listener.EndReceive(result, ref _discoveryEndpoint);

            // Start listening for the next connection message
            _listener.BeginReceive(OnReceive, _listener);

            // Convert the byte array to a string and print it to the console
            string message = System.Text.Encoding.ASCII.GetString(data);
            var ipListString = Global.Configuration.VarRegistry.GetVariable<string>($"{DeviceName}_IP");
            var deviceIps = ipListString.Split(',');
            var autoDiscover = Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_auto_discovery");
            foreach (var s in message.Split('\n'))
            {
                if (!s.StartsWith("Location:")) continue;
                var ip = IPEndPoint.Parse(s.Remove(0, "Location: yeelight://".Length).Trim());
                if (autoDiscover || deviceIps.Contains(ip.Address.ToString()))
                {
                    YeeLightConnector.ConnectNewDevice(_lights, ip.Address);
                }
                break;
            }
        }

        private void InitiateState()
        {
            _yeeLightState = YeeLightStateBuilder.Build(_lights, Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_white_delay"));
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

        protected override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                return TryUpdate(keyColors);
            }catch(Exception exc)
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

            _yeeLightState = _yeeLightState.Update(targetColor);
            _updateDelayStopWatch.Restart();
            
            return true;
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            var devKeysEnumAsEnumerable = Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>();

            var keysEnumAsEnumerable = devKeysEnumAsEnumerable as DeviceKeys[] ?? devKeysEnumAsEnumerable.ToArray();
            variableRegistry.Register($"{DeviceName}_devicekey", DeviceKeys.Peripheral_Logo, "Key to Use",
                keysEnumAsEnumerable.Max(), keysEnumAsEnumerable.Min());
            variableRegistry.Register($"{DeviceName}_send_delay", 35, "Send delay (ms)");
            variableRegistry.Register($"{DeviceName}_IP", "", "YeeLight IP(s)",
                null, null, "Comma separated IPv4 or IPv6 addresses.");
            variableRegistry.Register($"{DeviceName}_auto_discovery", false, "Auto-discovery",
                null, null, "Enable this and empty out the IP field to auto-discover lights.");
            variableRegistry.Register($"{DeviceName}_white_delay", 10, "White mode delay(ticks)",
                null, null, "How many ticks should happen before white mode is activated.");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            
            StopListeningConnections();
        }
    }
}
