using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    public class LayerEffectConfig
    {
        public Color primary;
        public Color secondary;
        public float speed;
        public float angle;

        [JsonIgnoreAttribute]
        public float shift_amount = 0.0f;
        [JsonIgnoreAttribute]
        public long last_effect_call = 0L;

        public LayerEffectConfig()
        {
            primary = Color.Red;
            secondary = Color.Blue;
            speed = 1.0f;
            angle = 0.0f;
        }

        public LayerEffectConfig(LayerEffectConfig otherConfig)
        {
            primary = otherConfig.primary;
            secondary = otherConfig.secondary;
            speed = otherConfig.speed;
            angle = otherConfig.angle;
        }
    }
}
