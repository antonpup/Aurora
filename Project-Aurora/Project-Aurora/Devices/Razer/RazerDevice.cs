using Aurora.Utils;
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
                if (!Chroma.SdkAvailable)
                {
                    LogError("SDK not available. Install Razer synapse");
                    return IsInitialized = false;
                }

                Chroma.Instance.Initialize();
            }
            catch (Corale.Colore.Razer.NativeCallException e)
            {
                LogError("Error initializing:" + e.Message);
                return IsInitialized = false;
            }
            catch (Exception e)
            {
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
                Chroma.Instance.SetAll(Color.Black);
                Chroma.Instance.Uninitialize();
                IsInitialized = false;
            }
            catch (Exception e)
            {
                LogError(e.Message);
            }
        }

        public override bool UpdateDevice(Dictionary<int, System.Drawing.Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!IsInitialized)
                return false;

            if (keyColors.TryGetValue((int)DeviceKeys.Peripheral_Logo, out var color))
            {
                keyboard.Set(ToColore(color));
                mousepad.Set(ToColore(color));
                mouse.Set(ToColore(color));
                headset = ToColore(color);
                chromalink.Set(ToColore(color));
                keypad.Set(ToColore(color));
            }

            foreach (var (key, clr) in keyColors)
            {
                if (RazerMappings.keyboardDictionary.TryGetValue((DeviceKeys)key, out var kbIndex))
                    keyboard[kbIndex] = ToColore(clr);

                if (RazerMappings.mousepadDictionary.TryGetValue((DeviceKeys)key, out var mousepadIndex))
                    mousepad[mousepadIndex] = ToColore(clr);

                if (RazerMappings.mouseDictionary.TryGetValue((DeviceKeys)key, out var mouseIndex))
                    mouse[mouseIndex] = ToColore(clr);
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
