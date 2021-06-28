using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Settings;
using KeyboardCustom = Colore.Effects.Keyboard.CustomKeyboardEffect;
using MousepadCustom = Colore.Effects.Mousepad.CustomMousepadEffect;
using MouseCustom = Colore.Effects.Mouse.CustomMouseEffect;
using KeypadCustom = Colore.Effects.Keypad.CustomKeypadEffect;
using ChromaLinkCustom = Colore.Effects.ChromaLink.CustomChromaLinkEffect;
using System.ComponentModel;
using System.Linq;
using Colore;
using Colore.Data;
using Colore.Api;

namespace Aurora.Devices.Razer
{
    public class RazerDevice : DefaultDevice
    {
        IChroma Chroma;
        public override string DeviceName => "Razer";

        private readonly List<(string Name, Guid Guid)> DeviceGuids = typeof(Colore.Data.Devices)
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
                Chroma = ColoreProvider.CreateNativeAsync().Result;
                var v = Chroma.SdkVersion;
            }
            catch (ColoreException e)
            {
                LogError("Error initializing:" + e.Message);
                return IsInitialized = false;
            }
            catch (AggregateException e)
            {
                LogError("SDK not available. Install Razer synapse " + e.Message);
                return IsInitialized = false;
            }

            if (!Chroma.Initialized)
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
                Chroma.SetAllAsync(Color.Black);
                Chroma.UninitializeAsync();
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
                Chroma.Keyboard.SetCustomAsync(keyboard);
            if (!Global.Configuration.DevicesDisableMouse)
                Chroma.Mousepad.SetCustomAsync(mousepad);
            if (!Global.Configuration.DevicesDisableMouse)
                Chroma.Mouse.SetGridAsync(mouse);
            if (!Global.Configuration.DevicesDisableHeadset)
                Chroma.Headset.SetAllAsync(headset);

            Chroma.Keypad.SetCustomAsync(keypad);
            Chroma.ChromaLink.SetCustomAsync(chromalink);

            return true;
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_query", false, "Query Razer devices", remark: "This is slow so it is disabled by default. Can be useful for debugging");
        }

        private Color ToColore(System.Drawing.Color value) => new Color(value.R, value.G, value.B);

        private async void DetectDevices()
        {
            deviceNames.Clear();

            foreach (var device in DeviceGuids)
            {
                try
                {
                    var devInfo =  await Chroma.QueryAsync(device.Guid);
                    if (devInfo.Connected)
                    {
                        deviceNames.Add(device.Name);
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
