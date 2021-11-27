using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Settings;
using Aurora.Utils;
using Bloody.NET;

namespace Aurora.Devices.Bloody
{
    public class BloodyDevice : DefaultDevice
    {
        public override string DeviceName => IsInitialized ? "Bloody:" + GetDeviceNames() : "Bloody";

        private BloodyKeyboard keyboard;
        private List<BloodyPeripheral> peripherals;

        private event EventHandler<Dictionary<DeviceKeys, Color>> deviceUpdated;

        public override bool Initialize()
        {
            keyboard = BloodyKeyboard.Initialize();
            if(keyboard != null)
            {
                deviceUpdated += UpdateKeyboard;
            }

            peripherals = BloodyPeripheral.GetDevices();
            deviceUpdated += UpdatePeripherals;

            return IsInitialized = (keyboard != null) || (peripherals.Any());
        }

        public override void Shutdown()
        {
            keyboard?.Disconnect();
            deviceUpdated -= UpdateKeyboard;

            peripherals.ForEach(p => p.Disconnect());
            deviceUpdated -= UpdatePeripherals;

            IsInitialized = false;
        }

        protected override bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false) {
            deviceUpdated(this, keyColors);
            return true;
        }

        private void UpdateKeyboard(object sender, Dictionary<DeviceKeys, Color> keyColors)
        {
            foreach (var key in keyColors)
            {
                if (BloodyKeyMap.KeyMap.TryGetValue(key.Key, out var bloodyKey))
                    keyboard.SetKeyColor(bloodyKey, ColorUtils.CorrectWithAlpha(key.Value));
            }

            keyboard.Update();
        }

        private void UpdatePeripherals(object sender, Dictionary<DeviceKeys, Color> keyColors)
        {
            foreach(var dev in peripherals)
            {
                Dictionary<BloodyPeripheralLed, DeviceKeys> keyMap = null;
                switch (dev.PeripheralType)
                {
                    case PeripheralType.MOUSE:
                        keyMap = BloodyKeyMap.MouseLightMap;
                        break;
                    case PeripheralType.MOUSEPAD:
                        keyMap = BloodyKeyMap.MousePadLightMap;
                        break;
                }
                foreach (KeyValuePair<BloodyPeripheralLed, DeviceKeys> ledAndKey in keyMap)
                {
                    Color color;
                    keyColors.TryGetValue(ledAndKey.Value, out color);
                    dev.SetKeyColor(ledAndKey.Key, ColorUtils.CorrectWithAlpha(color));
                }
                dev.Update();
            }
        }

        private string GetDeviceNames()
        {
            return (keyboard != null ? " Keyboard" : "") + (peripherals.ConvertAll(peripheral => " " + peripheral.PeripheralType));
        }
    }
}