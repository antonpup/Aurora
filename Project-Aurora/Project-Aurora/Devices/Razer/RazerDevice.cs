using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Settings;
using Colore;
using Colore.Data;
using Colore.Effects.ChromaLink;
using Colore.Effects.Keyboard;
using Colore.Effects.Keypad;
using Colore.Effects.Mouse;
using Colore.Effects.Mousepad;
using Colore.Native;

namespace Aurora.Devices.Razer
{
    public class RazerDevice : DefaultDevice
    {
        private static readonly AppInfo appInfo = new("Aurora-RGB", string.Empty, string.Empty, string.Empty, Category.Application);
        private static SemaphoreSlim initializeSemaphore = new(1);
        private static IChroma chroma;

        private readonly List<(string Name, Guid Guid)> DeviceGuids = typeof(RazerDevice)
            .GetFields()
            .Select(f => (f.Name, (Guid)f.GetValue(null)))
            .ToList();
        private readonly List<string> deviceNames = new();

        private CustomKeyboardEffect keyboard = CustomKeyboardEffect.Create();
        private CustomMousepadEffect mousepad = CustomMousepadEffect.Create();
        private CustomMouseEffect mouse = CustomMouseEffect.Create();
        private Color headset = Color.Black;
        private CustomKeypadEffect keypad = CustomKeypadEffect.Create();
        private CustomChromaLinkEffect chromalink = CustomChromaLinkEffect.Create();

        public override string DeviceName => "Razer";

        protected override string DeviceInfo => string.Join(", ", deviceNames);

        protected override async Task<bool> DoInitialize()
        {
            if (chroma is null)
            {
                await initializeSemaphore.WaitAsync();
                if (chroma is null)
                {
                    try
                    {
                        chroma = await ColoreProvider.CreateNativeAsync();
                    }
                    catch (ColoreException ce) when (ce.Message.Contains("126"))
                    {
                        LogInfo("Chroma SDK unavailable.");
                        IsInitialized = false;
                        return false;
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error initializing: {ex.Message}");
                        IsInitialized = false;
                        return false;
                    }
                    finally
                    {
                        initializeSemaphore.Release();
                    }
                }
            }

            if (!chroma.Initialized)
            {
                await initializeSemaphore.WaitAsync();
                if (!chroma.Initialized)
                {
                    try
                    {
                        await chroma.InitializeAsync(appInfo);
                        IsInitialized = true;
                    }
                    catch (Exception e)
                    {
                        LogError("Error initializing:", e);
                        IsInitialized = false;
                        return false;
                    }
                    finally
                    {
                        initializeSemaphore.Release();
                    }
                }

                if (!chroma.Initialized)
                {
                    LogError("Failed to Initialize Razer Chroma sdk");
                    IsInitialized = false;
                    return false;
                }
            }

            if (Global.Configuration.VarRegistry.GetVariable<bool>($"{DeviceName}_query"))
            {
                await DetectDevices();
            }

            return true;
        }

        public override async Task Shutdown()
        {
            if (!IsInitialized)
                return;

            try
            {
                await chroma.UninitializeAsync();
                IsInitialized = false;
            }
            catch (Exception e)
            {
                LogError("Error shutting down", e);
            }
        }

        protected override async Task<bool> UpdateDevice(Dictionary<DeviceKeys, System.Drawing.Color> keyColors, DoWorkEventArgs e, bool forced = false)
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

            List<Task> tasks = new();
            if (!Global.Configuration.DevicesDisableKeyboard)
                tasks.Add(chroma.Keyboard.SetCustomAsync(keyboard));
            if (!Global.Configuration.DevicesDisableMouse)
                tasks.Add(chroma.Mousepad.SetCustomAsync(mousepad));
            if (!Global.Configuration.DevicesDisableMouse)
                tasks.Add(chroma.Mouse.SetGridAsync(mouse));
            if (!Global.Configuration.DevicesDisableHeadset)
                tasks.Add(chroma.Headset.SetAllAsync(headset));

            tasks.Add(chroma.Keypad.SetCustomAsync(keypad));
            tasks.Add(chroma.ChromaLink.SetCustomAsync(chromalink));

            await Task.WhenAll(tasks);

            return true;
        }

        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_query", false, "Query Razer devices", remark: "This is slow so it is disabled by default. Can be useful for debugging");
        }

        private Color ToColore(System.Drawing.Color value) => new Color(value.R, value.G, value.B);

        private async Task DetectDevices()
        {
            deviceNames.Clear();

            foreach (var device in DeviceGuids.Where(d => d.Name != "Razer Core Chroma"))//somehow this device is unsupported, can't query it
            {
                try
                {
                    var devInfo = await chroma.QueryAsync(device.Guid);
                    if (devInfo.Connected)
                    {
                        deviceNames.Add(device.Name);
                    }
                }
                catch (NativeCallException e)
                {
                    LogError("Error querying device: ", e);
                }
            }
        }
    }
}
