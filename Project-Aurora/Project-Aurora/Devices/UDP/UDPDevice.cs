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
    public class UdpDevice : DefaultDevice
    {
        public override string DeviceName => "UDP";

        private Stopwatch redrawWatch = new Stopwatch();
        private UdpClient udpClient;

        private string[] ips = new string[0];
        private List<IPEndPoint> endpoints;
        private DeviceKeys deviceKey;
        private int ledCount;
        private int udpPort;

        private Color lastColor;

        protected override string DeviceInfo => string.Join(", ", ips);

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            var devKeysEnumAsEnumerable = Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>();

            variableRegistry.Register($"{DeviceName}_devicekey", DeviceKeys.Peripheral, "Key Color to Use",
                    devKeysEnumAsEnumerable.Max(), devKeysEnumAsEnumerable.Min());
            variableRegistry.Register($"{DeviceName}_led_count", 300, "LED Count");
            variableRegistry.Register($"{DeviceName}_ip", "", "IP Adresses (Comma separated)");
            variableRegistry.Register($"{DeviceName}_port", 19446, "UDP Port");
        }


        public override bool Initialize()
        {
            if (IsInitialized) return IsInitialized = true;

            deviceKey = Global.Configuration.VarRegistry.GetVariable<DeviceKeys>($"{DeviceName}_devicekey");
            ledCount = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_led_count");

            udpPort = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_port");
            ips = Global.Configuration.VarRegistry.GetVariable<string>($"{DeviceName}_ip").Split(',');

            if (ips.Length < 2 && ips[0] == "")
            {
                LogError("UDP has no IP");
                return IsInitialized = false;
            }
            else
            {
                LogInfo($"{ips.Length} |{string.Join("|", ips)}|");
            }

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

            return IsInitialized = true;
        }

        public override void Shutdown()
        {
            endpoints = null;
            udpClient.Dispose(); // udpClient is IDisposable
            udpClient = null;
            redrawWatch.Stop();

            IsInitialized = false;
        }

        public override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized) return false;

            if (keyColors.ContainsKey(deviceKey))
            {
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
            }

            return true;
        }
    }
}