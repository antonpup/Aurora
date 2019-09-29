using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Aurora.Core.Utils
{
    public static class JsonUtils
    {
        public static bool IsFalse(this JsonElement element)
        {
            return element.ValueKind == JsonValueKind.False;
        }

        public static string TryGetString(this JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.String)
                return null;
            else
                return element.GetString();
        }

        public static string TryGetStringProperty(this JsonElement element, string property)
        {
            if (element.ValueKind != JsonValueKind.Object)
                return null;

            if (element.TryGetProperty(property, out JsonElement elem))
            {
                if (elem.ValueKind == JsonValueKind.String)
                    return elem.GetString();
            }

            return null;
        }
    }
}
