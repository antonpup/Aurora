using Aurora.Utils;
using FastMember;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Profiles {

    /// <summary>
    /// The <see cref="GameStateParameterNode"/> is specialized a data structure for storing the possible game state parameter paths.<br/>
    /// These paths are stored as a tree-like structure, with each branch representing a distinct path part.
    /// </summary>
    public class GameStateParameterNode : IEnumerable<GameStateParameterNode> {

        private readonly Dictionary<string, GameStateParameterNode> children;

        /// <summary>
        /// Creates a new leaf node of the given type.
        /// </summary>
        private GameStateParameterNode(string name, Type type) {
            Name = name;
            Type = GSIPropertyTypeConverter.TypeToPropertyType(type);
            ClrType = type;
        }

        /// <summary>
        /// Creates a new branch node with the given children.
        /// </summary>
        private GameStateParameterNode(string name, IEnumerable<GameStateParameterNode> children) {
            Name = name;
            Type = GSIPropertyType.None;
            this.children = children.ToDictionary(c => c.Name, c => c);
        }

        /// <summary>The path name for this node.</summary>
        public string Name { get; }

        /// <summary>The type of leaf this parameter represents. Will be <see cref="GSIPropertyType.None"/> for branch nodes (i.e. nodes with child parameters).</summary>
        public GSIPropertyType Type { get; }

        /// <summary>The <see cref="Type"/> this leaf node represents. Null if not a leaf node.</summary>
        public Type ClrType { get; }

        /// <summary>Whether this Node is a 'leaf' node, representing a value rather than a 'folder' or collection of values.</summary>
        public bool IsLeaf => Type != GSIPropertyType.None;

        /// <summary>The number of child nodes underneath this node.</summary>
        public int ChildCount => children?.Count ?? 0;

        /// <summary>Attempts to get the child of the given name. Returns null if a child of this name does not exist.</summary>
        public GameStateParameterNode this[string child] => children.TryGetValue(child, out var c) ? c : null;

        /// <summary>Returns a filtered <see cref="GameStateParameterNode"/> that only contains paths that are of the given property type.</summary>
        public GameStateParameterNode OfType(GSIPropertyType type) => _OfType(type) ?? new GameStateParameterNode(Name, ClrType);
        private GameStateParameterNode _OfType(GSIPropertyType type) {
            if (Type == type) return this; // If this is a match, return it.
            else if (!IsLeaf) { // If this is a branch, recurse
                var filteredChildren = new List<GameStateParameterNode>();
                foreach (var child in children.Values) {
                    var filteredChild = child._OfType(type);
                    if (filteredChild != null) filteredChildren.Add(filteredChild);
                }
                return filteredChildren.Count > 0 ? new GameStateParameterNode(Name, filteredChildren) : null;
            }
            return null; // Otherwise return nothing.
        }

        /// <summary>
        /// Determines if the given fullpath points to a valid property.
        /// </summary>
        public bool IsValidPath(string path) => ResolvePathType(path.Split('/')) != GSIPropertyType.None;

        /// <summary>
        /// Determines if the given fullpath points to a valid property of the given property type.
        /// </summary>
        public bool IsValidPath(string path, GSIPropertyType type) => ResolvePathType(path.Split('/')) == type;

        /// <summary>
        /// Gets the <see cref="GSIPropertyType"/> of the property with the given path.
        /// </summary>
        private GSIPropertyType ResolvePathType(string[] pathParts, int cursor = 0) {
            if (cursor >= pathParts.Length) // If we have reached the end of the path, check if this is a valid leaf node.
                return Type;
            if (children?.ContainsKey(pathParts[cursor]) != true) // If this node doesn't have a child node of this name, path must be invalid, return false
                return GSIPropertyType.None;
            return children[pathParts[cursor]].ResolvePathType(pathParts, cursor + 1);
        }

        /// <summary>
        /// Flattens the paths of this node, returning the full paths of all child nodes (including nested child nodes etc).
        /// </summary>
        public IEnumerable<string> Flatten() => Flatten(true);
        private IEnumerable<string> Flatten(bool asRoot) => IsLeaf
            ? Name.ToEnumerable()
            : children?.Values.SelectMany(child => child.Flatten(false).Select(path => (asRoot ? "" : Name + "/") + path)) ?? Enumerable.Empty<string>();

        /// <summary>
        /// Creates a <see cref="GameStateParameterNode"/> for the given type.
        /// </summary>
        public static GameStateParameterNode GenerateTreeFor(Type type) {
            static GameStateParameterNode getNode(string name, Type t) {
                // If this node is a valid GSIPropertyType, it must be a leaf element
                var gsiType = GSIPropertyTypeConverter.TypeToPropertyType(t);
                if (gsiType != GSIPropertyType.None)
                    return new GameStateParameterNode(name, t);

                // Otherwise, check all the members for this type and recursively get the nodes.
                var accessor = TypeAccessor.Create(t);
                var children = new List<GameStateParameterNode>();
                if (accessor.GetMembersSupported) {
                    foreach (var member in accessor.GetMembers()) {
                        if (member.Type == t) continue; // For now, ignore any recursive types. May need a better solution in future.
                        if (member.GetAttribute(typeof(GameStateIgnoreAttribute), true) != null) continue; // Ignore anything with the GSIIgnore attribute on it

                        if (member.Type.ImplementsGenericInterface(typeof(IEnumerable<>), out var ienumTypes) && member.GetAttribute(typeof(RangeAttribute), true) is RangeAttribute range) {
                            // If the type is an IEnumerable with a RangeAttribute, create a path for each int in that range.
                            var enumeratedChildren = new List<GameStateParameterNode>();
                            for (var i = range.Start; i <= range.End; i++)
                                enumeratedChildren.Add(getNode(i.ToString(), ienumTypes[0]));
                            children.Add(new GameStateParameterNode(member.Name, enumeratedChildren));

                        } else {
                            // Else if it's a normal member, just recursively populate the tree
                            children.Add(getNode(member.Name, member.Type));
                        }
                    }
                }
                return new GameStateParameterNode(name, children);
            }
            return getNode("", type);
        }

        #region IEnumerable
        public IEnumerator<GameStateParameterNode> GetEnumerator() => children.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => children.Values.GetEnumerator();
        #endregion
    }
}
