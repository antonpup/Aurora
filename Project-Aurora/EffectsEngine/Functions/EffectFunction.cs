using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.EffectsEngine.Functions
{
    public interface EffectFunction
    {
        void SetXBounds(float low_bound, float high_bound);
        void SetYBounds(float low_bound, float high_bound);

        EffectPoint GetOrigin();

        EffectPoint GetPoint(float x);
        string ToString();
    }
}
