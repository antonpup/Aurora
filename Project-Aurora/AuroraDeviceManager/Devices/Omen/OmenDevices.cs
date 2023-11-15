using System.ComponentModel;
using System.Drawing;
using Common.Devices;

namespace AuroraDeviceManager.Devices.Omen
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
                if (!IsInitialized) return "";
                string result = "";
                foreach (var dev in devices)
                {
                    if (dev.GetDeviceName() != string.Empty)
                    {
                        result += " " + dev.GetDeviceName() + ";";
                    }
                }

                return result;

            }
        }

        protected override Task<bool> DoInitialize()
        {
            lock (this)
            {
                if (IsInitialized) return Task.FromResult(IsInitialized);

                devices = new List<IOmenDevice>();
                foreach (var omenKeyboard in OmenKeyboard.GetOmenKeyboards())
                {
                    devices.Add(omenKeyboard);
                    kbConnected = true;
                }

                foreach (var omenMouse in OmenMouse.GetOmenMice())
                {
                    devices.Add(omenMouse);
                    peripheralConnected = true;
                }
                    
                foreach (var omenMousePad in OmenMousePad.GetOmenMousePads())
                {
                    devices.Add(omenMousePad);
                    peripheralConnected = true;
                }

                IOmenDevice dev;
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

                IsInitialized = devices.Count != 0;
            }
            return Task.FromResult(IsInitialized);
        }

        protected override async Task Shutdown()
        {
            try
            {
                await this._thisLock.WaitAsync().ConfigureAwait(false);
                if (IsInitialized)
                {
                    await Reset().ConfigureAwait(false);

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
                Global.Logger.Error("OMEN device, Exception during Shutdown. Message: " + e);
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
                Global.Logger.Error("OMEN device, Exception during update device. Message: " + ex);
            }

            return Task.FromResult(true);
        }
    }
}
