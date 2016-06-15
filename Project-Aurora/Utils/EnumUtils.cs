using Aurora.Profiles.Desktop;
using Aurora.Profiles.GTA5;
using Aurora.Profiles.GTA5.GSI;
using Aurora.Profiles.Payday_2.GSI.Nodes;
using Aurora.Settings;
using System;
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

    public class DeviceKeysToStringVC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return Devices.DeviceKeys.NONE;
            return (StringToEnum<Devices.DeviceKeys>(value.ToString())).GetDescription();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return Devices.DeviceKeys.NONE;
            return StringToEnum<Devices.DeviceKeys>(value.ToString());
        }

        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }
    }

    public class PercentEffectTypeToStringVC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return PercentEffectType.Progressive;
            return (StringToEnum<PercentEffectType>(value.ToString())).GetDescription();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return PercentEffectType.Progressive;
            return StringToEnum<PercentEffectType>(value.ToString());
        }

        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }
    }

    public class InteractiveEffectsToStringVC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return InteractiveEffects.None;
            return (StringToEnum<InteractiveEffects>(value.ToString())).GetDescription();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return InteractiveEffects.None;
            return StringToEnum<InteractiveEffects>(value.ToString());
        }

        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }
    }

    public class IdleEffectsToStringVC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return IdleEffects.None;
            return (StringToEnum<IdleEffects>(value.ToString())).GetDescription();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return IdleEffects.None;
            return StringToEnum<IdleEffects>(value.ToString());
        }

        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }
    }

    public class KbLayoutToStringVC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return PreferredKeyboardLocalization.None;
            return (StringToEnum<PreferredKeyboardLocalization>(value.ToString())).GetDescription();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return PreferredKeyboardLocalization.None;
            return StringToEnum<PreferredKeyboardLocalization>(value.ToString());
        }

        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }
    }

    public class KbBrandToStringVC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return PreferredKeyboard.None;
            return (StringToEnum<PreferredKeyboard>(value.ToString())).GetDescription();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return PreferredKeyboard.None;
            return StringToEnum<PreferredKeyboard>(value.ToString());
        }

        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }
    }

    public class GTA5_PoliceEffectsToStringVC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return GTA5_PoliceEffects.Default;
            return (StringToEnum<GTA5_PoliceEffects>(value.ToString())).GetDescription();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return GTA5_PoliceEffects.Default;
            return StringToEnum<GTA5_PoliceEffects>(value.ToString());
        }

        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }
    }

    public class GTA5_PlayerStateTypeToStringVC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return Profiles.GTA5.GSI.PlayerState.Undefined;
            return (StringToEnum<Profiles.GTA5.GSI.PlayerState>(value.ToString())).GetDescription();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return Profiles.GTA5.GSI.PlayerState.Undefined;
            return StringToEnum<Profiles.GTA5.GSI.PlayerState>(value.ToString());
        }

        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }
    }

    public class PD2_LevelPhaseTypeToStringVC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return LevelPhase.Undefined;
            return (StringToEnum<LevelPhase>(value.ToString())).GetDescription();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return LevelPhase.Undefined;
            return StringToEnum<LevelPhase>(value.ToString());
        }

        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }
    }

    public class PD2_PlayerStateTypeToStringVC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return Profiles.Payday_2.GSI.Nodes.PlayerState.Undefined;
            return (StringToEnum<Profiles.Payday_2.GSI.Nodes.PlayerState>(value.ToString())).GetDescription();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return Profiles.Payday_2.GSI.Nodes.PlayerState.Undefined;
            return StringToEnum<Profiles.Payday_2.GSI.Nodes.PlayerState>(value.ToString());
        }

        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }
    }

    public class PD2_GameStateTypeToStringVC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return Profiles.Payday_2.GSI.Nodes.GameStates.Undefined;
            return (StringToEnum<Profiles.Payday_2.GSI.Nodes.GameStates>(value.ToString())).GetDescription();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return Profiles.Payday_2.GSI.Nodes.GameStates.Undefined;
            return StringToEnum<Profiles.Payday_2.GSI.Nodes.GameStates>(value.ToString());
        }

        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }
    }

    public class LayerEffectsToStringVC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return LayerEffects.None;
            return (StringToEnum<LayerEffects>(value.ToString())).GetDescription();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return LayerEffects.None;
            return StringToEnum<LayerEffects>(value.ToString());
        }

        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }
    }

    public class appexitmodeToStringVC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return AppExitMode.Ask;
            return (StringToEnum<AppExitMode>(value.ToString())).GetDescription();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return AppExitMode.Ask;
            return StringToEnum<AppExitMode>(value.ToString());
        }

        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }
    }
}
