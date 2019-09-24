using System;

namespace AuroraUI.Controls.InputField {

    /// <summary>
    /// This attribute can be applied to Blazor controls to make them accessible by the 'Field' control.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class InputFieldControlAttribute : Attribute {

        /// <summary>
        /// The type of data that this field can edit. The Blazor control must have a `Value` property of this type.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// If a special name is provided, it will mark that this control is a specialized control for a particular purpose.
        /// An example of this could be a password input, which takes a string (like a normal text input does) but should not be used
        /// everywhere where a string is required. Metadata attributes on class properties will state whether that property should
        /// use field input with a special name.
        /// </summary>
        public string SpecialName { get; set; }


        public InputFieldControlAttribute(Type type) {
            Type = type;
        }
    }
}
