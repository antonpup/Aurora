using Aurora.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Aurora.Profiles
{

    /// <summary>
    /// A class representing various information retaining to the game.
    /// </summary>
    public interface IGameState {
        JObject _ParsedData { get; }
        string Json { get; }        
        string GetNode(string name);

        /// <summary>Attempts to resolve the given path into a numeric value. Returns 0 on failure.</summary>
        double GetNumber(string path);

        /// <summary>Attempts to resolve the given path into a boolean value. Returns false on failure.</summary>
        bool GetBool(string path);

        /// <summary>Attempts to resolve the given path into a string value. Returns an empty string on failure.</summary>
        string GetString(string path);

        /// <summary>Attempts to resolve the given path into a enum value. Returns null on failure.</summary>
        Enum GetEnum(string path);

        /// <summary>Attempts to resolve the given path into a numeric value. Returns default on failure.</summary>
        TEnum GetEnum<TEnum>(string path) where TEnum : Enum;
    }

    public class GameState : IGameState
    {
        private static LocalPCInformation _localpcinfo;

        // Holds a cache of the child nodes on this gamestate
        private readonly Dictionary<string, object> childNodes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        [GameStateIgnore] public JObject _ParsedData { get; }
        [GameStateIgnore] public string Json { get; }

        public LocalPCInformation LocalPCInfo => _localpcinfo ?? (_localpcinfo = new LocalPCInformation());

        /// <summary>
        /// Creates a default GameState instance.
        /// </summary>
        public GameState() : base() {
            Json = "{}";
            _ParsedData = new JObject();
        }

        /// <summary>
        /// Creates a GameState instance based on the passed json data.
        /// </summary>
        /// <param name="json_data">The passed json data</param>
        public GameState(string json_data) : base() {
            if (string.IsNullOrWhiteSpace(json_data))
                json_data = "{}";

            Json = json_data;
            _ParsedData = JObject.Parse(json_data);
        }

        /// <summary>
        /// Gets the JSON for a child node in this GameState.
        /// </summary>
        public string GetNode(string name) =>
            _ParsedData.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var value) ? value.ToString() : "";

        /// <summary>
        /// Use this method to more-easily lazily return the child node of the given name that exists on this AutoNode.
        /// </summary>
        protected TNode NodeFor<TNode>(string name) where TNode : Node
            => (TNode)(childNodes.TryGetValue(name, out var n) ? n : (childNodes[name] = Instantiator<TNode, string>.Create(_ParsedData[name]?.ToString() ?? "")));

        #region GameState path resolution
        /// <summary>
        /// Attempts to resolve the given GameState path into a value.<para/>
        /// Returns whether or not the path resulted in a field or property (true) or was invalid (false).
        /// </summary>
        /// <param name="type">The <see cref="GSIPropertyType"/> that the property must match for this to be valid.</param>
        /// <param name="value">The current value of the resulting property or field on this instance.</param>
        private bool TryResolveGSPath(string path, GSIPropertyType type, out object value) {
            value = null;
            return !string.IsNullOrEmpty(path)
                && (value = this.ResolvePropertyPath(path)) != null
                && GSIPropertyTypeConverter.IsTypePropertyType(value?.GetType(), type);
        }

        public double GetNumber(string path) {
            if (double.TryParse(path, out var val)) // If the path is a raw number, return that
                return val;
            if (TryResolveGSPath(path, GSIPropertyType.Number, out var pVal)) // Next, try resolve the path as we would other types
                return Convert.ToDouble(pVal);
            return 0;
        }

        public bool GetBool(string path) => TryResolveGSPath(path, GSIPropertyType.Boolean, out var @bool) ? Convert.ToBoolean(@bool) : false;
        public string GetString(string path) => TryResolveGSPath(path, GSIPropertyType.String, out var str) ? str.ToString() : "";
        public Enum GetEnum(string path) => TryResolveGSPath(path, GSIPropertyType.Enum, out var @enum) && @enum is Enum e ? e : null;
        public TEnum GetEnum<TEnum>(string path) where TEnum : Enum => TryResolveGSPath(path, GSIPropertyType.Enum, out var @enum) && @enum is TEnum e ? e : default;
        #endregion

        /// <summary>
        /// Displays the JSON, representative of the GameState data
        /// </summary>
        /// <returns>JSON String</returns>
        public override string ToString() => Json;
    }


    /// <summary>The valid types of GSI property.</summary>
    public enum GSIPropertyType { None, Number, Boolean, String, Enum }

    internal static class GSIPropertyTypeConverter {
        /// <summary>
        /// A set of predicates that determine if the given <see cref="Type"/> is of the given <see cref="GSIPropertyType"/>
        /// </summary>
        private static Dictionary<GSIPropertyType, Func<Type, bool>> predicates = new Dictionary<GSIPropertyType, Func<Type, bool>> {
            [GSIPropertyType.None] = _ => false,
            [GSIPropertyType.Enum] = type => type.IsEnum, // Needs to take priority over number, since enums are stored as numbers as so IsNumericType would be true
            [GSIPropertyType.Number] = type => TypeUtils.IsNumericType(type),
            [GSIPropertyType.Boolean] = type => Type.GetTypeCode(type) == TypeCode.Boolean,
            [GSIPropertyType.String] = type => Type.GetTypeCode(type) == TypeCode.String
        };

        /// <summary>
        /// Gets the <see cref="GSIPropertyType"/> for the given <see cref="Type"/>.
        /// </summary>
        public static GSIPropertyType TypeToPropertyType(Type type) {
            if (type == null) return GSIPropertyType.None;
            foreach (var (propertyType, predicate) in predicates)
                if (predicate(type))
                    return propertyType;
            return GSIPropertyType.None;
        }

        /// <summary>
        /// Determines if the given <see cref="Type"/> is valid for the given <see cref="GSIPropertyType"/>.
        /// </summary>
        public static bool IsTypePropertyType(Type type, GSIPropertyType propertyType) => type == null ? false : predicates[propertyType](type);
    }


    /// <summary>
    /// An empty gamestate with no child nodes.
    /// </summary>
    public class EmptyGameState : GameState
    {
        public EmptyGameState() : base() { }
        public EmptyGameState(string json) : base(json) { }
    }


    /// <summary>
    /// Attribute that can be applied to properties to indicate they should be excluded from the game state.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class GameStateIgnoreAttribute : Attribute { }

    /// <summary>
    /// Attribute that indicates the range of indicies that are valid for an enumerable game state property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RangeAttribute : Attribute {

        public RangeAttribute(int start, int end) {
            Start = start;
            End = end;
        }

        public int Start { get; set; }
        public int End { get; set; }
    }
}
