using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using AuraServiceLib;

namespace Aurora.Devices.Asus
{
    public class AsusHandler
    {
        private IAuraSdk2 auraSdk;
        private readonly List<AuraSyncDevice> devices = new List<AuraSyncDevice>();
        private object deviceLock = new object();
        
        public int DeviceCount => devices.Count;
        
        public bool Start()
        {
            try
            {
                auraSdk = new AuraSdk() as IAuraSdk2;
            }
            catch
            {
                Log("AuraSDK not installed!");
                auraSdk = null;
            }
            if (auraSdk == null)
                return false;

            auraSdk.ReleaseControl(0);
            auraSdk.SwitchMode();
            foreach (IAuraSyncDevice device in auraSdk.Enumerate((uint)AsusDeviceType.All))
            {
                var deviceType = (AsusDeviceType)device.Type;
                Log($"Added device {device.Name} of type {deviceType} it has {device.Lights.Count} lights");
                switch (deviceType)
                {
                    case AsusDeviceType.Keyboard:
                        devices.Add(new AuraSyncKeyboardDevice(this, (IAuraSyncKeyboard)device));
                        break;
                    default:
                        devices.Add(new AuraSyncDevice(this, device));
                        break;
                }
            }
            
            foreach (AuraSyncDevice device in  devices)
                device.Start();

            return true;
        }

        public void Stop()
        {
            lock (deviceLock)
            {
                foreach (var device in devices)
                    device.Stop();
            
                devices.Clear();
                auraSdk.ReleaseControl(0);
            }
        }
        
        public void UpdateColors(Dictionary<DeviceKeys, Color> colors)
        {
            lock (deviceLock)
            {
                foreach (var device in devices)
                    device.UpdateColors(colors);
            }
        }

        public string GetDevicePerformance()
        {
            StringBuilder stringBuilder = new StringBuilder();

            lock (deviceLock)
            {
                for (var i = 0; i < devices.Count; i++)
                {
                    var device = devices[i];
                    if (i != 0)
                        stringBuilder.Append(", ");

                    stringBuilder.Append(device.Name).Append(" ").Append(device.LastUpdateMillis).Append("ms");
                }
            }

            return stringBuilder.ToString();
        }

        public bool KeyboardActive()
        {
            lock (deviceLock)
            {
                foreach (var device in devices)
                {
                    if (device.DeviceType == AsusDeviceType.Keyboard && device.Active)
                        return true;
                }
            }

            return false;
        }

        public bool MouseActive()
        {
            lock (deviceLock)
            {
                foreach (var device in devices)
                {
                    if (device.DeviceType == AsusDeviceType.Mouse && device.Active)
                        return true;
                }
            }

            return false;
        }

        public void DisconnectDevice(AuraSyncDevice device)
        {
            lock (deviceLock)
            {
                Log($"Device {device.Name} was disconnected");
                device.Stop();
                devices.Remove(device);
            }
        }
        
        public static void Log(string text)
        {
            Global.logger.Info($"[ASUS] {text}");
        }
        
        /// <summary>
        /// Devices specified in the AsusSDK documentation
        /// </summary>
        public enum AsusDeviceType : uint
        {
            All = 0x00000000,
            Motherboard = 0x00010000,
            MotherboardLedStrip = 0x00011000,
            AllInOnePc = 0x00012000,
            Vga = 0x00020000,
            Display = 0x00030000,
            Headset = 0x00040000,
            Microphone = 0x00050000,
            ExternalHdd = 0x00060000,
            ExternalBdDrive = 0x00061000,
            Dram = 0x00070000,
            Keyboard = 0x00080000,
            NotebookKeyboard = 0x00081000,
            NotebookKeyboard4ZoneType = 0x00081001,
            Mouse = 0x00090000,
            Chassis = 0x000B0000,
            Projector = 0x000C0000,
        }
    }
}