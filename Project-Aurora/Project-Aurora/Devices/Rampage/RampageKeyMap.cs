using System.Collections.Generic;
using System.Collections.Immutable;

namespace Aurora.Devices.Rampage
{
    public static class RampageKeyMap
    {
        public static readonly IImmutableDictionary<RampageKey, DeviceKeys> MouseLightMap = new Dictionary<RampageKey, DeviceKeys>
        {
            [RampageKey.L1] = DeviceKeys.PERIPHERAL_LIGHT1,
            [RampageKey.L2] = DeviceKeys.PERIPHERAL_LIGHT2,
            [RampageKey.L3] = DeviceKeys.PERIPHERAL_LIGHT3,
            [RampageKey.L4] = DeviceKeys.PERIPHERAL_LIGHT4,
            [RampageKey.L5] = DeviceKeys.PERIPHERAL_LIGHT5,
            [RampageKey.L6] = DeviceKeys.PERIPHERAL_LIGHT6,
            [RampageKey.L7] = DeviceKeys.Peripheral_Logo,
        }.ToImmutableDictionary();
    }
}