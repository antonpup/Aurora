using RGB.NET.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices.RGBNet
{
    internal class CustomRGBNetBrush : AuroraRGBNetBrush
    {
        new protected Dictionary<LedId, DeviceKeys> LedMapping { get; } = new Dictionary<LedId, DeviceKeys>();

        public CustomRGBNetBrush(string path)
        {
            try
            {
                foreach (var line in System.IO.File.ReadAllLines(path).Where(d => !d.StartsWith("//") && !string.IsNullOrWhiteSpace(d)))
                {
                    var ids = line.Split(',');
                    var ledid = ids[0].Replace("LedId.", "");
                    var devicekey = ids[1].Replace(" DeviceKeys.", "");

                    if (Enum.TryParse(ledid, out LedId led) && Enum.TryParse(devicekey, out DeviceKeys dev))
                        LedMapping.Add(led, dev);
                }
                Global.logger.Info("Parsed LedMappings file with " +  LedMapping.Count +  " entries");
            }
            catch(Exception e)
            {
                Global.logger.Error("Error parsing LedMappings file:" + e.Message);
            }
        }
    }
}