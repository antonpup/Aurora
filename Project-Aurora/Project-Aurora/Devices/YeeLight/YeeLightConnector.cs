using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CSScripting;
using YeeLightAPI.YeeLightConstants;
using YeeLightAPI.YeeLightDeviceLocator;

namespace Aurora.Devices.YeeLight;

public static class YeeLightConnector
{
    private const int LightListenPort = 55443;

    public static async Task PopulateDevices(List<YeeLightAPI.YeeLightDevice> lights, string? ipListString)
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
                await ConnectNewDevice(lights, ipAddress).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                LogError($"Encountered an error while connecting to the {ipAddress} light. Exception: {exc}");
            }
        }
    }

    public static async Task ConnectNewDevice(List<YeeLightAPI.YeeLightDevice> lights, IPAddress lightIp)
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

        var connected = await LightConnectAndEnableMusicMode(light).ConfigureAwait(false);
        if (!connected)
        {
            return;
        }

        if (light.IsConnected())
        {
            lights.Add(light);
        }
    }

    private static async Task<bool> LightConnectAndEnableMusicMode(YeeLightAPI.YeeLightDevice light)
    {
        var localMusicModeListenPort = GetFreeTcpPort(); // This can be any free port

        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        var lightIp = light.GetLightIPAddressAndPort().ipAddress;
        await socket.ConnectAsync(lightIp, LightListenPort).ConfigureAwait(false);
        var localIp = ((IPEndPoint)socket.LocalEndPoint).Address;

        light.Connect();
        int connectionTries = 200;
        do
        {
            await Task.Delay(200).ConfigureAwait(false);
        } while (!light.IsConnected() && --connectionTries > 0);

        if (!light.IsConnected())
        {
            return false;
        }

        try
        {
            light.SetMusicMode(localIp, (ushort)localMusicModeListenPort, true);
            return true;
        }
        catch
        {
            LogError($"Couldn't set MusicMode for device with address '{lightIp}'");
            try
            {
                light.Connect();
                return true;
            }
            catch
            {
                LogError($"Connect failed for device with address '{lightIp}'");
            }
        }

        return false;
    }

    private static int GetFreeTcpPort()
    {
        // When a TCPListener is created with 0 as port, the TCP/IP stack will assign it a free port
        var listener =
            new TcpListener(IPAddress.Loopback, 0); // Create a TcpListener on loopback with 0 as the port
        listener.Start();
        var freePort = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return freePort;
    }

    private static void LogError(string s) => Global.logger.Error($"[YeelightConnector] {s}");
}