using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Management;
using Omen.OmenFourZoneLighting;

using Aurora.Settings;

namespace Aurora.Devices.Omen
{
    class OmenFourZoneLighting : IOmenDevice
    {
        public void SetLights(Dictionary<DeviceKeys, Color> keyColors)
        {
            Task.Run(() => {
                if (Monitor.TryEnter(this))
                {
                    try
                    {
                        if(FourZoneLighting.IsTurnOn() 
                            && keyColors.ContainsKey(DeviceKeys.ENTER)
                            && keyColors.ContainsKey(DeviceKeys.J)
                            && keyColors.ContainsKey(DeviceKeys.E)
                            && keyColors.ContainsKey(DeviceKeys.A))
                        {
                            FourZoneLighting.SetZoneColors(new Color[] { keyColors[DeviceKeys.ENTER], keyColors[DeviceKeys.J], keyColors[DeviceKeys.E], keyColors[DeviceKeys.A] });
                        }

                    }
                    finally
                    {
                        Monitor.Exit(this);
                    }
                }
            });
        }

        public string GetDeviceName()
        {
            return string.Empty;
        }
        
        public void Shutdown()
        {

        }
    }
}
