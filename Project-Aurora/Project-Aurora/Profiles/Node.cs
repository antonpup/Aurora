using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Aurora.Profiles {

    /// <summary>
    /// Represents a node in a GameState tree.
    /// </summary>
    public class Node<TClass> : StringProperty<TClass> where TClass : Node<TClass> {
        protected JObject _ParsedData;

        public Node() : base() {
            _ParsedData = new JObject();
        }

        public Node(string json_data) : this() {
            if (string.IsNullOrWhiteSpace(json_data))
                json_data = "{}";

            try {
                _ParsedData = JObject.Parse(json_data);
            } catch (Exception exc) {
                Global.logger.Error($"Exception during Node parsing. Exception: {exc}");

                _ParsedData = JObject.Parse("{}");
            }
        }
        
        internal string GetString(string name) => _ParsedData.TryGetValue(name, out JToken value) ? value.ToString() : "";
        internal int GetInt(string Name) => _ParsedData.TryGetValue(Name, out JToken value) ? Convert.ToInt32(value.ToString()) : -1;
        internal float GetFloat(string Name) => _ParsedData.TryGetValue(Name, out JToken value) ? Convert.ToSingle(value.ToString()) : -1.0f;
        internal double GetDouble(string Name) => _ParsedData.TryGetValue(Name, out JToken value) ? Convert.ToDouble(value.ToString()) : -1.0d;
        internal long GetLong(string Name) => _ParsedData.TryGetValue(Name, out JToken value) ? Convert.ToInt64(value.ToString()) : -1;
        internal bool GetBool(string Name) => _ParsedData.TryGetValue(Name, out JToken value) && value.ToObject<bool>() ? value.ToObject<bool>() : false;
        internal T[] GetArray<T>(string Name) => _ParsedData.TryGetValue(Name, out JToken value) ? value.ToObject<T[]>() : new T[] { };
        internal T GetEnum<T>(string Name) => (T)GetEnum(Name, typeof(T));

        internal object GetEnum(string Name, Type enumType) {
            if (_ParsedData.TryGetValue(Name, out JToken value) && !string.IsNullOrWhiteSpace(value.ToString())) {
                if (!enumType.IsEnum) throw new InvalidOperationException();
                foreach (var field in enumType.GetFields()) {
                    var attribute = Attribute.GetCustomAttribute(field,
                        typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attribute != null) {
                        if (attribute.Description.ToLowerInvariant().Equals(value.ToString().ToLowerInvariant()))
                            return field.GetValue(null);
                    }

                    if (field.Name.ToLowerInvariant().Equals(value.ToString().ToLowerInvariant()))
                        return field.GetValue(null);
                }

                return Enum.Parse(enumType, "Undefined", true);
            } else
                return Enum.Parse(enumType, "Undefined", true);
        }

        internal object GetAsType(string Name, Type type) {
            if (type == typeof(string)) return GetString(Name);
            else if (type == typeof(int)) return GetInt(Name);
            else if (type == typeof(float)) return GetFloat(Name);
            else if (type == typeof(double)) return GetDouble(Name);
            else if (type == typeof(long)) return GetLong(Name);
            else if (type == typeof(bool)) return GetBool(Name);
            else if (type.IsEnum) return GetEnum(Name, type);
            else return null;
        }
    }

    /// <summary>
    /// Similar to the regular Node but will attempt to automatically populate fields/properties.
    /// </summary>
    public abstract class AutoNode<T> : Node<T> where T : AutoNode<T> {

        public AutoNode() : base() { }

        public AutoNode(string json) : base(json) {

            // For every member on this class
            foreach (var member in PropertyLookup) {
                var memberName = member.Key.ToLowerInvariant();

                // Search the JSON for a field with the same name (ignoring case).
                if (_ParsedData is JObject obj) {
                    var prop = obj.Properties().Where(p => p.Name.ToLowerInvariant() == memberName).FirstOrDefault();

                    // If a JSON property was found, attempt to set the member to its value
                    if (prop != null)
                        member.Value.Item2.Invoke((T)this, GetAsType(prop.Name, member.Value.Item3));
                }
            }
        }
    }
}
