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

namespace Aurora.Devices.Corsair
{
    public class CorsairDeviceConnector : AuroraDeviceConnector
    {
        private SemaphoreSlim allDeviceUpdated;
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
            allDeviceUpdated = new SemaphoreSlim(0, 1);

            for (int i = 0; i < CUESDK.CorsairGetDeviceCount(); i++)
            {
                RegisterDevice(CreateDevice(CUESDK.CorsairGetDeviceInfo(i), i));
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
            switch (info.type)
            {
                case CorsairDeviceType.CDT_Keyboard:
                    return new CorsairKeyboard(info, index);
                case CorsairDeviceType.CDT_Mouse:
                case CorsairDeviceType.CDT_MouseMat:
                    return new CorsairDevice(info, index, AuroraDeviceType.Mouse);
                case CorsairDeviceType.CDT_Headset:
                case CorsairDeviceType.CDT_HeadsetStand:
                    return new CorsairDevice(info, index, AuroraDeviceType.Headset);
                case CorsairDeviceType.CDT_CommanderPro:
                case CorsairDeviceType.CDT_LightingNodePro:
                case CorsairDeviceType.CDT_MemoryModule:
                case CorsairDeviceType.CDT_Cooler:
                case CorsairDeviceType.CDT_Motherboard:
                case CorsairDeviceType.CDT_GraphicsCard:
                default:
                    return new CorsairDevice(info, index);
            }
        }

        protected override void ShutdownImpl()
        {
            CUESDK.CorsairSetLayerPriority(0);
            CUESDK.CorsairReleaseControl(CorsairAccessMode.CAM_ExclusiveLightingControl);
            allDeviceUpdated.Dispose();
        }

        internal bool LedBufferFinished(Mutex mutex)
        {
            if (Devices.Count != CUESDK.CorsairGetDeviceCount())
            {
                this.Reset();
                return false;
            }

            deviceWaitCounter++;
            if(deviceWaitCounter != Devices.Count)
            {
                return false;
            }
            CUESDK.CorsairSetLedsColorsFlushBuffer();
            return true;
            
        }
        public override void DeviceLedUpdateFinished()
        {
            if (Devices.Count != CUESDK.CorsairGetDeviceCount())
            {
                this.Reset();
                return;
            }

            deviceWaitCounter++;
            if (deviceWaitCounter != Devices.Count)
            {
                allDeviceUpdated.Wait();
                return;
            }
            CUESDK.CorsairSetLedsColorsFlushBuffer();
            allDeviceUpdated.Release();
            allDeviceUpdated = new SemaphoreSlim(0, 1);
            deviceWaitCounter = 0;
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
        protected List<CorsairLedColor> colors = new List<CorsairLedColor>();
        private AuroraDeviceType type;
        protected override AuroraDeviceType AuroraDeviceType => type;

        protected CorsairDeviceInfo deviceInfo;
        protected int deviceIndex;
        protected Dictionary<DeviceKey, CorsairLedId> KeyMapping = new Dictionary<DeviceKey, CorsairLedId>();

        public CorsairDevice(CorsairDeviceInfo deviceInfo, int index, AuroraDeviceType type = AuroraDeviceType.Unkown)
        {
            this.deviceInfo = deviceInfo;
            deviceIndex = index;
            this.type = type;
            var possibleLedId = System.Enum.GetValues(typeof(CorsairLedId)).Cast<CorsairLedId>()
                                .Where(l => l != CorsairLedId.CLI_Last)
                                .Select(l => new CorsairLedColor { ledId = l }).ToArray();

            if (CUESDK.CorsairGetLedsColorsByDeviceIndex(index, possibleLedId.Length, possibleLedId) != true)
            {
                LogError("Did not get device led list");
            }
            int deviceKeyIndex = 0;
            foreach (var item in possibleLedId)
            {
                if (item.r != 0 || item.g != 0 || item.b != 0)
                {
                    colors.Add(item);
                    KeyMapping[new DeviceKey(deviceKeyIndex++)] = item.ledId;
                }
            }
            if (KeyMapping.Count != deviceInfo.ledsCount)
            {
                LogError("Not all of the led was discover");
            }
        }
        protected override bool UpdateDeviceImpl(DeviceColorComposition composition)
        {
            for (int i = 0; i < deviceInfo.ledsCount; i++)
            {
                if (composition.keyColors.TryGetValue(i, out Color clr))
                {
                    colors[i].r = clr.R;
                    colors[i].g = clr.G;
                    colors[i].b = clr.B;
                }
            }
            CUESDK.CorsairSetLedsColorsBufferByDeviceIndex(deviceIndex, colors.Count, colors.ToArray());
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

        public override List<DeviceKey> GetAllDeviceKey() => colors.Select((c, index) => new DeviceKey( index, c.ledId.ToString() )).ToList();
    }
    public class CorsairKeyboard : CorsairDevice
    {
        protected override string DeviceName => deviceInfo.model;

        // protected override string DeviceInfo => string.Join(", ", deviceInfos.Select(d => d.model));
        protected override AuroraDeviceType AuroraDeviceType => AuroraDeviceType.Keyboard;

        public CorsairKeyboard(CorsairDeviceInfo deviceInfo, int index) :base(deviceInfo, index)
        {
           /* if (LedMaps.KeyboardLedMap.TryGetValue(Device.Leds[j].Name, out var dk))
            {
                KeyMapping.Add(new DeviceKey(dk));
            }
            else
            {
                KeyMapping.Add(new DeviceKey(500 + overIndex++, Device.Leds[j].Name));
            }*/
        }
        private int ledIdIndex(CorsairLedId id)
        {
            for (int i = 0; i < colors.Count; i++)
            {
                if (colors[i].ledId == id)
                    return i;
            }
            return colors.Count - 1;
        }
        protected override bool UpdateDeviceImpl(DeviceColorComposition composition)
        {
            foreach (var (key, clr) in composition.keyColors)
            {
                if (LedMaps.KeyboardLedMap.TryGetValue((DeviceKeys)key, out var ledid))
                {
                    int index = ledIdIndex(ledid);
                    colors[index].r = clr.R;
                    colors[index].g = clr.G;
                    colors[index].b = clr.B;
                }
            }
            CUESDK.CorsairSetLedsColorsBufferByDeviceIndex(deviceIndex, colors.Count, colors.ToArray());
            return true;
        }
    }
}
