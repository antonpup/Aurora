using System.Collections.Generic;
using Aurora.EffectsEngine;
using Aurora.Profiles.Aurora_Wrapper;
using System.Linq;

namespace Aurora.Profiles.BF3
{
    public class GameEvent_BF3 : GameEvent_Aurora_Wrapper
    {
        public GameEvent_BF3()
        {
            _game_state = new GameState_Wrapper();
        }
    }
}
