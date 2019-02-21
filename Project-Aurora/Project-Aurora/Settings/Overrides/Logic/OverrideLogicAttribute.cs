using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Simple attribute that can be added to conditions to add metadata to them and register them as conditions.
    /// Unregistered conditions will still work, but they will not be shown in the dropdown list when editing layer visibility conditions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class OverrideLogicAttribute : Attribute {

        /// <param name="name">The name of the condition (will appear in the dropdown list).</param>
        public OverrideLogicAttribute(string name, OverrideLogicCategory category = OverrideLogicCategory.Misc) {
            Name = name;
            Category = category;
        }

        /// <summary>The name of the condition (will appear in the dropdown list).</summary>
        public string Name { get; }

        /// <summary>The category this condition belongs to (items will be grouped by this in the dropdown list).</summary>
        public OverrideLogicCategory Category { get; }

        /// <summary>Gets the description of the category as a string.</summary>
        public string CategoryStr => Utils.EnumUtils.GetDescription(Category);
    }

    public enum OverrideLogicCategory {
        [Description("Logic")] Logic,
        [Description("State Variable")] State,
        [Description("Input")] Input,
        [Description("Misc.")] Misc,
        [Description("Maths (Advanced)")] Maths,
        [Description("String (Advanced)")] String
    }
}
