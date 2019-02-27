using System;

namespace Aurora.Settings.Overrides {

    /// <summary>
    /// Specifies that the marked property on a layer handler can be used with the overrides system.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class LogicOverridableAttribute : Attribute {

        /// <summary>
        /// The user-friendly name given to this property.
        /// </summary>
        public string Name { get; }

        public LogicOverridableAttribute(string name) {
            Name = name;
        }
    }
}
