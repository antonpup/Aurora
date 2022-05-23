using Corale.Colore.Core;
using Corale.Colore.Razer.Keyboard;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Settings;
using KeyboardCustom = Corale.Colore.Razer.Keyboard.Effects.Custom;
using MousepadCustom = Corale.Colore.Razer.Mousepad.Effects.Custom;
using MouseCustom = Corale.Colore.Razer.Mouse.Effects.CustomGrid;
using KeypadCustom = Corale.Colore.Razer.Keypad.Effects.Custom;
using ChromaLinkCustom = Corale.Colore.Razer.ChromaLink.Effects.Custom;
using System.ComponentModel;
using System.Linq;
using Corale.Colore.Razer.Mouse;

namespace Aurora.Devices.Razer
{
    public class RazerDevice : DefaultDevice
    {
        public override string DeviceName => "Razer";

        private readonly List<(string Name, Guid Guid)> DeviceGuids = typeof(Corale.Colore.Razer.Devices)
            .GetFields()
            .Select(f => (f.Name, (Guid)f.GetValue(null)))
            .ToList();

        private KeyboardCustom keyboard = KeyboardCustom.Create();
        private MousepadCustom mousepad = MousepadCustom.Create();
        private MouseCustom mouse = MouseCustom.Create();
        private Color headset = Color.Black;
        private KeypadCustom keypad = KeypadCustom.Create();
        private ChromaLinkCustom chromalink = ChromaLinkCustom.Create();

        private readonly List<string> deviceNames = new List<string>();

        protected override string DeviceInfo => string.Join(", ", deviceNames);

        public override bool Initialize()
        {
            try
            {
                Chroma.Instance.Initialize();
            }
            catch (Exception e)
            {
                LogError("Error initializing:" + e.Message);
                return IsInitialized = false;
            }

            if (!Chroma.Instance.Initialized)
            {
                LogError("Failed to Initialize Razer Chroma sdk");
                return IsInitialized = false;
            }

            if (Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_query"))
                DetectDevices();

            return IsInitialized = true;
        }

        public override void Shutdown()
        {
            if (!IsInitialized)
                return;

            try
            {
                Chroma.Instance.Uninitialize();
                IsInitialized = false;
            }
            catch (Exception e)
            {
                LogError(e.Message);
            }
        }

        protected bool MouseTryGetGridCoords(DeviceKeys key, out int row, out int column)
        {
            if (RazerMappings.mouseDictionary.TryGetValue(key, out var razerKeyCode))
            {
                // According to SDK source code
                // https://github.com/chroma-sdk/Colore/blob/ad27da8b5fda8d596ab6a9ddabf7f6a0ac4c8cfe/src/Corale.Colore/Razer/Mouse/Effects/CustomGrid.cs#L223
                row = (int)razerKeyCode >> 8;
                column = (int)razerKeyCode & 0xFF;
                return true;
            }

            // There are more leds than the ones defined in Corale.Colore.Razer.Mouse.GridLed            
            switch (key)
            {
                case DeviceKeys.PERIPHERAL_LIGHT20:
                    row = 4;
                    column = 3;
                    return true;
            }

            row = -1;
            column = -1;
            return false;
        }

        protected override bool UpdateDevice(Dictionary<DeviceKeys, System.Drawing.Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized)
                return false;

            if (keyColors.TryGetValue(DeviceKeys.Peripheral_Logo, out var clr))
            {
                keyboard.Set(ToColore(clr));
                mousepad.Set(ToColore(clr));
                mouse.Set(ToColore(clr));
                headset = ToColore(clr);
                chromalink.Set(ToColore(clr));
                keypad.Set(ToColore(clr));
            }

            foreach (var key in keyColors.ToList())
            {
                if (RazerMappings.keyboardDictionary.TryGetValue(key.Key, out var kbIndex))
                    keyboard[kbIndex] = ToColore(key.Value);

                if (RazerMappings.mousepadDictionary.TryGetValue(key.Key, out var mousepadIndex))
                    mousepad[mousepadIndex] = ToColore(key.Value);

                if (MouseTryGetGridCoords(key.Key, out var row, out var column))
                    mouse[row, column] = ToColore(key.Value);
            }

            if (!Global.Configuration.DevicesDisableKeyboard)
                Chroma.Instance.Keyboard.SetCustom(keyboard);
            if (!Global.Configuration.DevicesDisableMouse)
                Chroma.Instance.Mousepad.SetCustom(mousepad);
            if (!Global.Configuration.DevicesDisableMouse)
                Chroma.Instance.Mouse.SetGrid(mouse);
            if (!Global.Configuration.DevicesDisableHeadset)
                Chroma.Instance.Headset.SetAll(headset);

            Chroma.Instance.Keypad.SetCustom(keypad);
            Chroma.Instance.ChromaLink.SetCustom(chromalink);

            return true;
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_query", false, "Query Razer devices", remark: "This is slow so it is disabled by default. Can be useful for debugging");
        }

        private Color ToColore(System.Drawing.Color value) => new Color(value.R, value.G, value.B);

        private void DetectDevices()
        {
            deviceNames.Clear();

            foreach (var device in DeviceGuids.Where(d => d.Name != "Razer Core Chroma"))//somehow this device is unsupported, can't query it
            {
                try
                {
                    var devInfo = Chroma.Instance.Query(device.Guid);
                    if (devInfo.Connected)
                    {
                        deviceNames.Add(device.Name);
                    }
                }
                catch (Corale.Colore.Razer.NativeCallException e)
                {
                    LogError("Error querying device: " + e.Message);
                }
            }
        }
    }
}
