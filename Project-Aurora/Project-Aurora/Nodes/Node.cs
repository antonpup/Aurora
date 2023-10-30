using System;
using System.Collections.Generic;
using System.ComponentModel;
using Aurora.Profiles;
using Aurora.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Aurora.Nodes;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.Members)]
public class Node
{
    [AutoJsonIgnore]
    protected JObject _ParsedData;

    // Holds a cache of the child nodes on this node
    private readonly Dictionary<string, object> _childNodes = new(StringComparer.OrdinalIgnoreCase);

    public Node() {
        _ParsedData = new JObject();
    }

    public Node(string jsonData) {
        if (string.IsNullOrWhiteSpace(jsonData))
            jsonData = "{}";

        try {
            _ParsedData = JObject.Parse(jsonData);
        } catch (Exception exc) {
            Global.logger.Error(exc, $"Exception during Node parsing");
            _ParsedData = JObject.Parse("{}");
        }
    }

    public string GetString(string name) =>
        _ParsedData.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var value) ? value.ToString() : "";

    public int GetInt(string name) =>
        _ParsedData.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var value) ? Convert.ToInt32(value.ToString()) : -1;

    public float GetFloat(string name) =>
        _ParsedData.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var value) ? Convert.ToSingle(value.ToString()) : -1.0f;

    public long GetLong(string name) =>
        _ParsedData.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var value) ? Convert.ToInt64(value.ToString()) : -1L;

    public T GetEnum<T>(string name) where T : struct
    {
        if (_ParsedData.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var value) && !string.IsNullOrWhiteSpace(value.ToString())) {
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
        return Enum.TryParse<T>("Undefined", true, out var u) ? u : default;
    }

    public bool GetBool(string name) =>
        _ParsedData.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var value) && value.ToObject<bool>() && value.ToObject<bool>();

    public T[] GetArray<T>(string name) =>
        _ParsedData.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var value) ? value.ToObject<T[]>() : new T[] { };

    /// <summary>
    /// Method for accessing and caching a child node.
    /// </summary>
    /// <typeparam name="TNode">The type of node that will be returned by this method.</typeparam>
    /// <param name="name">The JSON path of the child node.</param>
    public TNode NodeFor<TNode>(string name) where TNode : Node
        => (TNode)(_childNodes.TryGetValue(name, out var n) ? n : _childNodes[name] = Instantiator<TNode, string>.Create( _ParsedData[name]?.ToString() ?? ""));
}