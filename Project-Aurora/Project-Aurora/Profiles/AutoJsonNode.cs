using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;

namespace Aurora.Profiles {

    /// <summary>
    /// A version of <see cref="Node"/> which automatically populates the fields defined on it from the parsed JSON data.
    /// </summary>
    public class AutoJsonNode<TSelf> : Node where TSelf : AutoJsonNode<TSelf> {
        // Did consider implementing this auto feature as a Fody weaver however, should profiles become plugin-based, each plugin would need to use Fody if they
        // wished to have the automatic capability. Doing it as a class that can be extended means that no additional setup is required for plugin authors.

        public AutoJsonNode() : base() { }
        public AutoJsonNode(string json) : base(json) {
            ctorAction.Value((TSelf)this);
        }

        #region Constructor builder
        // Compiled action to be run during the contructor that will populate relevant fields
        private static readonly Lazy<Action<TSelf>> ctorAction = new Lazy<Action<TSelf>>(() => {
            var fields = typeof(TSelf).GetFields(bf | BindingFlags.FlattenHierarchy);
            var body = new List<Expression>(fields.Length);
            var selfParam = Parameter(typeof(TSelf));

            // Find all the fields
            foreach (var field in fields.Where(f => f.GetCustomAttribute<AutoJsonIgnoreAttribute>() == null)) {
                if (TryGetMethodForType(field.FieldType, out var getter))
                    // If a relevant Getter method exists for this field, add an assignment to the ctor body for this (e.g. adding `this.SomeField = GetString("SomeField");` )
                    body.Add(
                        Assign(
                            Field(selfParam, field),
                            Call(selfParam, getter, Constant(field.GetCustomAttribute<AutoJsonPropertyNameAttribute>()?.Path ?? field.Name))
                        )
                    );
                else
                    Global.logger.Warn($"Could not find an AutoNode getter method for field '{field.Name}' of type '{field.FieldType.Name}'. It will not be automatically populated.");
            }

            // Compile and return the action
            return Lambda<Action<TSelf>>(body.Count == 0 ? (Expression)Empty() : Block(body), selfParam).Compile();
        });
        #endregion

        #region Getter methods
        private static BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        // An auto-generated list of get methods on Node<TSelf>
        private static readonly Dictionary<Type, MethodInfo> methods = typeof(TSelf).GetMethods(bf)
            // Only count methods that return something, take a single string parameter and whose names start with Get. E.G. bool GetBool(string name) would be a method that is returned
            .Where(m => !m.IsSpecialName && m.Name.StartsWith("Get") && m.ReturnType != typeof(void) && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string))
            .ToDictionary(m => m.ReturnType, m => m);

        // Special methods (with signatures that don't match the others)
        private static readonly MethodInfo arrayMethod = typeof(TSelf).GetMethod("GetArray", bf);
        private static readonly MethodInfo enumMethod = typeof(TSelf).GetMethod("GetEnum", bf);

        /// <summary>
        /// Tries to get the relevant get method on <see cref="Node{TClass}"/> for the given data type. Returns false if no method found.<para/>
        /// Examples:<code>
        /// GetMethodForType(typeof(string)); // returns 'GetString'<br/>
        /// GetMethodForType(typeof(SomeEnum)); // returns the closed generic variant of 'GetEnum&lt;T&gt;' bound to the given enum.
        /// </code></summary>
        private static bool TryGetMethodForType(Type type, out MethodInfo method) {
            if (type.IsEnum)
                method = enumMethod.MakeGenericMethod(type);
            else if (type.IsArray)
                method = arrayMethod.MakeGenericMethod(type.GetElementType());
            else if (methods.TryGetValue(type, out var mi))
                method = mi;
            else
                method = null;
            return method != null;
        }
        #endregion
    }


    #region Attributes
    /// <summary>
    /// Attribute to mark a field to indicate that the <see cref="AutoJsonNode{TSelf}"/> should use a different path when accessing the JSON.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class AutoJsonPropertyNameAttribute : Attribute {
        public string Path { get; set; }
        public AutoJsonPropertyNameAttribute(string path) {
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }
    }

    /// <summary>
    /// Attribute to mark a field to indicate that the <see cref="AutoJsonNode{TSelf}"/> should ignore this field when populating the class members.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class AutoJsonIgnoreAttribute : Attribute { }
    #endregion
}
