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

        public override void DeviceLedUpdateFinished()
        {
            if (Devices.Count != CUESDK.CorsairGetDeviceCount())
            {
                this.Reset();
                return;
            }

            deviceWaitCounter++;
            if (deviceWaitCounter != Devices.Where(d => d.id.ViewPort != null).ToList().Count)
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

        protected List<CorsairLedColor> colors = new List<CorsairLedColor>();
        private AuroraDeviceType type;
        protected override AuroraDeviceType AuroraDeviceType => type;

        protected CorsairDeviceInfo deviceInfo;
        protected int deviceIndex;
        protected Dictionary<DeviceKey, CorsairLedId> KeyMapping = new Dictionary<DeviceKey, CorsairLedId>(new DeviceKey.EqualityComparer());

        protected CorsairDevice() {}
        public CorsairDevice(CorsairDeviceInfo deviceInfo, int index, AuroraDeviceType type = AuroraDeviceType.Unkown)
        {
            this.deviceInfo = deviceInfo;
            deviceIndex = index;
            this.type = type;
            var ledPositions = CUESDK.CorsairGetLedPositionsByDeviceIndex(deviceIndex);

            int overIndex = 0;
            foreach (var pos in ledPositions.pLedPosition)
            {
                KeyMapping[new DeviceKey(overIndex++, pos.ledId.ToString())] = pos.ledId;
                colors.Add(new CorsairLedColor { ledId = pos.ledId });
            }
        }
        protected override bool UpdateDeviceImpl(DeviceColorComposition composition)
        {
            List<CorsairLedColor> colors = new List<CorsairLedColor>();
            foreach (var (key, clr) in composition.keyColors)
            {
                if (KeyMapping.TryGetValue(key, out var ledid))
                {
                    colors.Add(new CorsairLedColor()
                    {
                        ledId = ledid,
                        r = clr.R,
                        g = clr.G,
                        b = clr.B
                    });
                }
            }
            CUESDK.CorsairSetLedsColorsBufferByDeviceIndex(deviceIndex, colors.Count, colors.ToArray());
            return true;
        }


        public override List<DeviceKey> GetAllDeviceKey() => KeyMapping.Keys.ToList();
    }
    public class CorsairKeyboard : CorsairDevice
    {
        protected override AuroraDeviceType AuroraDeviceType => AuroraDeviceType.Keyboard;

        public CorsairKeyboard(CorsairDeviceInfo deviceInfo, int index)
        {
            this.deviceInfo = deviceInfo;
            deviceIndex = index;
            var ledPositions = CUESDK.CorsairGetLedPositionsByDeviceIndex(deviceIndex);
            int overIndex = 0;
            foreach (var pos in ledPositions.pLedPosition)
            {
                if (LedMaps.KeyboardLedMap.TryGetValue(pos.ledId, out var dk))
                {
                    KeyMapping[new DeviceKey(dk)] = pos.ledId;
                }
                else
                {
                    KeyMapping[new DeviceKey(500 + overIndex++, pos.ledId.ToString())] = pos.ledId;
                }
                colors.Add(new CorsairLedColor { ledId = pos.ledId });
            }

        }
    }
}
