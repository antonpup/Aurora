using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Aurora.Devices.UDP
{
    public class UdpDevice : IDevice
    {
        public string DeviceName => "UDP";
        public bool IsInitialized { get; private set; } = false;

        private VariableRegistry defaultRegistry = null;

        private Stopwatch redrawWatch = new Stopwatch();
        private Stopwatch updateStopwatch = new Stopwatch();
        private UdpClient udpClient;

        private List<IPEndPoint> endpoints;
        private DeviceKeys deviceKey;
        private int ledCount;

        private Color lastColor;
        private long lastUpdateTime = long.MaxValue;

        public string DeviceDetails => DeviceName + (IsInitialized ? ": Initialized" : ": Not initialized");
        public string DeviceUpdatePerformance => (IsInitialized ? lastUpdateTime + " ms" : "");

        public VariableRegistry RegisteredVariables
        {
            get
            {
                if (defaultRegistry == null)
                {
                    var devKeysEnumAsEnumerable = Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>();

                    defaultRegistry = new VariableRegistry();
                    defaultRegistry.Register($"{DeviceName}_devicekey", DeviceKeys.Peripheral, "Key Color to Use",
                        devKeysEnumAsEnumerable.Max(), devKeysEnumAsEnumerable.Min());
                    defaultRegistry.Register($"{DeviceName}_led_count", 300, "LED Count");
                    defaultRegistry.Register($"{DeviceName}_ip", "", "IP Adresses (Comma separated)");
                    defaultRegistry.Register($"{DeviceName}_port", 19446, "UDP Port");
                }

                return defaultRegistry;
            }
        }


        public bool Initialize()
        {
            if (IsInitialized) return true;

            deviceKey = Global.Configuration.VarRegistry.GetVariable<DeviceKeys>($"{DeviceName}_devicekey");
            ledCount = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_led_count");

            var udpPort = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_port");
            var ips = Global.Configuration.VarRegistry.GetVariable<string>($"{DeviceName}_ip").Split(',');

            endpoints = new List<IPEndPoint>();
            foreach (var ip in ips)
            {
                try
                {
                    endpoints.Add(new IPEndPoint(IPAddress.Parse(ip), udpPort));
                }
                catch (FormatException)
                {
                } // Don't crash on malformed IPs
            }

            udpClient = new UdpClient();
            lastColor = Color.Black;
            redrawWatch.Start();

            IsInitialized = true;
            return true;
        }


        public void Reset()
        {
            Shutdown();
            Initialize();
        }

        public void Shutdown()
        {
            endpoints = null;
            udpClient.Dispose(); // udpClient is IDisposable
            udpClient = null;
            redrawWatch.Stop();

            IsInitialized = false;
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            return UpdateDevice(colorComposition.keyColors, e, forced);
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized) return false;

            if (keyColors.ContainsKey(deviceKey))
            {
                updateStopwatch.Restart();

                var c = keyColors[deviceKey];
                if (redrawWatch.ElapsedMilliseconds < 1000
                ) // Only send the color when it changes or a full second has passed
                {
                    if (c == lastColor && !forced) return true;
                }
                else
                {
                    redrawWatch.Restart();
                }

                lastColor = c;

                // Build a payload by repeating the color ledCount times
                var payload = new byte[3 * ledCount];
                for (var i = 0; i < ledCount * 3; i += 3)
                {
                    payload[i + 0] = c.R;
                    payload[i + 1] = c.G;
                    payload[i + 2] = c.B;
                }

                // Actually send the payload to each endpoint
                foreach (var endpoint in endpoints)
                {
                    udpClient.Send(payload, payload.Length, endpoint);
                }

                updateStopwatch.Stop();
                lastUpdateTime = updateStopwatch.ElapsedMilliseconds;
            }

            return true;
        }
    }
}