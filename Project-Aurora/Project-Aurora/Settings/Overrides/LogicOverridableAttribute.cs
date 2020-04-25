using Aurora.Settings.Layers;
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

        public LogicOverridableAttribute() { }

        public LogicOverridableAttribute(string name) {
            Name = name;
        }
    }



    /// <summary>
    /// Marks that a property in this <see cref="LayerHandler"/>'s properties' inheritance tree should be
    /// ignored and not shown in the overridable list. This is useful for when properties are incorrectly
    /// inherited from other <see cref="LayerHandlerProperties"/> classes. This attribute should be applied
    /// TO THE LAYER HANDLER, NOT THE PROPERTIES. This is because some layer handlers share the same properties
    /// class, but only on some of the handlers should the properties be hidden. Yes it's a mess, I know.
    /// <para>This wants to be removed and the inheritance of the properties should be fixed ASAP.</para>
    /// </summary>
    #warning This should be removed as soon as possible when the inheritance tree for layer properties is fixed.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class LogicOverrideIgnorePropertyAttribute : Attribute {

        /// <summary>
        /// A name of a property (by member name, including the underscore) that should not
        /// be shown in the overridable list for this LayerHandlerProperties.
        /// </summary>
        public string PropertyName { get; }

        public LogicOverrideIgnorePropertyAttribute(string propertyName) {
            PropertyName = propertyName;
        }
    }
}
