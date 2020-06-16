using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Markup;

namespace Aurora.Utils {

    public static class EnumUtils {

        // Collection of custom description resolvers, stored by the EnumType as a list.
        private static readonly Dictionary<Type, List<Func<Enum, string>>> customDescriptionResolvers = new Dictionary<Type, List<Func<Enum, string>>>();

        static EnumUtils() {
            // Register a global enum description handler for more user-friendly key names
            RegisterCustomDescriptionResolver<Keys>(e => e switch {
                Keys.LControlKey => "Control",
                Keys.LMenu => "Alt",
                Keys.LWin => "Win",
                _ => null
            });
        }


        /// <summary>Attempts to match the enum to a custom description provider. If unsuccessful or the resolver returns null, gets the value of
        /// the <see cref="DescriptionAttribute"/> applied to this enum value or simply the name of the enum if this attribute is not applied.</summary>
        public static string GetDescription(this Enum enumObj) =>
            RunCustomDescriptionResolvers(enumObj) ?? enumObj.GetCustomAttribute<DescriptionAttribute>()?.Description ?? enumObj.ToString();

        /// <summary>Gets the value of the <see cref="CategoryAttribute"/> assigned to this enum value.
        /// Returns an empty string if this attribute is not applied.</summary>
        public static string GetCategory(this Enum enumObj) =>
            enumObj.GetCustomAttribute<CategoryAttribute>()?.Category ?? "";

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
        public static T TryParseOr<T>(string value, bool ignoreCase = true, T defaultValue = default) where T : struct, IConvertible {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("`T` must be an enum", nameof(T));
            return Enum.TryParse<T>(value, ignoreCase, out var res) ? res : defaultValue;
        }

        /// <summary>
        /// Registers a new custom enum description resolver. If <see cref="GetDescription(Enum)"/> is called on a enum of the given <paramref name="enumType"/>, the
        /// <paramref name="resolver"/> function will be invoked (passing the value of the enum) and the value returned from that will be used as the description for
        /// this enum instead. If the resolver returns null, the default handling will be used instead.<para/>
        /// Using <see cref="DescriptionAttribute"/> should be preferred when possible, but if the enum is in another project or needs some custom logic applied, resolvers
        /// can be used for that instead.
        /// </summary>
        public static void RegisterCustomDescriptionResolver<TEnum>(Func<Enum, string> resolver) where TEnum : Enum {
            if (resolver is null) throw new ArgumentNullException(nameof(resolver));
            var enumType = typeof(TEnum);
            if (!customDescriptionResolvers.ContainsKey(enumType))
                customDescriptionResolvers.Add(enumType, new List<Func<Enum, string>>());
            customDescriptionResolvers[enumType].Add(resolver);
        }

        /// <summary>Resolves the custom description resolvers on the given enum value, returning the value of the first resolver with a non-null value or null if no resolvers returned a value.</summary>
        private static string RunCustomDescriptionResolvers(Enum enumValue) {
            if (customDescriptionResolvers.TryGetValue(enumValue?.GetType(), out var resolverList)) {
                foreach (var resolver in resolverList) {
                    var value = resolver(enumValue);
                    if (value != null)
                        return value;
                }
            }
            return null;
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

        public override object ProvideValue(IServiceProvider serviceProvider) => enumType == null ? new object() : GetListFor(enumType, DoGroup);

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


    /// <summary>
    /// Converter that takes an enum value and outputs it's description.
    /// </summary>
    public class EnumToDescriptionConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is Enum e ? e.GetDescription() : value.ToString();
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
