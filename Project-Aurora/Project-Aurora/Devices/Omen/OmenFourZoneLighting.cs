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
        public void SetLights(Dictionary<int, Color> keyColors)
        {
            Task.Run(() => {
                if (Monitor.TryEnter(this))
                {
                    try
                    {
                        if(FourZoneLighting.IsTurnOn() 
                            && keyColors.ContainsKey((int)DeviceKeys.ADDITIONALLIGHT1)
                            && keyColors.ContainsKey((int)DeviceKeys.ADDITIONALLIGHT2)
                            && keyColors.ContainsKey((int)DeviceKeys.ADDITIONALLIGHT3)
                            && keyColors.ContainsKey((int)DeviceKeys.ADDITIONALLIGHT4))
                        {
                            FourZoneLighting.SetZoneColors(
                                new Color[] { keyColors[(int)DeviceKeys.ADDITIONALLIGHT1], 
                                    keyColors[(int)DeviceKeys.ADDITIONALLIGHT2], 
                                    keyColors[(int)DeviceKeys.ADDITIONALLIGHT3], 
                                    keyColors[(int)DeviceKeys.ADDITIONALLIGHT4] });
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
