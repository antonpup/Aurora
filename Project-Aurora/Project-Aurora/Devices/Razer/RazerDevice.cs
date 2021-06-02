using Colore;
using ColoreColor = Colore.Data.Color;
using System;
using System.Collections.Generic;
using Aurora.Settings;
using KeyboardCustom = Colore.Effects.Keyboard.CustomKeyboardEffect;
using MousepadCustom = Colore.Effects.Mousepad.CustomMousepadEffect;
using MouseCustom = Colore.Effects.Mouse.CustomMouseEffect;
using KeypadCustom = Colore.Effects.Keypad.CustomKeypadEffect;
using ChromaLinkCustom = Colore.Effects.ChromaLink.CustomChromaLinkEffect;
using System.ComponentModel;
using System.Linq;

namespace Aurora.Devices.Razer
{
    public class RazerDevice : DefaultDevice
    {
        public override string DeviceName => "Razer";

        private readonly List<(string Name, Guid Guid)> DeviceGuids = typeof(Colore.Data.Devices)
            .GetFields()
            .Select(f => (f.Name, (Guid)f.GetValue(null)))
            .ToList();

        private KeyboardCustom keyboard = KeyboardCustom.Create();
        private MousepadCustom mousepad = MousepadCustom.Create();
        private MouseCustom mouse = MouseCustom.Create();
        private ColoreColor headset = ColoreColor.Black;
        private KeypadCustom keypad = KeypadCustom.Create();
        private ChromaLinkCustom chromalink = ChromaLinkCustom.Create();
        private IChroma chroma;

        private readonly List<string> deviceNames = new List<string>();

        protected override string DeviceInfo => string.Join(", ", deviceNames);

        public override bool Initialize()
        {
            try
            {
                var task = ColoreProvider.CreateNativeAsync();
                chroma = task.GetAwaiter().GetResult();
            }

            catch (Exception)
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
                chroma.SetAllAsync(ColoreColor.Black);
                chroma.UninitializeAsync();
                IsInitialized = false;
            }
            catch (Exception e)
            {
                LogError(e.Message);
            }
        }

        public override bool UpdateDevice(Dictionary<DeviceKeys, System.Drawing.Color> keyColors, DoWorkEventArgs e, bool forced = false)
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

            foreach (var key in keyColors)
            {
                if (RazerMappings.keyboardDictionary.TryGetValue(key.Key, out var kbIndex))
                    keyboard[kbIndex] = ToColore(key.Value);

                if (RazerMappings.mousepadDictionary.TryGetValue(key.Key, out var mousepadIndex))
                    mousepad[mousepadIndex] = ToColore(key.Value);

                if (RazerMappings.mouseDictionary.TryGetValue(key.Key, out var mouseIndex))
                    mouse[mouseIndex] = ToColore(key.Value);
            }

            if (!Global.Configuration.DevicesDisableKeyboard)
                chroma.Keyboard.SetCustomAsync(keyboard);
            if (!Global.Configuration.DevicesDisableMouse)
                chroma.Mousepad.SetCustomAsync(mousepad);
            if (!Global.Configuration.DevicesDisableMouse)
                chroma.Mouse.SetGridAsync(mouse);
            if (!Global.Configuration.DevicesDisableHeadset)
                chroma.Headset.SetAllAsync(headset);

            chroma.Keypad.SetCustomAsync(keypad);
            chroma.ChromaLink.SetCustomAsync(chromalink);

            return true;
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_query", false, "Query Razer devices", remark: "This is slow so it is disabled by default. Can be useful for debugging");
        }

        private ColoreColor ToColore(System.Drawing.Color value) => new ColoreColor(value.R, value.G, value.B);

        private void DetectDevices()
        {
            deviceNames.Clear();
            foreach (var device in DeviceGuids.Where(d => d.Guid != Colore.Data.Devices.Core))  //somehow this device is unsupported, can't query it
            {
                try
                {
                    var devInfo = chroma.QueryAsync(device.Guid).GetAwaiter().GetResult();
                    if (devInfo.Connected)
                    {
                        deviceNames.Add(devInfo.Name);
                    }
                }
                catch (ColoreException e)
                {
                    LogError("Error querying device: " + e.Message);
                }
            }
        }
    }
}
