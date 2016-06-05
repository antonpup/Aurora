using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles
{
    public interface GameEvent
    {
        void UpdateLights(EffectsEngine.EffectFrame frame);
        void UpdateLights(EffectsEngine.EffectFrame frame, GameState new_game_state);

        bool IsEnabled();
    }
}
