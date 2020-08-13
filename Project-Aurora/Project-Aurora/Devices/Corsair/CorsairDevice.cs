using Aurora.Settings;
using CorsairRGB.NET;
using CorsairRGB.NET.Enums;
using CorsairRGB.NET.Structures;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using CUESDK = CorsairRGB.NET.CUE;

namespace Aurora.Devices.Corsair
{
    public class CorsairDevice : DefaultDevice
    {
        public override string DeviceName => "Corsair";
        protected override string DeviceInfo => ": " + GetSubDeviceDetails();

        private readonly List<CorsairDeviceInfo> deviceInfos = new List<CorsairDeviceInfo>();

        public override bool Initialize()
        {
            CUESDK.PerformProtocolHandshake();
            var error = CUESDK.GetLastError();
            if (error != CorsairError.Success)
            {
                Global.logger.Error("Corsair Error: " + error);
                return IsInitialized = false;
            }

            for (int i = 0; i < CUESDK.GetDeviceCount(); i++)
                deviceInfos.Add(CUESDK.GetDeviceInfo(i));

            if (Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_exclusive") && !CUESDK.RequestControl())
            {
                Global.logger.Error("Error requesting cuesdk exclusive control:" + CUESDK.GetLastError());
            }

            return IsInitialized = true;
        }

        public override void Shutdown()
        {
            IsInitialized = false;
            deviceInfos.Clear();
            CUESDK.ReleaseControl();
        }

        private bool SetDeviceColors(CorsairDeviceType type, int index, Dictionary<DeviceKeys, Color> keyColors)
        {
            List<CorsairLedColor> colors = new List<CorsairLedColor>();

            if (LedMaps.MapsMap.TryGetValue(type, out var dict) && dict.Count != 0)
            {
                foreach (var led in keyColors)
                {
                    if (dict.TryGetValue(led.Key, out var ledid))
                    {
                        colors.Add(new CorsairLedColor()
                        {
                            LedId = ledid,
                            R = led.Value.R,
                            G = led.Value.G,
                            B = led.Value.B
                        });
                    }
                }
            }
            else
            {
                if (keyColors.TryGetValue(DeviceKeys.Peripheral_Logo, out var clr))
                {
                    foreach (CorsairLedId led in LedMaps.DIYLeds)
                    {
                        colors.Add(new CorsairLedColor()
                        {
                            LedId = led,
                            R = clr.R,
                            G = clr.G,
                            B = clr.B
                        });
                    }
                }
            }

            if (colors.Count == 0)
                return false;

            return CUESDK.SetDeviceColors(index, colors.ToArray());
        }

        private string GetSubDeviceDetails()
        {
            StringBuilder a = new StringBuilder();
            for (int i = 0; i < deviceInfos.Count; i++)
            {
                a.Append(deviceInfos[i].Model);
                if (deviceInfos[i].Channels.ChannelsCount != 0)
                    a.Append(": ");

                for (int j = 0; j < deviceInfos[i].Channels.Channels.Length; j++)
                {
                    CorsairChannelInfo channels = deviceInfos[i].Channels.Channels[j];
                    for (int k = 0; k < channels.Devices.Length; k++)
                    {
                        a.Append(channels.Devices[k].Type);
                        if (k != channels.Devices.Length - 1)
                            a.Append(", ");
                    }
                    if (j != deviceInfos[i].Channels.Channels.Length - 1)
                        a.Append(", ");
                }
                if (i != deviceInfos.Count - 1)
                    a.Append("; ");
            }
            return a.ToString();
        }

        public override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            for (int i = 0; i < deviceInfos.Count; i++)
                SetDeviceColors(deviceInfos[i].Type, i, keyColors);

            return CUESDK.Update();
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_exclusive", false);
        }
    }

}
