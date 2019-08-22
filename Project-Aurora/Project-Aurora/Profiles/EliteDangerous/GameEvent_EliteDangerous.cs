using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aurora.EffectsEngine;
using Aurora.Profiles.EliteDangerous.GSI;
using Aurora.Settings;

namespace Aurora.Profiles.EliteDangerous
{
    public class GameEvent_EliteDangerous : GameEvent_Generic
    {
        public GameEvent_EliteDangerous() : base()
        {
            //TODO: Read initial game configuration
        }

        public override void OnResume()
        {
            //TODO: Enable Journal API reading
        }

        public override void OnPause()
        {
            //TODO: Disable Journal API reading
        }
    }
}