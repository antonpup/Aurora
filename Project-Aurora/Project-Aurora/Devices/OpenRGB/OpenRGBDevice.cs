using Aurora.Settings;
using OpenRGB.NET;
using OpenRGB.NET.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DK = Aurora.Devices.DeviceKeys;
using OpenRGBColor = OpenRGB.NET.Models.Color;
using OpenRGBDevice = OpenRGB.NET.Models.Device;
using OpenRGBDeviceType = OpenRGB.NET.Enums.DeviceType;
using OpenRGBZoneType = OpenRGB.NET.Enums.ZoneType;

namespace Aurora.Devices.OpenRGB
{
    public class OpenRGBDeviceConnector : AuroraDeviceConnector
    {

        protected override string ConnectorName => "OpenRGB";

        private OpenRGBClient _openRgb;

        protected override bool InitializeImpl()
        {
            try
            {
                _openRgb = new OpenRGBClient(name: "Aurora");
                _openRgb.Connect();

                OpenRGBDevice[] _devices = _openRgb.GetAllControllerData();


                for (var i = 0; i < _devices.Length; i++)
                {
                    OpenRGBAuroraDevice device = new OpenRGBAuroraDevice(_devices[i], i, _openRgb);
                    RegisterDevice(device);
                }
            }
            catch (Exception e)
            {
                LogError("There was an error in OpenRGB device: " + e);
                return false;
            }

            return true;
        }

        protected override void ShutdownImpl()
        {
            _openRgb?.Dispose();
            _openRgb = null;
        }
        public void UpdateLeds(int deviceIndex, OpenRGBColor[] colors)
        {
            _openRgb.UpdateLeds(deviceIndex, colors);
        }
    }
    public class OpenRGBAuroraDevice : AuroraDevice
    {
        private OpenRGBDevice Device;
        private OpenRGBColor[] DeviceColors;
        private List<DeviceKey> KeyMapping = new List<DeviceKey>();
        private int DeviceIndex;
        static object update_lock = new object();
        private OpenRGBClient _openRgb;
        protected override string DeviceName => Device.Name;

        protected override AuroraDeviceType AuroraDeviceType => AuroraDeviceTypeConverter(Device.Type);

        public int Id { get; set; }

        public OpenRGBAuroraDevice(OpenRGBDevice device, int deviceIndex, OpenRGBClient openRgb)
        {
            Device = device;
            _openRgb = openRgb;
            DeviceIndex = deviceIndex;
            DeviceColors = new OpenRGBColor[Device.Leds.Length];
            for (var ledIdx = 0; ledIdx < Device.Leds.Length; ledIdx++)
                DeviceColors[ledIdx] = new OpenRGBColor();

            int overIndex = 0;

            for (int j = 0; j < Device.Leds.Length; j++)
            {
                if (Device.Type == OpenRGBDeviceType.Keyboard)
                {
                    if (OpenRGBKeyNames.Keyboard.TryGetValue(Device.Leds[j].Name, out var dk))
                    {
                        KeyMapping.Add(new DeviceKey(dk));
                    }
                    else
                    {
                        KeyMapping.Add(new DeviceKey(500 + overIndex++, Device.Leds[j].Name));
                    }
                }
                else
                {
                    KeyMapping.Add(new DeviceKey(j, Device.Leds[j].Name));
                }
            }
        }

        protected override bool UpdateDeviceImpl(DeviceColorComposition composition)
        {

            //should probably store these bools somewhere when initing
            //might also add this as a property in the library
            if (!Device.Modes.Any(m => m.Name == "Direct"))
                return true;

            for (int ledIdx = 0; ledIdx < Device.Leds.Length; ledIdx++)
            {
                if (composition.keyColors.TryGetValue(KeyMapping[ledIdx].Tag, out var keyColor))
                {
                    DeviceColors[ledIdx] = new OpenRGBColor(keyColor.R, keyColor.G, keyColor.B);
                }
            }

            try
            {
                lock (update_lock)
                {
                    _openRgb.UpdateLeds(DeviceIndex, DeviceColors);
                }
            }
            catch (Exception exc)
            {
                LogError($"Failed to update OpenRGB device {DeviceName}: " + exc);
                return false;
            }

            /*var sleep = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_sleep");
            if (sleep > 0)
                Thread.Sleep(sleep);*/

            return true;
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_sleep", 25, "Sleep for", 1000, 0);
        }
        private AuroraDeviceType AuroraDeviceTypeConverter(OpenRGBDeviceType type)
        {
            switch (type)
            {
                case OpenRGBDeviceType.Motherboard:
                    break;
                case OpenRGBDeviceType.Dram:
                    break;
                case OpenRGBDeviceType.Gpu:
                    break;
                case OpenRGBDeviceType.Cooler:
                    break;
                case OpenRGBDeviceType.Ledstrip:
                    break;
                case OpenRGBDeviceType.Keyboard:
                    return AuroraDeviceType.OpenRGBKeyboard;
                case OpenRGBDeviceType.Mouse:
                    return AuroraDeviceType.OpenRGBMouse;
                case OpenRGBDeviceType.Mousemat:
                    break;
                case OpenRGBDeviceType.Headset:
                    return AuroraDeviceType.OpenRGBHeadset;
                case OpenRGBDeviceType.HeadsetStand:
                    break;
                case OpenRGBDeviceType.Unknown:
                    return AuroraDeviceType.OpenRGBUnkown;
                default:
                    return AuroraDeviceType.OpenRGBUnkown;
            }
            return AuroraDeviceType.Unkown;
        }

        public override List<DeviceKey> GetAllDeviceKey() => KeyMapping;
    }
}
