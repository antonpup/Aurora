using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Aurora.Devices.YeeLight;

public class DeviceDetectedEventArgs : EventArgs
{
    public IPAddress IpAddress { get; }

    public DeviceDetectedEventArgs(IPAddress ipAddress)
    {
        IpAddress = ipAddress;
    }
}

public class YeelightConnectionListener
{
    public event EventHandler<DeviceDetectedEventArgs>? DeviceDetected;

    private UdpClient? _listener;
    private IPEndPoint _discoveryEndpoint = new(IPAddress.Parse("239.255.255.250"), 1982);

    private readonly IReadOnlyList<string> _deviceIps;
    private readonly bool _connectAll;

    public YeelightConnectionListener()
    {
        _connectAll = true;
        _deviceIps = new List<string>();
    }

    public YeelightConnectionListener(IReadOnlyList<string> deviceIps)
    {
        _deviceIps = deviceIps;
    }

    public void StartListeningConnections()
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
        Global.logger.Information("Listening for Yeelight broadcast messages on port 1982...");
    }

    public void StopListeningConnections()
    {
        _listener = null;
        var udpClient = _listener;
        udpClient?.Close();
    }

    private void OnReceive(IAsyncResult result)
    {
        if (_listener == null)
            return;
        // End the async receive operation and store the received data in a byte array
        byte[] data = _listener.EndReceive(result, ref _discoveryEndpoint);

        // Start listening for the next connection message
        _listener.BeginReceive(OnReceive, _listener);

        // Convert the byte array to a string and print it to the console
        string message = System.Text.Encoding.ASCII.GetString(data);
        foreach (var s in message.Split('\n'))
        {
            if (!s.StartsWith("Location:")) continue;
            var ip = IPEndPoint.Parse(s.Remove(0, "Location: yeelight://".Length).Trim());
            if (_connectAll || _deviceIps.Contains(ip.Address.ToString()))
            {
                DeviceDetected?.Invoke(this, new DeviceDetectedEventArgs(ip.Address));
            }

            break;
        }
    }
}