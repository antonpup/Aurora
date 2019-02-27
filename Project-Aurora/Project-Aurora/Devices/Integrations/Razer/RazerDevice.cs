using Corale.Colore.Core;
using Corale.Colore.Razer.Keyboard;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Settings;
using KeyboardCustom = Corale.Colore.Razer.Keyboard.Effects.Custom;
using MousepadCustom = Corale.Colore.Razer.Mousepad.Effects.Custom;
using System.ComponentModel;
using Aurora.Devices.Layout;
using System.Drawing;
using Aurora.Devices.Layout.Layouts;

namespace Aurora.Devices.Razer
{
    class RazerDevice : Device
    {
        private String devicename = "Razer";
        private bool isInitialized = false;

        private bool keyboard_updated = false;
        private bool peripheral_updated = false;
        private KeyboardCustom grid = KeyboardCustom.Create();
        private MousepadCustom MousepadGrid = MousepadCustom.Create();
        //private bool bladeLayout = true;

        IKeyboard keyboard = null;
        IMouse mouse = null;
        IHeadset headset = null;
        IMousepad mousepad = null;
        IKeypad keypad = null;
        IChromaLink chromalink = null;

        private readonly object action_lock = new object();

        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private long lastUpdateTime = 0;

        private System.Drawing.Color previous_peripheral_Color = System.Drawing.Color.Black;

        public string GetDeviceDetails()
        {
            if (isInitialized)
            {
                return devicename + ": " + (keyboard != null ? "Keyboard Connected " : "") + (mouse != null ? "Mouse Connected " : "") + (headset != null ? "Headset Connected " : "") + (mousepad != null ? "Mousepad Connected " : "") + (chromalink != null ? "ChromaLink Connected " : "");
            }
            else
            {
                return devicename + ": Not initialized";
            }
        }

        public string GetDeviceName()
        {
            return devicename;
        }

        public string GetDeviceUpdatePerformance()
        {
            return (isInitialized ? lastUpdateTime + " ms" : "");
        }

        public VariableRegistry GetRegisteredVariables()
        {
            return new VariableRegistry();
        }

        public bool Initialize()
        {
            lock (action_lock)
            {
                if (!IsInitialized())
                {
                    try
                    {
                        if (!Chroma.SdkAvailable)
                            throw new Exception("No Chroma SDK available");

                        Chroma.Instance.Initialize();

                        Global.logger.Info("Razer device, Initialized");

                        keyboard = Chroma.Instance.Keyboard;
                        mouse = Chroma.Instance.Mouse;
                        headset = Chroma.Instance.Headset;
                        mousepad = Chroma.Instance.Mousepad;
                        keypad = Chroma.Instance.Keypad;
                        chromalink = Chroma.Instance.ChromaLink;

                        if (keyboard == null &&
                            mouse == null &&
                            headset == null &&
                            mousepad == null &&
                            keypad == null &&
                            chromalink == null
                            )
                        {
                            throw new Exception("No devices connected");
                        }
                        else
                        {

                            /*if (Chroma.Instance.Query(Corale.Colore.Razer.Devices.BladeStealth).Connected || Chroma.Instance.Query(Corale.Colore.Razer.Devices.Blade14).Connected)
                                bladeLayout = true;*/

                            if (Global.Configuration.razer_first_time)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    RazerInstallInstructions instructions = new RazerInstallInstructions();
                                    instructions.ShowDialog();
                                });
                                Global.Configuration.razer_first_time = false;
                                Settings.ConfigManager.Save(Global.Configuration);
                            }

                            isInitialized = true;
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Global.logger.Error("Razer device, Exception! Message:" + ex);
                    }

                    isInitialized = false;
                    return false;
                }

                return isInitialized;
            }
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsInitialized()
        {
            return isInitialized && Chroma.Instance.Initialized;
        }

        public bool IsKeyboardConnected()
        {
            return keyboard != null;
        }

        public bool IsPeripheralConnected()
        {
            return (mouse != null || headset != null || mousepad != null);
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            if (this.IsInitialized() && (keyboard_updated || peripheral_updated))
            {
                keyboard_updated = false;
                peripheral_updated = false;
            }
        }

        public void Shutdown()
        {
            lock (action_lock)
            {
                try
                {
                    if (IsInitialized())
                    {
                        //Chroma.Instance.Uninitialize();
                        isInitialized = false;
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.Error("Razer device, Exception during Shutdown. Message: " + exc);
                    isInitialized = false;
                }
            }
        }

        public bool UpdateDevice(System.Drawing.Color GlobalColor, List<DeviceLayout> devices, DoWorkEventArgs e, bool forced = false)
        {
            watch.Restart();

            bool updateResult = true;

            try
            {

                foreach (DeviceLayout layout in devices)
                {
                    switch (layout)
                    {
                        case KeyboardDeviceLayout kb:
                            if (!UpdateDevice(kb, e, forced))
                                updateResult = false;
                            break;
                        case MouseDeviceLayout mouse:
                            if (!UpdateDevice(mouse, e, forced))
                                updateResult = false;
                            break;
                    }

             //       SendColorToPeripheral(globalColor);
                }
            }
            catch (Exception ex)
            {
                Global.logger.Error("Razer device, error when updating device: " + ex);
                return false;
            }

            watch.Stop();
            lastUpdateTime = watch.ElapsedMilliseconds;

            return updateResult;
        }

        public bool UpdateDevice(KeyboardDeviceLayout device, DoWorkEventArgs e, bool forced = false)
        {
            if ((coord = GetKeyCoord(key.Key)) != null)
            {
                SetOneKey(coord, key.Value);
            }
            else
            {
                Key localKey = ToRazer(key.Key);
                SetOneKey(localKey, key.Value);
            }
        }

        public bool UpdateDevice(MouseDeviceLayout device, DoWorkEventArgs e, bool forced = false)
        {
            throw new NotImplementedException();
        }
    }
}
