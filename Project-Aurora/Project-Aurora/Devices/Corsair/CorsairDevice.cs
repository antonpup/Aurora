using Aurora.Settings;
using Aurora.Utils;
using CorsairRGB.NET;
using CorsairRGB.NET.Enums;
using CorsairRGB.NET.Structures;
using Mono.CSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using CUESDK = CorsairRGB.NET.CUE;

namespace Aurora.Devices.Corsair
{
    public class CorsairDevice : DefaultDevice
    {
        public override string DeviceName => "Corsair";

        protected override string DeviceInfo => string.Join(", ", deviceInfos.Select(d => d.Model));

        private readonly List<CorsairDeviceInfo> deviceInfos = new List<CorsairDeviceInfo>();

        public override bool Initialize()
        {
            CUESDK.PerformProtocolHandshake();
            var error = CUESDK.GetLastError();
            if (error != CorsairError.Success)
            {
                LogError("Error: " + error);
                return IsInitialized = false;
            }

            for (int i = 0; i < CUESDK.GetDeviceCount(); i++)
                deviceInfos.Add(CUESDK.GetDeviceInfo(i));

            if (Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_exclusive") && !CUESDK.RequestControl())
            {
                LogError("Error requesting cuesdk exclusive control:" + CUESDK.GetLastError());
            }
            CUESDK.SetLayerPriority(255);

            return IsInitialized = true;
        }

        public override void Shutdown()
        {
            CUESDK.SetLayerPriority(0);
            deviceInfos.Clear();
            CUESDK.ReleaseControl();
            IsInitialized = false;
        }

        public override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (deviceInfos.Count != CUESDK.GetDeviceCount())
                this.Reset();

            for (int i = 0; i < deviceInfos.Count; i++)
            {
                var deviceInfo = deviceInfos[i];
                if (deviceInfo.Type == CorsairDeviceType.Keyboard && Global.Configuration.DevicesDisableKeyboard)
                    continue;

                if ((deviceInfo.Type == CorsairDeviceType.Mouse || deviceInfo.Type == CorsairDeviceType.MouseMat)
                    && Global.Configuration.DevicesDisableMouse)
                    continue;

                if ((deviceInfo.Type == CorsairDeviceType.Headset || deviceInfo.Type == CorsairDeviceType.HeadsetStand)
                    && Global.Configuration.DevicesDisableHeadset)
                    continue;

                List<CorsairLedColor> colors = new List<CorsairLedColor>();

                if (LedMaps.MapsMap.TryGetValue(deviceInfo.Type, out var dict) && dict.Count != 0)
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
                        if(deviceInfo.Type == CorsairDeviceType.LightingNodePro || deviceInfo.Type == CorsairDeviceType.CommanderPro)
                        {
                            int totalLeds = 0;
                            for (int j = 0; j < deviceInfo.Channels.ChannelsCount; j++)
                            {
                                totalLeds += deviceInfo.Channels.Channels[j].TotalLedsCount;
                                foreach (var ledid in LedMaps.ChannelLeds[j])
                                {
                                    if (colors.Count == totalLeds)
                                        continue;

                                    colors.Add(new CorsairLedColor()
                                    {
                                        LedId = ledid,
                                        R = clr.R,
                                        G = clr.G,
                                        B = clr.B
                                    });
                                }
                            }
                        }
                        else
                        {
                            CorsairLedId initial = GetInitialLedIdForDeviceType(deviceInfo.Type);

                            if (initial == CorsairLedId.I_Invalid)
                                continue;

                            for(int j = 0; j <  deviceInfo.LedsCount; j++)
                            {
                                colors.Add(new CorsairLedColor()
                                {
                                    LedId = initial++,
                                    R = clr.R,
                                    G = clr.G,
                                    B = clr.B
                                });
                            }
                        }
                    }
                }

                if (colors.Count == 0)
                    continue;

                CUESDK.SetDeviceColors(i, colors.ToArray());
            }

            return CUESDK.Update();
        }

        private CorsairLedId GetInitialLedIdForDeviceType(CorsairDeviceType type)
        {
            return type switch
            {
                CorsairDeviceType.Headset => CorsairLedId.H_LeftLogo,
                CorsairDeviceType.MemoryModule => CorsairLedId.DRAM_1,
                CorsairDeviceType.Cooler => CorsairLedId.LC_C1_1,
                CorsairDeviceType.Motherboard => CorsairLedId.MB_Zone1,
                CorsairDeviceType.GraphicsCard => CorsairLedId.GPU_Zone1,
                _ => CorsairLedId.I_Invalid
            };
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_exclusive", false, "Request exclusive control");
        }
    }
}
