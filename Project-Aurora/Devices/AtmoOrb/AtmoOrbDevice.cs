using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Aurora.Devices.AtmoOrbDevice
{
  public class AtmoOrbDevice : Device
  {
    private string devicename = "AtmoOrb";
    private Socket socket;
    private IPEndPoint ipClientEndpoint;
    private bool isConnected;
    private bool isConnecting;
    private Stopwatch sw = new Stopwatch();

    public string GetDeviceDetails()
    {
      if (isConnected)
        return devicename + ": Connected";
      else
        return devicename + ": Not connected";
    }

    public string GetDeviceName()
    {
      return devicename;
    }

    public bool Initialize()
    {
      // Only connect when user has AtmoOrb enabled, additional check to reduce unwanted network load
      if (!Global.Configuration.atmoorb_enabled)
        return false;

      if (!isConnected)
      {
        try
        {
          Connect();
        }
        catch (Exception exc)
        {
          Global.logger.LogLine(string.Format("Device {0} encountered an error during Connecting. Exception: {1}", devicename, exc), Logging_Level.External);
          isConnected = false;

          return false;
        }
      }

      return isConnected;
    }

    public bool IsConnected()
    {
      return isConnected;
    }

    public bool IsInitialized()
    {
      return IsConnected();
    }

    public bool IsKeyboardConnected()
    {
      throw new NotImplementedException();
    }

    public bool IsPeripheralConnected()
    {
      throw new NotImplementedException();
    }

    public bool Reconnect()
    {
      if (socket != null)
      {
        socket.Close();
        socket = null;
        ipClientEndpoint = null;
      }

      isConnected = false;

      Connect();
      return true;
    }

    public void Reset()
    {
      Reconnect();
    }

    public void Shutdown()
    {
      if (socket != null)
      {
        // Set color to black
        SendColorsToOrb(0, 0, 0);

        // Close all connections
        socket.Close();
        socket = null;
        ipClientEndpoint = null;
      }

      isConnected = false;

      if(sw.IsRunning)
        sw.Stop();
      sw = null;
    }

    public void Connect()
    {
      try
      {
        if (isConnecting)
          return;

        isConnecting = true;
        var multiCastIp = IPAddress.Parse("239.15.18.2");
        var port = 49692;

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        ipClientEndpoint = new IPEndPoint(multiCastIp, port);
        socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
            new MulticastOption(multiCastIp));
        socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
        socket.Connect(ipClientEndpoint);

        isConnected = true;
        isConnecting = false;
      }
      catch (Exception)
      {
        Thread.Sleep(2500);
        isConnecting = false;
      }
    }

    public bool UpdateDevice(DeviceColorComposition colorComposition, bool forced = false)
    {
      // Connect if needed
      if (!isConnected)
        Connect();

      // Reduce sending based on user config
      if (!sw.IsRunning)
        sw.Start();

      if (sw.ElapsedMilliseconds > Global.Configuration.atmoorb_send_delay)
      {
        Color averageColor = Utils.BitmapUtils.GetRegionColor(
          colorComposition.keyBitmap,
          new BitmapRectangle(0, 0, colorComposition.keyBitmap.Width, colorComposition.keyBitmap.Height)
        );

        SendColorsToOrb(averageColor.R, averageColor.G, averageColor.B);
        sw.Restart();
      }
      return true;
    }

    public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced = false)
    {
      throw new NotImplementedException();
    }

    public void SendColorsToOrb(byte red, byte green, byte blue)
    {
      if (!isConnected)
      {
        Reconnect();
        return;
      }

      List<string> orbIDs = new List<string>();
      if (Global.Configuration.atmoorb_ids.Contains(","))
      {
        orbIDs = Global.Configuration.atmoorb_ids.Split(',').ToList();
      }
      else
      {
        orbIDs.Add(Global.Configuration.atmoorb_ids);
      }

      foreach (var orbID in orbIDs)
      {
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

          if (Global.Configuration.atmoorb_use_smoothing)
            bytes[3] = 2;
          else
             bytes[3] = 4;

          // Orb ID
          bytes[4] = byte.Parse(orbID);

          // RED / GREEN / BLUE
          bytes[5] = red;
          bytes[6] = green;
          bytes[7] = blue;

          socket.Send(bytes, bytes.Length, SocketFlags.None);
        }
        catch (Exception){
        }
      }
    }
  }
}