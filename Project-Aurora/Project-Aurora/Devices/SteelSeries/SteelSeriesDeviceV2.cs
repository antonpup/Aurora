using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aurora.Settings;

namespace Aurora.Devices.SteelSeries
{
    public partial class SteelSeriesDeviceV2 : Device
    {
        public VariableRegistry GetRegisteredVariables()
        {
            return new VariableRegistry();
        }

        public string GetDeviceName()
        {
            throw new NotImplementedException();
        }

        public string GetDeviceDetails()
        {
            throw new NotImplementedException();
        }

        public string GetDeviceUpdatePerformance()
        {
            throw new NotImplementedException();
        }

        public bool Initialize()
        {
            loadCoreProps();
            baseObject.Add("game", "PROJECTAURORA");
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public bool IsInitialized()
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsKeyboardConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsPeripheralConnected()
        {
            throw new NotImplementedException();
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            throw new NotImplementedException();
        }

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            throw new NotImplementedException();
        }
    }
}
