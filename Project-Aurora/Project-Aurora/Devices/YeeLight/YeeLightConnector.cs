using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CSScripting;
using YeeLightAPI.YeeLightConstants;
using YeeLightAPI.YeeLightDeviceLocator;

namespace Aurora.Devices.YeeLight
{
    public static class YeeLightConnector
    {
        private const int LightListenPort = 55443;
        public static void PopulateDevices(List<YeeLightAPI.YeeLightDevice> lights, string ipListString)
        {
            lights.Clear();

            var lightIpList = new List<IPAddress>();

            //Auto discover a device if the IP is empty and auto-discovery is enabled
            if (string.IsNullOrWhiteSpace(ipListString))
            {
                var devices = DeviceLocator.DiscoverDevices(5000, 2);
                if (!devices.Any())
                {
                    throw new Exception("Auto-discovery is enabled but no devices have been located.");
                }

                lightIpList.AddRange(devices.Select(v => v.GetLightIPAddressAndPort().ipAddress));
            }
            else
            {
                lightIpList = ipListString.Split(',').Select(x => IPAddress.Parse(x.Replace(" ", ""))).ToList();
            }

            foreach (var ipAddress in lightIpList)
            {
                try
                {
                    ConnectNewDevice(lights, ipAddress);
                }
                catch (Exception exc)
                {
                    LogError($"Encountered an error while connecting to the {ipAddress} light. Exception: {exc}");
                }
            }
        }

        public static void ConnectNewDevice(List<YeeLightAPI.YeeLightDevice> lights, IPAddress lightIp)
        {
            var yeeLightDevices = lights.Where(x => Equals(x.GetLightIPAddressAndPort().ipAddress, lightIp));
            if (!yeeLightDevices.IsEmpty())
            {
                var yeeLightDevice = yeeLightDevices.First();
                lights.Remove(yeeLightDevice);
                yeeLightDevice.CloseConnection();
            }

            var light = new YeeLightAPI.YeeLightDevice();
            light.SetLightIPAddressAndPort(lightIp, Constants.DefaultCommandPort);

            var connected = LightConnectAndEnableMusicMode(light);
            if (!connected)
            {
                return;
            }

            if (light.IsConnected())
            {
                lights.Add(light);
            }
        }

        private static int _connectionTries;

        private static bool LightConnectAndEnableMusicMode(YeeLightAPI.YeeLightDevice light)
        {
            var localMusicModeListenPort = GetFreeTcpPort(); // This can be any free port

            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            var lightIp = light.GetLightIPAddressAndPort().ipAddress;
            socket.Connect(lightIp, LightListenPort);
            var localIp = ((IPEndPoint) socket.LocalEndPoint).Address;

            light.Connect();
            _connectionTries = 200;
            do
            {
                Thread.Sleep(200);
            } while (!light.IsConnected() && --_connectionTries > 0);

            if (!light.IsConnected())
            {
                return false;
            }

            try
            {
                light.SetMusicMode(localIp, (ushort) localMusicModeListenPort, true);
                return true;
            }
            catch (Exception e)
            {
                return true;
            }
        }

        private static int GetFreeTcpPort()
        {
            // When a TCPListener is created with 0 as port, the TCP/IP stack will assign it a free port
            var listener =
                new TcpListener(IPAddress.Loopback, 0); // Create a TcpListener on loopback with 0 as the port
            listener.Start();
            var freePort = ((IPEndPoint) listener.LocalEndpoint).Port;
            listener.Stop();
            return freePort;
        }
        
        private static void LogError(string s) => Global.logger.Error($"[YeelightConnector] {s}");
    }
}