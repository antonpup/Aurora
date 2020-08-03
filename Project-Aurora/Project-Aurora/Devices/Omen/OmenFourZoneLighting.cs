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
                            && keyColors.ContainsKey(DeviceKeys.ADDITIONALLIGHT1)
                            && keyColors.ContainsKey(DeviceKeys.ADDITIONALLIGHT2)
                            && keyColors.ContainsKey(DeviceKeys.ADDITIONALLIGHT3)
                            && keyColors.ContainsKey(DeviceKeys.ADDITIONALLIGHT4))
                        {
                            FourZoneLighting.SetZoneColors(
                                new Color[] { keyColors[DeviceKeys.ADDITIONALLIGHT1], 
                                    keyColors[DeviceKeys.ADDITIONALLIGHT2], 
                                    keyColors[DeviceKeys.ADDITIONALLIGHT3], 
                                    keyColors[DeviceKeys.ADDITIONALLIGHT4] });
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
