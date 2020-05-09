using Aurora.Utils;
using FastMember;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Aurora.Profiles {

    /// <summary>
    /// A class representing various information retaining to the game.
    /// </summary>
    public interface IGameState {

        /// <summary>Gets the raw unparsed JSON that this GameState is based on.</summary>
        string Json { get; }

        /// <summary>Gets the raw parsed JObject for this GameState's data.</summary>
        JObject _ParsedData { get; }

        /// <summary>Gets the JSON for the sub-node with the given name.</summary>
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

    public abstract class GameState<TSelf> : IGameState where TSelf : GameState<TSelf> {

        // Holds a cache of the child nodes on this gamestate
        private readonly Dictionary<string, object> childNodes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        [GameStateIgnore] public JObject _ParsedData { get; }
        [GameStateIgnore] public string Json { get; }

        /// <summary>
        /// Creates a default GameState instance.
        /// </summary>
        public GameState() {
            Json = "{}";
            _ParsedData = new JObject();
        }

        /// <summary>
        /// Creates a GameState instance based on the passed json data.
        /// </summary>
        /// <param name="json_data">The passed json data</param>
        public GameState(string json_data) {
            Json = string.IsNullOrWhiteSpace(json_data) ? "{}" : json_data;
            _ParsedData = JObject.Parse(Json);
        }

        /// <summary>
        /// Information about the local system
        /// </summary>
        private static LocalPCInformation _localpcinfo;
        public LocalPCInformation LocalPCInfo => _localpcinfo ?? (_localpcinfo = new LocalPCInformation());

        /// <summary>
        /// Gets the JSON for the sub-node with the given name.
        /// </summary>
        [GameStateIgnore]
        public string GetNode(string name) => _ParsedData.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var value) ? value.ToString() : "{}";

        /// <summary>
        /// Fetches a child node from this game state.<para/>
        /// Use this method to more-easily lazily return the child node of the given name that exists on this AutoNode.
        /// </summary>
        [GameStateIgnore] protected TNode NodeFor<TNode>(string name) where TNode : Node<TNode>
            => (TNode)(childNodes.TryGetValue(name, out var n) ? n : (childNodes[name] = Instantiator<TNode, string>.Create(_ParsedData[name]?.ToString() ?? "")));

        /// <summary>
        /// Displays the JSON, representative of the GameState data
        /// </summary>
        [GameStateIgnore] public override string ToString() => Json;

        #region GameState path resolution
        /// <summary>
        /// Attempts to resolve the given GameState path into a value.<para/>
        /// Returns whether or not the path resulted in a field or property (true) or was invalid (false).
        /// </summary>
        /// <param name="type">The <see cref="GSIPropertyType"/> that the property must match for this to be valid.</param>
        /// <param name="value">The current value of the resulting property or field on this instance.</param>
        private bool TryResolveGSPath(string path, GSIPropertyType type, out object value) {
            object curObj = this;
            try {
                foreach (var part in path.Split('/')) {
                    // If the current object is an IEnumerable and the part is an integer, we can get the nth item of this array.
                    if (curObj is IEnumerable enumerable && int.TryParse(part, out var targetIdx))
                        curObj = enumerable.ElementAtIndex(targetIdx);

                    // Else if the object is something else (but not null), try access the requested field
                    else if (curObj != null) {
                        var accessor = ObjectAccessor.Create(curObj);
                        curObj = accessor[part];
                    }
                }

            // IndexOutOfRangeException thrown if enumerable.ElementAtIndex fails, ArgumentOutOfRangeException if ObjectAccessor fails
            } catch (Exception e) when (e is IndexOutOfRangeException || e is ArgumentOutOfRangeException) {
                value = null;
                return false;
            }

            // Once we have resolved the path into an object, check it's the type requested
            var valid = GSIPropertyTypeConverter.IsTypePropertyType(curObj?.GetType(), type);
            value = valid ? curObj : null;
            return valid;
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
    }

    /// <summary>The valid types of GSI property.</summary>
    public enum GSIPropertyType { None, Number, Boolean, String, Enum }

    internal static class GSIPropertyTypeConverter {
        /// <summary>
        /// A set of predicates that determine if the given <see cref="Type"/> is of the given <see cref="GSIPropertyType"/>
        /// </summary>
        private static Dictionary<GSIPropertyType, Func<Type, bool>> predicates = new Dictionary<GSIPropertyType, Func<Type, bool>> {
            [GSIPropertyType.None] = _ => false,
            [GSIPropertyType.Number] = type => TypeUtils.IsNumericType(type),
            [GSIPropertyType.Boolean] = type => Type.GetTypeCode(type) == TypeCode.Boolean,
            [GSIPropertyType.String] = type => Type.GetTypeCode(type) == TypeCode.String,
            [GSIPropertyType.Enum] = type => type.IsEnum
        };

        /// <summary>
        /// Gets the <see cref="GSIPropertyType"/> for the given <see cref="Type"/>.
        /// </summary>
        public static GSIPropertyType TypeToPropertyType(Type type) {
            foreach (var (propertyType, predicate) in predicates)
                if (predicate(type))
                    return propertyType;
            return GSIPropertyType.None;
        }

        /// <summary>
        /// Determines if the given <see cref="Type"/> is valid for the given <see cref="GSIPropertyType"/>.
        /// </summary>
        public static bool IsTypePropertyType(Type type, GSIPropertyType propertyType) => predicates[propertyType](type);
    }


    /// <summary>
    /// An empty GameState that has no child properties or nodes.
    /// </summary>
    public class EmptyGameState : GameState<EmptyGameState> {
        public EmptyGameState() : base() { }
        public EmptyGameState(string json) : base(json) { }
    }


    /// <summary>
    /// Metadata attribute that can be applied to properties or fields of a GameState to indicate they should not be included in the list.
    /// </summary>
    public class GameStateIgnoreAttribute : Attribute { }

    /// <summary>
    /// Metadata attribute that can be applied to certain types (any array type or anything that extends <see cref="IEnumerable{T}"/>) of properties
    /// or fields of a GameState to indicate the valid range of indicies for that list.
    /// </summary>
    public class RangeAttribute : Attribute {

        /// <summary>Specifies that a <see cref="IEnumerable{T}"/> property has a given number of valid values.</summary>
        /// <param name="start">The first valid index for this enumerable (inclusive).</param>
        /// <param name="end">The last valid index for this enumerable (inclusive).</param>
        public RangeAttribute(int start, int end) {
            Start = start;
            End = end;
        }

        public int Start { get; }
        public int End { get; }
    }
}
