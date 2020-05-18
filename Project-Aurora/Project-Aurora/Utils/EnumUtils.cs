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
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;

namespace Aurora.Utils
{
    public static class EnumUtils
    {
        public static string GetDescription(this Enum enumObj)
        {
            return enumObj.GetType().GetField(enumObj.ToString()).GetCustomAttribute(typeof(DescriptionAttribute), false) is DescriptionAttribute attr ? attr.Description : enumObj.ToString();
        }

        public static string GetCategory(this Enum enumObj) {
            return enumObj.GetType().GetField(enumObj.ToString()).GetCustomAttribute(typeof(CategoryAttribute), false) is CategoryAttribute attr ? attr.Category : "";
        }

        /// <summary>Takes a particular type of enum and returns all values of the enum and their associated description in a list suitable for use as an ItemsSource.
        /// Returns an enumerable of KeyValuePairs where the key is the description/name and the value is the enum's value.</summary>
        /// <typeparam name="T">The type of enum whose values to fetch.</typeparam>
        public static IEnumerable<KeyValuePair<string, T>> GetEnumItemsSource<T>() =>
            typeof(T).GetEnumValues().Cast<T>().Select(@enum => new KeyValuePair<string, T>(
                typeof(T).GetMember(@enum.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? @enum.ToString(),
                @enum
            ));

        /// <summary>Takes a particular type of enum and returns all values of the enum and their associated description in a list suitable for use as an ItemsSource.
        /// Returns an enumerable of KeyValuePairs where the key is the description/name and the value is the enum's value.</summary>
        /// <param name="enumType">The type of enum whose values to fetch.</param>
        public static IEnumerable<KeyValuePair<string, object>> GetEnumItemsSource(Type enumType) =>
            enumType.GetEnumValues().Cast<object>().Select(@enum => new KeyValuePair<string, object>(
                enumType.GetMember(@enum.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? @enum.ToString(),
                @enum
            ));

        /// <summary>Returns the attribute of the given type for this enum.</summary>
        public static TAttr GetCustomAttribute<TAttr>(this Enum enumObj) where TAttr : Attribute =>
            enumObj.GetType().GetField(enumObj.ToString()).GetCustomAttribute(typeof(TAttr), false) as TAttr;

        /// <summary>
        /// Tries to parse a string into a given enum. Returns the parsed enum if successful, and <paramref name="defaultValue"/> if not.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T TryParseOr<T>(string value, bool ignoreCase, T defaultValue) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enum");

            if (Enum.TryParse<T>(value, ignoreCase, out var res))
                return res;
            else
                return defaultValue;
        }
    }

    /// <summary>
    /// Markup Extension that takes an enum type and returns a collection of anonymous objects containing all the enum values, with "Text"
    /// as the <see cref="DescriptionAttribute"/> of the enum item, "Value" as the enum value itself and "Group" as the <see cref="CategoryAttribute"/>.
    /// <para>Set the <see cref="System.Windows.Controls.ItemsControl.DisplayMemberPath"/> to "Text" and
    /// <see cref="System.Windows.Controls.Primitives.Selector.SelectedValuePath"/> to "Value".</para>
    /// </summary>
    public class EnumToItemsSourceExtension : MarkupExtension {

        private readonly Type enumType;

        public bool DoGroup { get; set; } = false;

