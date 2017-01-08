using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Profiles.TheDivision
{
    public class GameEvent_TheDivision : GameEvent_Aurora_Wrapper
    {
        public GameEvent_TheDivision()
        {
        }

        public override bool IsEnabled()
        {
            return this.Profile.Settings.isEnabled;
        }
    }
}
