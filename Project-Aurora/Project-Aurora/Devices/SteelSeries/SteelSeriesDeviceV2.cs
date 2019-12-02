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
            return true;
        }

        public void Shutdown()
        {
            pingTaskTokenSource.Cancel();
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
            var keyColors = colorComposition.keyColors;
            var mousePad = keyColors.Where(t => t.Key >= DeviceKeys.MOUSEPADLIGHT1 && t.Key <= DeviceKeys.MOUSEPADLIGHT12).ToList();

            return true;
        }
    }
}
