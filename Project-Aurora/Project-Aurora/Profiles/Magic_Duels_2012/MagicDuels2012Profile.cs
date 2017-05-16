using System.Collections.Generic;
using Aurora.Settings;

namespace Aurora.Profiles.Magic_Duels_2012
{
    public class MagicDuels2012Profile : ApplicationProfile
    {
        //Effects
        //// Lighting Areas
        public List<ColorZone> lighting_areas { get; set; }

        public MagicDuels2012Profile() : base()
        {
            
        }

        public override void Reset()
        {
            base.Reset();
            //Effects
            //// Lighting Areas
            lighting_areas = new List<ColorZone>() { };
        }
    }
}
