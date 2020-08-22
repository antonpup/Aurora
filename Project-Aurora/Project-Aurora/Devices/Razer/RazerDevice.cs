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
    public class RazerDevice2 : DefaultDevice
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

        protected override string DeviceInfo => deviceNames.Any() ?
            ": " + string.Join(",", deviceNames)
            : "";

        public override bool Initialize()
        {
            if (!Chroma.SdkAvailable)
            {
                Global.logger.Error("SDK not available. Install Razer synapse");
                return IsInitialized = false;
            }

            try
            {
                Chroma.Instance.Initialize();
            }
            catch (Corale.Colore.Razer.NativeCallException e)
            {
                Global.logger.Error("Error initializing:" + e.Message);
                return IsInitialized = false;
            }

            if (!Chroma.Instance.Initialized)
            {
                Global.logger.Error("Failed to Initialize Razer Chroma sdk");
                return IsInitialized = false;
            }

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
                Global.logger.Error(e.Message);
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
                if (RazerMappings.mousepadDictionary.TryGetValue(key.Key, out var kbIndex))
                    keyboard[kbIndex] = ToColore(key.Value);

                if (RazerMappings.mousepadDictionary.TryGetValue(key.Key, out var mousepadIndex))
                    mousepad[mousepadIndex] = ToColore(key.Value);

                if (RazerMappings.mouseDictionary.TryGetValue(key.Key, out var mouseIndex))
                    mouse[mouseIndex] = ToColore(key.Value);
            }

            if(!Global.Configuration.devices_disable_keyboard)
                Chroma.Instance.Keyboard.SetCustom(keyboard);
            if (!Global.Configuration.devices_disable_mouse)
                Chroma.Instance.Mousepad.SetCustom(mousepad);
            if (!Global.Configuration.devices_disable_mouse)
                Chroma.Instance.Mouse.SetGrid(mouse);
            if (!Global.Configuration.devices_disable_headset)
                Chroma.Instance.Headset.SetAll(headset);

            Chroma.Instance.Keypad.SetCustom(keypad);
            Chroma.Instance.ChromaLink.SetCustom(chromalink);

            return true;
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
                    Global.logger.Error("Error querying device: " + e.Message);
                }
            }
        }
    }
}
