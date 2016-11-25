﻿using Aurora.Profiles.Desktop;
using Aurora.Profiles.GTA5;
using Aurora.Profiles.Payday_2.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
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

        public EnumToStringVC() { }

        public EnumToStringVC(Enum val) {
            EnumType = val.GetType();
            DefaultEnum = val;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (EnumType == null)
                return null;
            
            if (value == null || string.IsNullOrEmpty(value.ToString()) || (value.GetType() != EnumType && value.GetType() != typeof(string)))
                return DefaultEnum.GetDescription();
            return (this.StringToEnum(EnumType, value.ToString())).GetDescription();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (EnumType == null)
                return null;

            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return (Enum)Enum.Parse(EnumType, DefaultEnum.ToString()); ;
            return this.StringToEnum(EnumType, value.ToString());
        }
    }

    public class DeviceKeysToStringVC : EnumToStringVC
    {
        public DeviceKeysToStringVC() : base(Devices.DeviceKeys.NONE) { }
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

    public class LayerTypeToStringVC : EnumToStringVC
    {
        public LayerTypeToStringVC() : base(LayerType.Solid) { }
    }

    public class LogicOperatorToStringVC : EnumToStringVC
    {
        public LogicOperatorToStringVC() : base(LogicOperator.GreaterThan) { }
    }
}
