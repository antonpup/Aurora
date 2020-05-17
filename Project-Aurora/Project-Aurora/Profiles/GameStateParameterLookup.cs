using Aurora.Utils;
using FastMember;
using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Profiles {

    /// <summary>
    /// Data structure that holds a record of all game state parameters for a particular type of GameState.
    /// </summary>
    public sealed class GameStateParameterLookup {

        // List of types that are permitted to be recursively searched
        private static readonly Type[] recursiveWhiteList = new[] { typeof(Node), typeof(GameState), typeof(IEnumerable<Node>) };

        // Internal parameter store. Key = full path, Value = meta
        private readonly Dictionary<string, GameStateParameterLookupEntry> lookup = new Dictionary<string, GameStateParameterLookupEntry>();

        /// <summary>
        /// Creates a new <see cref="GameStateParameterLookup"/> by inspecting all properties on the given type.
        /// </summary>
        public GameStateParameterLookup(Type type) {
            // Recursive function for visiting all types on the given type
            // Will add relevant entries to lookup
            void Visit(string path, string name, Type type) {
                // If this is a variable that can be handled (such as a number or bool), add it to the lookup
                if (GSIPropertyTypeConverter.TypeToPropertyType(type) != GSIPropertyType.None)
                    lookup.Add(path, GameStateParameterLookupEntry.Property(name, path, type));

                else if (recursiveWhiteList.Any(t => t.IsAssignableFrom(type))) {
                    // Else if this not a handlable property, check if it's a node or list of nodes and if so make a folder and visit it's children
                    if (path != "") // If it's the root folder, don't add it
                        lookup.Add(path, GameStateParameterLookupEntry.Folder(name, path));

                    var accessor = TypeAccessor.Create(type);
                    if (!accessor.GetMembersSupported) return;
                    foreach (var member in accessor.GetMembers()) {
                        if (member.Type == type) continue; // Ignore recursive types
                        if (member.GetAttribute(typeof(GameStateIgnoreAttribute), true) != null) continue; // Ignore properties with [GameStateIgnore]

                        var nextPath = (path + "/" + member.Name).TrimStart('/');

                        // If the type is an Enumerable with a range attribute, visit for each item in that range
                        if (member.Type.GetGenericInterfaceTypes(typeof(IEnumerable<>)) is { } ienumTypes && typeof(Node).IsAssignableFrom(ienumTypes[0]) && member.GetAttribute(typeof(RangeAttribute), true) is RangeAttribute range)
                            for (var i = range.Start; i <= range.End; i++)
                                Visit(nextPath + "/" + i, i.ToString(), ienumTypes[0]);

                        // Recursively visit the next type (do this even if it is IEnumerable, as it might be a custom class that implements IEnumerable with extra properties)
                        Visit(nextPath, member.Name, member.Type);
                    }
                }
            }

            // Start the recursive function at the root.
            Visit("", "", type);
        }

        /// <summary>
        /// Attempts to get the definition for the folder or property at the given path.
        /// </summary>
        public GameStateParameterLookupEntry this[string path] =>
            lookup.TryGetValue(path, out var entry) ? entry : null;

        /// <summary>
        /// Gets all the direct/first-level children of the folder at the given path.<br/>
        /// Optionally, only children ones that either are of the given type (in the case of properties) or that contain atleast one property of the given type (in the case of folders).
        /// </summary>
        /// <param name="path">Only children that are within the folder at the given path will be returned.</param>
        /// <param name="type">If not <see cref="GSIPropertyType.None"/>, only children of this type and only folders that contain atleast one property of this type will be returned.</param>
        public IEnumerable<GameStateParameterLookupEntry> Children(string path = "", GSIPropertyType type = GSIPropertyType.None) =>
            from kvp in lookup
            where GetFolderOf(kvp.Key) == path // only include anything in this folder
            where type == GSIPropertyType.None // if type is none, don't worry about type filtering
               || (kvp.Value.IsFolder && AllChildren(kvp.Key).Any(c => c.Type == type)) // return a folder if it contains atleast one child of type
               || (!kvp.Value.IsFolder && kvp.Value.Type == type) // return a property if it is of type
            select kvp.Value;

        /// <summary>Returns a list of all children of the given path, REGARDLESS of depth.</summary>
        private IEnumerable<GameStateParameterLookupEntry> AllChildren(string path) {
            if (!path.EndsWith("/")) path += '/';
            return from kvp in lookup where kvp.Key.StartsWith(path) select kvp.Value;
        }

        /// <summary>
        /// Determines if the given path results in a property of the given type.
        /// </summary>
        /// <param name="type">The result will only be true if the parameter type is of this type. If None is passed, any parameter type is allowed.</param>
        public bool IsValidParameter(string path, GSIPropertyType type = GSIPropertyType.None) =>
            lookup.TryGetValue(path, out var entry) && !entry.IsFolder && (type == GSIPropertyType.None || entry.Type == type);

        /// <summary>
        /// Returns the folder that the given path is in.
        /// </summary>
        private string GetFolderOf(string path) => path.Contains('/') ? path.Substring(0, path.LastIndexOf('/')) : "";
    }


    /// <summary>
    /// Plain object that holds metadata about a single parameter or folder in the <see cref="GameStateParameterLookup"/> collection.
    /// </summary>
    public sealed class GameStateParameterLookupEntry {
        public string Name { get; private set;  }
        public string Path { get; private set; }
        public bool IsFolder { get; private set; }
        public Type ClrType { get; private set; }
        public GSIPropertyType Type { get; private set; }

        public string DisplayName => Name.CamelCaseToSpaceCase();

        private GameStateParameterLookupEntry() { }

        internal static GameStateParameterLookupEntry Folder(string name, string path) => new GameStateParameterLookupEntry {
            Name = name,
            Path = path,
            IsFolder = true,
            ClrType = null,
            Type = GSIPropertyType.None
        };

        internal static GameStateParameterLookupEntry Property(string name, string path, Type type) => new GameStateParameterLookupEntry {
            Name = name,
            Path = path,
            IsFolder = false,
            ClrType = type,
            Type = GSIPropertyTypeConverter.TypeToPropertyType(type)
        };
    }
}
