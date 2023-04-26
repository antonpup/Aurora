using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Settings;

namespace Aurora.Devices.AtmoOrb;

public class AtmoOrbDevice : DefaultDevice
{
    private Socket _socket;
    private bool _isConnected;
    private bool _isConnecting;

    public override string DeviceDetails => IsInitialized
        ? "Initialized"
        : "Not Initialized";

    public override string DeviceName => "AtmoOrb";

    protected override async Task<bool> DoInitialize()
    {
        if (_isConnected) return true;
        try
        {
            await Connect().ConfigureAwait(false);
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, $"Device {DeviceName} encountered an error during Connecting.");
            _isConnected = false;

            return false;
        }

        return _isConnected;
    }

    public override bool IsInitialized => _isConnected;

    public async Task<bool> Reconnect()
    {
        if (_socket != null)
        {
            await _socket.DisconnectAsync(false).ConfigureAwait(false);
            _socket.Close();
            _socket = null;
        }

        _isConnected = false;

        await Connect().ConfigureAwait(false);
        return true;
    }

    public override async Task Reset()
    {
        await Reconnect().ConfigureAwait(false);
    }

    public override async Task Shutdown()
    {
        if (_socket != null)
        {
            // Set color to black
            await SendColorsToOrb(0, 0, 0).ConfigureAwait(false);

            // Close all connections
            if (_socket.Connected)
            {
                try
                {
                    await _socket.DisconnectAsync(false).ConfigureAwait(false);
                }
                catch (Exception exc)
                {
                    this.LogError(DeviceName, exc);
                }
            }

            _socket.Close();
            _socket = null;
        }

        _isConnected = false;
    }

    public async Task Connect(DoWorkEventArgs token = null)
    {
        if (_isConnecting)
            return;

        try
        {


            _isConnecting = true;
            var multiCastIp = IPAddress.Parse("239.15.18.2");
            var port = 49692;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var ipClientEndpoint = new IPEndPoint(multiCastIp, port);
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                new MulticastOption(multiCastIp));
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);

            await _socket.ConnectAsync(ipClientEndpoint).ConfigureAwait(false);

            _isConnected = true;
            _isConnecting = false;
        }
        catch (Exception)
        {
            await Task.Delay(2500).ConfigureAwait(false);
            _isConnecting = false;
        }
    }

    protected override async Task<bool> UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
    {
        if (e.Cancel) return false;

        // Connect if needed
        if (!_isConnected)
        {
            await Connect(e).ConfigureAwait(false);
        }

        if (Watch.ElapsedMilliseconds <= Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_send_delay"))
            return true;
        if (!keyColors.ContainsKey(DeviceKeys.ADDITIONALLIGHT1)) return false;
        var averageColor = keyColors[DeviceKeys.ADDITIONALLIGHT1];   //TODO add 1 zone kb

        await SendColorsToOrb(averageColor.R, averageColor.G, averageColor.B, e).ConfigureAwait(false);

        return !e.Cancel;
    }

    public async Task SendColorsToOrb(byte red, byte green, byte blue, DoWorkEventArgs e = null)
    {
        if (e?.Cancel ?? false) return;
        if (!_isConnected)
        {
            await Reconnect().ConfigureAwait(false);
            return;
        }

        List<string> orbIDs;
        try
        {
            string orbIds = Global.Configuration.VarRegistry.GetVariable<string>($"{DeviceName}_orb_ids") ?? "";
            orbIDs = orbIds.Split(',').ToList();
        }
        catch (Exception)
        {
            orbIDs = new List<string> { "1" };
        }

        if (e?.Cancel ?? false) return;

        foreach (var orbId in orbIDs)
        {
            if (e?.Cancel ?? false) return;
            if (string.IsNullOrWhiteSpace(orbId))
                continue;

            try
            {
                byte[] bytes = new byte[5 + 24 * 3];

                // Command identifier: C0FFEE
                bytes[0] = 0xC0;
                bytes[1] = 0xFF;
                bytes[2] = 0xEE;

                // Options parameter: 
                // 1 = force off
                // 2 = use lamp smoothing and validate by Orb ID
                // 4 = validate by Orb ID

                if (Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_use_smoothing"))
                    bytes[3] = 2;
                else
                    bytes[3] = 4;

                // Orb ID
                bytes[4] = byte.Parse(orbId);

                // RED / GREEN / BLUE
                bytes[5] = red;
                bytes[6] = green;
                bytes[7] = blue;

                if (e?.Cancel ?? false) return;
                _ = await _socket.SendAsync(bytes, SocketFlags.None).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
        }
    }

    protected override void RegisterVariables(VariableRegistry variableRegistry)
    {
        variableRegistry.Register($"{DeviceName}_use_smoothing", true, "Use Smoothing");
        variableRegistry.Register($"{DeviceName}_send_delay", 50, "Send delay (ms)");
        variableRegistry.Register($"{DeviceName}_orb_ids", "1", "Orb IDs", null, null, "For multiple IDs separate with comma");
    }
}