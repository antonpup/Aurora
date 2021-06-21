using Aurora.Settings;
using Aurora.Utils;
using Mono.CSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using Corsair.CUE.SDK;
using IronPython.Runtime;
using SharpDX.Direct3D11;
using System.Threading;
using Aurora.Profiles.Discord.GSI.Nodes;
using NLog.Fluent;
//using CUESDK = CorsairRGB.NET.CUE;

namespace Aurora.Devices.Corsair
{
    public class CorsairDeviceConnector : AuroraDeviceConnector
    {
        private List<Mutex> mutices = new List<Mutex>();
        protected override string ConnectorName => "Corsair";
        private int deviceWaitCounter = 0;
        protected override bool InitializeImpl()
        {
            CUESDK.CorsairPerformProtocolHandshake();

            var error = CUESDK.CorsairGetLastError();
            if (error != CorsairError.CE_Success)
            {
                LogError("Error: " + error);
                return false;
            }

            for (int i = 0; i < CUESDK.CorsairGetDeviceCount(); i++)
            {
                devices.Add(CreateDevice(CUESDK.CorsairGetDeviceInfo(i), i));
            }

            if (Global.Configuration.VarRegistry.GetVariable<bool>($"{ConnectorName}_exclusive") && !CUESDK.CorsairRequestControl(CorsairAccessMode.CAM_ExclusiveLightingControl))
            {
                LogError("Error requesting cuesdk exclusive control:" + CUESDK.CorsairGetLastError());
            }
            CUESDK.CorsairSetLayerPriority(255);

            return true;
        }
        private CorsairDevice CreateDevice(CorsairDeviceInfo info, int index)
        {
            mutices.Add(new Mutex());
            switch (info.type)
            {
                case CorsairDeviceType.CDT_Keyboard:
                    return new CorsairKeyboard(info, index, this, mutices.Last());
                case CorsairDeviceType.CDT_Mouse:
                case CorsairDeviceType.CDT_MouseMat:
                    return new CorsairDevice(info, index, this, mutices.Last(), AuroraDeviceType.Mouse);
                case CorsairDeviceType.CDT_Headset:
                case CorsairDeviceType.CDT_HeadsetStand:
                    return new CorsairDevice(info, index, this, mutices.Last(), AuroraDeviceType.Headset);
                case CorsairDeviceType.CDT_CommanderPro:
                case CorsairDeviceType.CDT_LightingNodePro:
                case CorsairDeviceType.CDT_MemoryModule:
                case CorsairDeviceType.CDT_Cooler:
                case CorsairDeviceType.CDT_Motherboard:
                case CorsairDeviceType.CDT_GraphicsCard:
                default:
                    return new CorsairDevice(info, index, this, mutices.Last());
            }
        }

        protected override void ShutdownImpl()
        {
            CUESDK.CorsairSetLayerPriority(0);
            CUESDK.CorsairReleaseControl(CorsairAccessMode.CAM_ExclusiveLightingControl);
            mutices.ForEach(m => m.ReleaseMutex());
        }

        internal bool LedBufferFinished()
        {
            if (devices.Count != CUESDK.CorsairGetDeviceCount())
            {
                this.Reset();
                return false;
            }
                
            deviceWaitCounter++;
            if(deviceWaitCounter != devices.Count)
            {
                return false;
            }
            CUESDK.CorsairSetLedsColorsFlushBuffer();
            mutices.ForEach(m => m.ReleaseMutex());
            return true;
            
        }
        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{ConnectorName}_exclusive", false, "Request exclusive control");
        }
    }


    public class CorsairDevice : AuroraDevice
    {
        protected override string DeviceName => deviceInfo.model;

        // protected override string DeviceInfo => string.Join(", ", deviceInfos.Select(d => d.model));
        protected CorsairLedColor[] colors;
        private AuroraDeviceType type;
        protected override AuroraDeviceType AuroraDeviceType => type;

        protected CorsairDeviceInfo deviceInfo;
        protected int deviceIndex;
        protected CorsairDeviceConnector connector;
        protected Mutex mutex;

        public CorsairDevice(CorsairDeviceInfo deviceInfo, int index, CorsairDeviceConnector con, Mutex mutex, AuroraDeviceType type = AuroraDeviceType.Unkown)
        {
            this.deviceInfo = deviceInfo;
            deviceIndex = index;
            connector = con;
            this.mutex = mutex;
            this.type = type;
            colors = new CorsairLedColor[deviceInfo.ledsCount];
            if (CUESDK.CorsairGetLedsColorsByDeviceIndex(index, deviceInfo.ledsCount, colors) != true)
            {
                LogError("Did not get device led list");
            }
        }
        protected override bool UpdateDeviceImpl(DeviceColorComposition composition)
        {
            for (int i = 0; i < deviceInfo.ledsCount; i++)
            {
                if (composition.keyColors.TryGetValue(i, out Color clr))
                {
                    colors[i] = new CorsairLedColor()
                    {
                        ledId = colors[i].ledId,
                        r = clr.R,
                        g = clr.G,
                        b = clr.B
                    };
                }
            }
            CUESDK.CorsairSetLedsColorsBufferByDeviceIndex(deviceIndex, deviceInfo.ledsCount, colors);
            mutex.WaitOne();
            if(!connector.LedBufferFinished())
            {
                mutex.WaitOne();
                mutex.ReleaseMutex();
            }
            return true;
        }

        private CorsairLedId GetInitialLedIdForDeviceType(CorsairDeviceType type)
        {
            return type switch
            {
                CorsairDeviceType.CDT_Headset => CorsairLedId.CLH_LeftLogo,
                CorsairDeviceType.CDT_MemoryModule => CorsairLedId.CLDRAM_1,
                CorsairDeviceType.CDT_Cooler => CorsairLedId.CLLC_C1_1,
                CorsairDeviceType.CDT_Motherboard => CorsairLedId.CLMB_Zone1,
                CorsairDeviceType.CDT_GraphicsCard => CorsairLedId.CLGPU_Zone1,
                _ => CorsairLedId.CLI_Invalid
            };
        }


    }
    public class CorsairKeyboard : CorsairDevice
    {
        protected override string DeviceName => deviceInfo.model;

        // protected override string DeviceInfo => string.Join(", ", deviceInfos.Select(d => d.model));
        protected override AuroraDeviceType AuroraDeviceType => AuroraDeviceType.Keyboard;

        public CorsairKeyboard(CorsairDeviceInfo deviceInfo, int index, CorsairDeviceConnector con, Mutex mutex) :base(deviceInfo, index, con, mutex)
        {
        }

        protected override bool UpdateDeviceImpl(DeviceColorComposition composition)
        {
            int i = 0;
            foreach (var (key, clr) in composition.keyColors)
            {
                if (LedMaps.KeyboardLedMap.TryGetValue((DeviceKeys)key, out var ledid))
                {
                    colors[i] = new CorsairLedColor()
                    {
                        ledId = ledid,
                        r = clr.R,
                        g = clr.G,
                        b = clr.B
                    };
                    i++;
                }
            }
            CUESDK.CorsairSetLedsColorsBufferByDeviceIndex(deviceIndex, deviceInfo.ledsCount, colors);
            mutex.WaitOne();
            connector.LedBufferFinished();
            mutex.WaitOne();
            mutex.ReleaseMutex();
            return true;
        }
    }
}
