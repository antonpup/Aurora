using Aurora.Devices;
using Aurora.Profiles.Desktop;
using Aurora.Profiles.GTA5;
using Aurora.Profiles.Payday_2.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace Aurora.Utils
{
    public static class EnumUtils
    {
        public static string GetDescription(this Enum enumObj)
        {
            FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

            object[] attribArray = fieldInfo.GetCustomAttributes(false);

            if (attribArray.Length == 0)
            {
                return enumObj.ToString();
            }
            else
            {
                DescriptionAttribute attrib = attribArray[0] as DescriptionAttribute;
                return attrib.Description;
            }
        }
    }

    public static class IValueConverterExt
    {
        public static Enum StringToEnum(this IValueConverter conv, Type t, string name)
        {
            return (Enum)Enum.Parse(t, name);
        }
    }

    public class EnumToStringVC : IValueConverter
    {
        protected Type EnumType;
        protected Enum DefaultEnum;

        public Dictionary<int, string> CustomDesc { get; set; } = new Dictionary<int, string>();

        public EnumToStringVC() { }

        public EnumToStringVC(Enum val) {
            EnumType = val.GetType();
            DefaultEnum = val;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (EnumType == null)
                return null;

            if (CustomDesc.ContainsKey(value as int? ?? 0))
                return CustomDesc[(int)value];


            if (value == null || string.IsNullOrEmpty(value.ToString()) || (value.GetType() != EnumType && value.GetType() != typeof(string)))
                return DefaultEnum.GetDescription();
            return (this.StringToEnum(EnumType, value.ToString())).GetDescription();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (EnumType == null)
                return null;

            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return (Enum)Enum.Parse(EnumType, DefaultEnum.ToString());
            return this.StringToEnum(EnumType, value.ToString());
        }
    }

    public class DeviceKeysToStringVC : IValueConverter
    {
        protected Type EnumType = typeof(DeviceKeys);
        protected Enum DefaultEnum = DeviceKeys.NONE;

        public DeviceKeysToStringVC() { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (EnumType == null)
                return null;


            if (value == null || string.IsNullOrEmpty(value.ToString()) || (value.GetType() != EnumType && value.GetType() != typeof(string)))
                return DefaultEnum.GetDescription();

            DeviceKeys key = (DeviceKeys)value;

            return Global.kbLayout.KeyText.ContainsKey(key) ? Global.kbLayout.KeyText[key] : key.GetDescription();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (EnumType == null)
                return null;

            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return (Enum)Enum.Parse(EnumType, DefaultEnum.ToString());
            return this.StringToEnum(EnumType, value.ToString());
        }
    }

    public class KeysToStringVC : EnumToStringVC
    {
        public KeysToStringVC() : base(System.Windows.Forms.Keys.None) {
            CustomDesc.Add((int)System.Windows.Forms.Keys.LControlKey, "Control");
            CustomDesc.Add((int)System.Windows.Forms.Keys.LMenu, "Alt");
            CustomDesc.Add((int)System.Windows.Forms.Keys.LWin, "Win");
        }
    }

    public class PercentEffectTypeToStringVC : EnumToStringVC
    {
        public PercentEffectTypeToStringVC() : base(PercentEffectType.Progressive) { }
    }

    public class InteractiveEffectsToStringVC : EnumToStringVC
    {
        public InteractiveEffectsToStringVC() : base(InteractiveEffects.None) { }
    }

    public class IdleEffectsToStringVC : EnumToStringVC
    {
        public IdleEffectsToStringVC() : base(IdleEffects.None) { }
    }

    public class KbLayoutToStringVC : EnumToStringVC
    {
        public KbLayoutToStringVC() : base(PreferredKeyboardLocalization.None) { }
    }

    public class KbBrandToStringVC : EnumToStringVC
    {
        public KbBrandToStringVC() : base(PreferredKeyboard.None) { }
    }

    public class MouseBrandToStringVC : EnumToStringVC
    {
        public MouseBrandToStringVC() : base(PreferredMouse.None) { }
    }

    public class BitmapAccuracyToStringVC : EnumToStringVC
    {
        public BitmapAccuracyToStringVC() : base(BitmapAccuracy.Okay) { }
    }

    public class MouseOrientationToStringVC : EnumToStringVC
    {
        public MouseOrientationToStringVC() : base(MouseOrientationType.RightHanded) { }
    }

    public class GTA5_PoliceEffectsToStringVC : EnumToStringVC
    {
        public GTA5_PoliceEffectsToStringVC() : base(GTA5_PoliceEffects.Default) { }
    }

    public class GTA5_PlayerStateTypeToStringVC : EnumToStringVC
    {
        public GTA5_PlayerStateTypeToStringVC() : base(Profiles.GTA5.GSI.PlayerState.Undefined) { }
    }

    public class PD2_LevelPhaseTypeToStringVC : EnumToStringVC
    {
        public PD2_LevelPhaseTypeToStringVC() : base(LevelPhase.Undefined) { }
    }

    public class PD2_PlayerStateTypeToStringVC : EnumToStringVC
    {
        public PD2_PlayerStateTypeToStringVC() : base(Profiles.Payday_2.GSI.Nodes.PlayerState.Undefined) { }
    }

    public class PD2_GameStateTypeToStringVC : EnumToStringVC
    {
        public PD2_GameStateTypeToStringVC() : base(Profiles.Payday_2.GSI.Nodes.GameStates.Undefined) { }
    }

    public class LayerEffectsToStringVC : EnumToStringVC
    {
        public LayerEffectsToStringVC() : base(LayerEffects.None) { }
    }

    public class appexitmodeToStringVC : EnumToStringVC
    {
        public appexitmodeToStringVC() : base(AppExitMode.Ask) { }
    }

    public class AnimationTypeToStringVC : EnumToStringVC
    {
        public AnimationTypeToStringVC() : base(AnimationType.None) { }
    }

    /*public class LayerTypeToStringVC : EnumToStringVC
    {
        public LayerTypeToStringVC() : base(LayerType.Solid) { }
    }*/

    public class LogicOperatorToStringVC : EnumToStringVC
    {
        public LogicOperatorToStringVC() : base(LogicOperator.GreaterThan) { }
    }
    
    public class ActionTypeToStringVC : EnumToStringVC
    {
        public ActionTypeToStringVC() : base(ActionType.SetProperty) { }
    }

    public class AppDetectionModeToStringVC : EnumToStringVC
    {
        public AppDetectionModeToStringVC() : base(ApplicationDetectionMode.WindowsEvents) { }
    }

    public class KeycapTypeToStringVC : EnumToStringVC
    {
        public KeycapTypeToStringVC() : base(KeycapType.Default) { }
    }

    public class AmbilightEffectsToStringVC : EnumToStringVC
    {
        public AmbilightEffectsToStringVC() : base(AmbilightType.Default) { }
    }

    public class AmbilightCaptureToStringVC : EnumToStringVC
    {
        public AmbilightCaptureToStringVC() : base(AmbilightCaptureType.Everything) { }
    }

    public class EqualizerTypeToStringVC : EnumToStringVC
    {
        public EqualizerTypeToStringVC() : base(EqualizerType.PowerBars) { }
    }

    public class EqualizerPresentationTypeToStringVC : EnumToStringVC
    {
        public EqualizerPresentationTypeToStringVC() : base(EqualizerPresentationType.SolidColor) { }
    }

    public class ShortcutAssistantPresentationTypeToStringVC : EnumToStringVC
    {
        public ShortcutAssistantPresentationTypeToStringVC() : base(ShortcutAssistantPresentationType.Default) { }
    }

    public class ETS2_BeaconStyleToStringVC : EnumToStringVC
    {
        public ETS2_BeaconStyleToStringVC() : base(Profiles.ETS2.ETS2_BeaconStyle.Simple_Flash) { }
    }
}