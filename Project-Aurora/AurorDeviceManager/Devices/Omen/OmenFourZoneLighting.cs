using System.Drawing;
using Common.Devices;
using Omen.OmenFourZoneLighting;

namespace AurorDeviceManager.Devices.Omen
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
