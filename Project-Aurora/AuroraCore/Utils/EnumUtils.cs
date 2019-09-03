using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Aurora.Utils
{
    public static class EnumUtils
    {
        public static string GetDescription(this Enum enumObj)
        {
            return enumObj.GetType().GetField(enumObj.ToString()).GetCustomAttribute(typeof(DescriptionAttribute), false) is DescriptionAttribute attr ? attr.Description : enumObj.ToString();
        }

        public static string GetCategory(this Enum enumObj)
        {
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
    }
}
