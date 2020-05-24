using Aurora.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Aurora.Profiles
{
    public class Node
    {
        protected JObject _ParsedData;

        // Holds a cache of the child nodes on this node
        private readonly Dictionary<string, object> childNodes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public Node() {
            _ParsedData = new JObject();
        }

        public Node(string json_data) {
            if (string.IsNullOrWhiteSpace(json_data))
                json_data = "{}";

            try {
                _ParsedData = JObject.Parse(json_data);
            } catch (Exception exc) {
                Global.logger.Error($"Exception during Node parsing. Exception: {exc}");
                _ParsedData = JObject.Parse("{}");
            }
        }

        public string GetString(string Name) =>
            _ParsedData.TryGetValue(Name, StringComparison.OrdinalIgnoreCase, out var value) ? value.ToString() : "";

        public int GetInt(string Name) =>
            _ParsedData.TryGetValue(Name, StringComparison.OrdinalIgnoreCase, out var value) ? Convert.ToInt32(value.ToString()) : -1;

        public float GetFloat(string Name) =>
            _ParsedData.TryGetValue(Name, StringComparison.OrdinalIgnoreCase, out var value) ? Convert.ToSingle(value.ToString()) : -1.0f;

        public long GetLong(string Name) =>
            _ParsedData.TryGetValue(Name, StringComparison.OrdinalIgnoreCase, out var value) ? Convert.ToInt64(value.ToString()) : -1l;

        public T GetEnum<T>(string Name) where T : struct
        {
            if (_ParsedData.TryGetValue(Name, StringComparison.OrdinalIgnoreCase, out var value) && !string.IsNullOrWhiteSpace(value.ToString())) {
                var type = typeof(T);
                if (!type.IsEnum) throw new InvalidOperationException();

                // Attempt to parse it by name or number
                if (Enum.TryParse<T>(value.ToString(), true, out var val))
                    return val;

                // If that wasn't successful, try by DescriptionAttribute
                foreach (var field in type.GetFields())
                    if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute
                        && attribute.Description.ToLowerInvariant().Equals(value.ToString().ToLowerInvariant()))
                        return (T)field.GetValue(null);
            }

            // If there is an "undefined" enum value, return that else just do the default(T).
            return Enum.TryParse<T>("Undefined", true, out var u) ? u : default(T);
        }

        public bool GetBool(string Name) =>
            _ParsedData.TryGetValue(Name, StringComparison.OrdinalIgnoreCase, out var value) && value.ToObject<bool>()
                ? value.ToObject<bool>()
                : false;

        public T[] GetArray<T>(string Name) =>
            _ParsedData.TryGetValue(Name, StringComparison.OrdinalIgnoreCase, out var value) ? value.ToObject<T[]>() : (new T[] { });

        /// <summary>
        /// Method for accessing and caching a child node.
        /// </summary>
        /// <typeparam name="TNode">The type of node that will be returned by this method.</typeparam>
        /// <param name="name">The JSON path of the child node.</param>
        public TNode NodeFor<TNode>(string name) where TNode : Node
            => (TNode)(childNodes.TryGetValue(name, out var n) ? n : (childNodes[name] = Instantiator<TNode, string>.Create( _ParsedData[name]?.ToString() ?? "")));
    }
}