        public EnumToItemsSourceExtension(Type enumType) {
            this.enumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => enumType == null ? (object)new { } : GetListFor(enumType, DoGroup);

        /// <summary>
        /// Creates a <see cref="ListCollectionView"/> for the given enum type. The items in the collection have properties: 'Text', 'Value',
        /// 'Group', 'LocalizationKey' and 'LocalizationPackage'.
        /// </summary>
        public static ListCollectionView GetListFor(Type enumType, bool doGroup = false) {
            var lcv = new ListCollectionView(Enum.GetValues(enumType)
                .Cast<Enum>()
                .Select(e => new {
                    Text = e.GetDescription(),
                    Value = e,
                    Group = e.GetCustomAttribute<CategoryAttribute>()?.Category ?? ""
                })
                .ToList()
            );
            if (doGroup) lcv.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            return lcv;
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

            string descriptionString = (this.StringToEnum(EnumType, value.ToString())).GetDescription();
            if (EnumType == typeof(PreferredKeyboard) && descriptionString.Contains("UNIWILL"))
            {
               string _CustomDescription =  CustomDescription(EnumType, descriptionString);
                if (_CustomDescription != null)
                    return _CustomDescription;
             }


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
   
    
        private string CustomDescription(Type EnumType  ,string descriptionString)
        {
             
            try
            {
                string oemstring = UniwillSDKDLL.KeyboardFactory.GetOEMName();
                if (oemstring.Equals("XMG"))
                {
                    if (descriptionString.Contains("UNIWILL2P1"))
                        return descriptionString.Replace("UNIWILL2P1", oemstring + " FUSION");
                    else if (descriptionString.Contains("UNIWILL2ND"))
                        return descriptionString.Replace("UNIWILL2ND", oemstring + " NEO");
                    else if (descriptionString.Contains("UNIWILL2P2"))
                        return descriptionString.Replace("UNIWILL2P2", oemstring + " NEO 15");
                }
                else
                {
                    if (descriptionString.Contains("UNIWILL2P1"))
                        return descriptionString.Replace("UNIWILL2P1", oemstring + " 550");
                    else if (descriptionString.Contains("UNIWILL2ND"))
                        return descriptionString.Replace("UNIWILL2ND", oemstring + " 35X");
                    else if (descriptionString.Contains("UNIWILL2P2"))
                        return descriptionString.Replace("UNIWILL2P2", oemstring + " 650");
                }
            }
            catch
            {
                return null;
            }
  
            return null;
        }
    
    
    }

    public class DeviceKeyToStringVC : IValueConverter
    {
        protected Type EnumType = typeof(DeviceKeys);
        protected Enum DefaultEnum = DeviceKeys.NONE;

        public DeviceKeyToStringVC() { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (EnumType == null)
                return null;


            if (value == null || string.IsNullOrEmpty(value.ToString()) || (value.GetType() != EnumType && value.GetType() != typeof(string)))
                return DefaultEnum.GetDescription();

            DeviceKeys key = (DeviceKeys)((DeviceKey)value).Tag;

            //Better description for these keys by using the DeviceKeys description instead
            switch (key)
            {
                case DeviceKeys.NUM_ASTERISK:
                case DeviceKeys.NUM_EIGHT:
                case DeviceKeys.NUM_ENTER:
                case DeviceKeys.NUM_FIVE:
                case DeviceKeys.NUM_FOUR:
                case DeviceKeys.NUM_MINUS:
                case DeviceKeys.NUM_NINE:
                case DeviceKeys.NUM_ONE:
                case DeviceKeys.NUM_PERIOD:
                case DeviceKeys.NUM_PLUS:
                case DeviceKeys.NUM_SEVEN:
                case DeviceKeys.NUM_SIX:
                case DeviceKeys.NUM_SLASH:
                case DeviceKeys.NUM_THREE:
                case DeviceKeys.NUM_TWO:
                case DeviceKeys.NUM_ZERO:
                case DeviceKeys.NUM_ZEROZERO:
                case DeviceKeys.ADDITIONALLIGHT1:
                case DeviceKeys.ADDITIONALLIGHT2:
                case DeviceKeys.ADDITIONALLIGHT3:
                case DeviceKeys.ADDITIONALLIGHT4:
                case DeviceKeys.ADDITIONALLIGHT5:
                case DeviceKeys.ADDITIONALLIGHT6:
                case DeviceKeys.ADDITIONALLIGHT7:
                case DeviceKeys.ADDITIONALLIGHT8:
                case DeviceKeys.ADDITIONALLIGHT9:
                case DeviceKeys.ADDITIONALLIGHT10:
                case DeviceKeys.ADDITIONALLIGHT11:
                case DeviceKeys.ADDITIONALLIGHT12:
                case DeviceKeys.ADDITIONALLIGHT13:
                case DeviceKeys.ADDITIONALLIGHT14:
                case DeviceKeys.ADDITIONALLIGHT15:
                case DeviceKeys.ADDITIONALLIGHT16:
                case DeviceKeys.ADDITIONALLIGHT17:
                case DeviceKeys.ADDITIONALLIGHT18:
                case DeviceKeys.ADDITIONALLIGHT19:
                case DeviceKeys.ADDITIONALLIGHT20:
                case DeviceKeys.ADDITIONALLIGHT21:
                case DeviceKeys.ADDITIONALLIGHT22:
                case DeviceKeys.ADDITIONALLIGHT23:
                case DeviceKeys.ADDITIONALLIGHT24:
                case DeviceKeys.ADDITIONALLIGHT25:
                case DeviceKeys.ADDITIONALLIGHT26:
                case DeviceKeys.ADDITIONALLIGHT27:
                case DeviceKeys.ADDITIONALLIGHT28:
                case DeviceKeys.ADDITIONALLIGHT29:
                case DeviceKeys.ADDITIONALLIGHT30:
                case DeviceKeys.ADDITIONALLIGHT31:
                case DeviceKeys.ADDITIONALLIGHT32:
                case DeviceKeys.LEFT_CONTROL:
                case DeviceKeys.LEFT_WINDOWS:
                case DeviceKeys.LEFT_ALT:
                case DeviceKeys.LEFT_SHIFT:
                case DeviceKeys.RIGHT_ALT:
                case DeviceKeys.FN_Key:
                case DeviceKeys.RIGHT_WINDOWS:
                case DeviceKeys.RIGHT_CONTROL:
                case DeviceKeys.RIGHT_SHIFT:
                case DeviceKeys.MOUSEPADLIGHT1:
                case DeviceKeys.MOUSEPADLIGHT2:
                case DeviceKeys.MOUSEPADLIGHT3:
                case DeviceKeys.MOUSEPADLIGHT4:
                case DeviceKeys.MOUSEPADLIGHT5:
                case DeviceKeys.MOUSEPADLIGHT6:
                case DeviceKeys.MOUSEPADLIGHT7:
                case DeviceKeys.MOUSEPADLIGHT8:
                case DeviceKeys.MOUSEPADLIGHT9:
                case DeviceKeys.MOUSEPADLIGHT10:
                case DeviceKeys.MOUSEPADLIGHT11:
                case DeviceKeys.MOUSEPADLIGHT12:
                case DeviceKeys.MOUSEPADLIGHT13:
                case DeviceKeys.MOUSEPADLIGHT14:
                case DeviceKeys.MOUSEPADLIGHT15:
                    return key.GetDescription();
            }
            //TODO
            return key.GetDescription();//Global.kbLayout.GetVisualName(key) ?? key.GetDescription();
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

    public class KeyboardPhysicalLayoutToStringVC : EnumToStringVC
    {
        public KeyboardPhysicalLayoutToStringVC() : base(KeyboardPhysicalLayout.ANSI) { }
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
        public AmbilightCaptureToStringVC() : base(AmbilightCaptureType.EntireMonitor) { }
    }

    public class AmbilightFpsToStringVC : EnumToStringVC
    {
        public AmbilightFpsToStringVC() : base(AmbilightFpsChoice.Medium) { }
    }

    public class AmbilightQualityToStringVC : EnumToStringVC
    {
        public AmbilightQualityToStringVC() : base(AmbilightQuality.Medium) { }
    }

    public class EqualizerTypeToStringVC : EnumToStringVC
    {
        public EqualizerTypeToStringVC() : base(EqualizerType.PowerBars) { }
    }

    public class EqualizerPresentationTypeToStringVC : EnumToStringVC
    {
        public EqualizerPresentationTypeToStringVC() : base(EqualizerPresentationType.SolidColor) { }
    }

    public class EqualizerBackgroundModeToStringVC : EnumToStringVC
    {
        public EqualizerBackgroundModeToStringVC() : base(EqualizerBackgroundMode.Disabled) { }
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
