using Aurora.Devices.RGBNet;
using RGB.NET.Core;

namespace Aurora.Devices.Corsair
{
    public class CorsairRGBNetBrush : AuroraRGBNetBrush
    {
        #region Constructors

        public CorsairRGBNetBrush()
        {
            LedMapping[LedId.Keyboard_Custom1] = DeviceKeys.ADDITIONALLIGHT1;
            LedMapping[LedId.Keyboard_Custom2] = DeviceKeys.ADDITIONALLIGHT2;
            LedMapping[LedId.Keyboard_Custom3] = DeviceKeys.ADDITIONALLIGHT3;
            LedMapping[LedId.Keyboard_Custom4] = DeviceKeys.ADDITIONALLIGHT4;
            LedMapping[LedId.Keyboard_Custom5] = DeviceKeys.ADDITIONALLIGHT5;
            LedMapping[LedId.Keyboard_Custom6] = DeviceKeys.ADDITIONALLIGHT6;
            LedMapping[LedId.Keyboard_Custom7] = DeviceKeys.ADDITIONALLIGHT7;
            LedMapping[LedId.Keyboard_Custom8] = DeviceKeys.ADDITIONALLIGHT8;
            LedMapping[LedId.Keyboard_Custom9] = DeviceKeys.ADDITIONALLIGHT9;
            LedMapping[LedId.Keyboard_Custom10] = DeviceKeys.ADDITIONALLIGHT10;
            LedMapping[LedId.Keyboard_Custom11] = DeviceKeys.ADDITIONALLIGHT11;
            LedMapping[LedId.Keyboard_Custom12] = DeviceKeys.ADDITIONALLIGHT12;
            LedMapping[LedId.Keyboard_Custom13] = DeviceKeys.ADDITIONALLIGHT13;
            LedMapping[LedId.Keyboard_Custom14] = DeviceKeys.ADDITIONALLIGHT14;
            LedMapping[LedId.Keyboard_Custom15] = DeviceKeys.ADDITIONALLIGHT15;
            LedMapping[LedId.Keyboard_Custom16] = DeviceKeys.ADDITIONALLIGHT16;
            LedMapping[LedId.Keyboard_Custom17] = DeviceKeys.ADDITIONALLIGHT17;
            LedMapping[LedId.Keyboard_Custom18] = DeviceKeys.ADDITIONALLIGHT18;
            LedMapping[LedId.Keyboard_Custom19] = DeviceKeys.ADDITIONALLIGHT19;
            //LedMapping[LedId.Keyboard_Custom20]  = LedProgramming - Not available
            LedMapping[LedId.Keyboard_Custom21] = DeviceKeys.FN_Key;
        }

        #endregion
    }
}
