using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aurora.Utils;

namespace Aurora.Settings
{
    public class AuroraConfigManager : JsonConfigManager
    {
        public AuroraConfigManager(string path, IDictionary<string, object> defaultOverrides) : base(path, defaultOverrides) { }

        protected override void InitialiseDefaults()
        {
            Set("philips_hue_switch", true);
            Set("philips_hue_set_default", 1, 1, int.MaxValue);
            Set("philips_hue_override_bitmap", false);
            Set("philips_hue_brightness", 255, 0, 255);
            Set("philips_hue_use_default", true);
            Set("philips_hue_default_color", new RealColor(Color.FromArgb(255,255,255,255)));
        }
    }
}