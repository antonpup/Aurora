using RGB.NET.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices.RGBNet
{
    internal class CustomRGBNetBrush : AuroraRGBNetBrush
    {
        public CustomRGBNetBrush(string path)
        {
            if (!File.Exists(path))
                return;
            LedMapping.Clear();
            var lines = File.ReadAllLines(path).Where(line => !line.StartsWith("//") && !string.IsNullOrWhiteSpace(line)).ToArray();

            for (int i = 0; i < lines.Length; i++)
            {
                try
                {
                    var line = lines[i];

                    var ids = line.Replace(" ", "").Split(',');
                    var ledid = ids[0].Replace("LedId.", "");
                    var devicekey = ids[1].Replace("DeviceKeys.", "");

                    if (Enum.TryParse(ledid, out LedId led) && Enum.TryParse(devicekey, out DeviceKeys dev))
                        LedMapping.Add(led, dev);
                    else
                        throw new FormatException();
                }
                catch (Exception)
                {
                    Global.logger.Error($"Error parsing LedMappings file in line " + i + ": " + lines[i]);
                }
            }

            Global.logger.Info("Parsed LedMappings file with " + LedMapping.Count + " entries");
        }
    }
}