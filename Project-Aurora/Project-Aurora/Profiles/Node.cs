using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Aurora.Profiles
{
    public class Node<TClass> : StringProperty<TClass> where TClass : Node<TClass>
    {
        protected Newtonsoft.Json.Linq.JObject _ParsedData;

        // Holds a cache of the child nodes on this node
        private readonly Dictionary<string, object> childNodes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public Node() : base()
        {
            _ParsedData = new Newtonsoft.Json.Linq.JObject();
        }

        public Node(string json_data) : this()
        {
            if (String.IsNullOrWhiteSpace(json_data))
                json_data = "{}";

            try
            {
                _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json_data);
            }
            catch(Exception exc)
            {
                Global.logger.Error($"Exception during Node parsing. Exception: {exc}");

                _ParsedData = Newtonsoft.Json.Linq.JObject.Parse("{}");
            }
        }

        internal string GetString(string Name)
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(Name, StringComparison.OrdinalIgnoreCase, out value))
                return value.ToString();
            else
                return "";
        }

        internal int GetInt(string Name)
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(Name, StringComparison.OrdinalIgnoreCase, out value))
                return Convert.ToInt32(value.ToString());
            else
                return -1;
        }

        internal float GetFloat(string Name)
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(Name, StringComparison.OrdinalIgnoreCase, out value))
                return Convert.ToSingle(value.ToString());
            else
                return -1.0f;
        }

        internal long GetLong(string Name)
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(Name, StringComparison.OrdinalIgnoreCase, out value))
                return Convert.ToInt64(value.ToString());
            else
                return -1;
        }

        internal T GetEnum<T>(string Name) where T : struct
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(Name, StringComparison.OrdinalIgnoreCase, out value) && !String.IsNullOrWhiteSpace(value.ToString()))
            {
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

        internal bool GetBool(string Name)
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(Name, StringComparison.OrdinalIgnoreCase, out value) && value.ToObject<bool>())
                return value.ToObject<bool>();
            else
                return false;
        }

        internal T[] GetArray<T>(string Name)
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(Name, StringComparison.OrdinalIgnoreCase, out value))
                return value.ToObject<T[]>();
            else
                return new T[] { };
        }

        /// <summary>
        /// Method for accessing and caching a child node.
        /// </summary>
        /// <typeparam name="TNode">The type of node that will be returned by this method.</typeparam>
        /// <param name="name">The JSON path of the child node.</param>
        internal TNode NodeFor<TNode>(string name) where TNode : Node<TNode>
            => (TNode)(childNodes.TryGetValue(name, out var n) ? n : (childNodes[name] = Instantiator<TNode, string>.Create( _ParsedData[name]?.ToString() ?? "")));
    }
}
