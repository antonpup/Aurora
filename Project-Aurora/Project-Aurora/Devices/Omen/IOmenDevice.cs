using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Aurora.Devices.Omen
{
    interface IOmenDevice
    {
        public void Shutdown();
        public void SetLights(Dictionary<DeviceKeys, Color> keyColors);
        public string GetDeviceName();
    };
}
