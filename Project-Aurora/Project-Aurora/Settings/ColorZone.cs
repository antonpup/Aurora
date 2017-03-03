using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    public class ColorZone
    {
        public string name;
        public KeySequence keysequence;
        public Color color;
        public LayerEffects effect;
        public LayerEffectConfig effect_config;

        public ColorZone()
        {
            name = "New Zone";
            keysequence = new KeySequence();
            color = Color.Black;
            effect = LayerEffects.None;
            effect_config = new LayerEffectConfig();
            GenerateRandomColor();
        }
        
        public ColorZone(ColorZone otherZone)
        {
            name = otherZone.name;
            keysequence = new KeySequence(otherZone.keysequence);
            color = otherZone.color;
            effect = otherZone.effect;
            effect_config = new LayerEffectConfig(otherZone.effect_config);
        }

        public ColorZone(string zone_name = "New Zone")
        {
            name = zone_name;
            keysequence = new KeySequence();
            color = Color.Black;
            effect = LayerEffects.None;
            effect_config = new LayerEffectConfig();
            GenerateRandomColor();
        }

        public ColorZone(Devices.DeviceKeys[] zone_keys, string zone_name = "New Zone")
        {
            name = zone_name;
            keysequence = new KeySequence(zone_keys);
            color = Color.Black;
            effect = LayerEffects.None;
            effect_config = new LayerEffectConfig();
            GenerateRandomColor();
        }

        public ColorZone(Devices.DeviceKeys[] zone_keys, LayerEffects zone_effect, string zone_name = "New Zone")
        {
            name = zone_name;
            keysequence = new KeySequence(zone_keys);
            color = Color.Black;
            effect = zone_effect;
            effect_config = new LayerEffectConfig();
            GenerateRandomColor();
        }

        public ColorZone(Devices.DeviceKeys[] zone_keys, Color zone_color, string zone_name = "New Zone")
        {
            name = zone_name;
            keysequence = new KeySequence(zone_keys);
            color = zone_color;
            effect = LayerEffects.None;
            effect_config = new LayerEffectConfig();
        }

        private void GenerateRandomColor()
        {
            Random r = new Random(Utils.Time.GetSeconds());
            color = Color.FromArgb(r.Next(255), r.Next(255), r.Next(255));
        }

        public override string ToString()
        {
            return this.name;
        }
    }
}
