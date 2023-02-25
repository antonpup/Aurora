using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Settings;

namespace Aurora.Devices.Omen
{
    public class OmenDevices : DefaultDevice
    {
        private readonly SemaphoreSlim _thisLock = new(1);
        bool kbConnected = false;
        bool peripheralConnected = false;

        List<IOmenDevice> devices;

        public override string DeviceName => "OMEN";

        public override string DeviceDetails
        {
            get
            {
                if (IsInitialized)
                {
                    string result = "";
                    foreach (var dev in devices)
                    {
                        if (dev.GetDeviceName() != string.Empty)
                        {
                            result += (" " + dev.GetDeviceName() + ";");
                        }
                    }

                    return result;
                }
                else
                {
                    return "Not Initialized";
                }
            }
        }

        public VariableRegistry RegisteredVariables => new VariableRegistry();

        protected override Task<bool> DoInitialize()
        {
            Global.kbLayout.KeyboardLayoutUpdated -= DeviceChangedHandler;
            Global.kbLayout.KeyboardLayoutUpdated += DeviceChangedHandler;

            lock (this)
            {
                if (!IsInitialized)
                {
                    devices = new List<IOmenDevice>();
                    IOmenDevice dev;
                    if ((dev = OmenKeyboard.GetOmenKeyboard()) != null)
                    {
                        devices.Add(dev);
                        kbConnected = true;
                    }

                    if ((dev = OmenMouse.GetOmenMouse()) != null)
                    {
                        devices.Add(dev);
                        peripheralConnected = true;
                    }

                    if ((dev = OmenMousePad.GetOmenMousePad()) != null)
                    {
                        devices.Add(dev);
                        peripheralConnected = true;
                    }

                    if ((dev = OmenChassis.GetOmenChassis()) != null)
                    {
                        devices.Add(dev);
                        peripheralConnected = true;
                    }

                    if ((dev = OmenSpeaker.GetOmenSpeaker()) != null)
                    {
                        devices.Add(dev);
                        peripheralConnected = true;
                    }

                    IsInitialized = (devices.Count != 0);
                }
            }
            return Task.FromResult(IsInitialized);
        }

        private void DeviceChangedHandler(object sender)
        {
            Global.logger.Info("Devices is changed. Reset Omen Devices");
            // Event handler is fire and forget
            Task.WaitAll(new[] { Shutdown(), Initialize()});
        }

        public override async Task Shutdown()
        {
            try
            {
                await this._thisLock.WaitAsync();
                if (IsInitialized)
                {
                    await Reset();

                    foreach (var dev in devices)
                    {
                        dev.Shutdown();
                    }
                    devices.Clear();
                    devices = null;
                    kbConnected = peripheralConnected = false;
                }
            }
            catch (Exception e)
            {
                Global.logger.Error("OMEN device, Exception during Shutdown. Message: " + e);
            }
            finally
            {
                _thisLock.Release();
            }

            IsInitialized = false;
        }

        protected override Task<bool> UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            try
            {
                if (e.Cancel || !IsInitialized) return Task.FromResult(false);

                foreach (var dev in devices)
                {
                    dev.SetLights(keyColors);
                }
            }
            catch (Exception ex)
            {
                Global.logger.Error("OMEN device, Exception during update device. Message: " + ex);
            }

            return Task.FromResult(true);
        }
    }
}
