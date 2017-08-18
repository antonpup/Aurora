using Aurora.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    public interface IEffectScript
    {
        string ID { get; }

        VariableRegistry Properties { get; }

        object UpdateLights(VariableRegistry properties, IGameState state = null);
    }
}
