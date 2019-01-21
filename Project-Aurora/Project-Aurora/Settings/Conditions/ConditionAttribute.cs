using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings.Conditions {
    /// <summary>
    /// Simple attribute that can be added to conditions to add metadata to them and register them as conditions.
    /// Unregistered conditions will still work, but they will not be shown in the dropdown list when editing layer visibility conditions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ConditionAttribute : Attribute {

        /// <param name="name">The name of the condition (will appear in the dropdown list).</param>
        public ConditionAttribute(string name) {
            Name = name;
        }

        /// <summary>The name of the condition (will appear in the dropdown list).</summary>
        public string Name { get; }
    }
}
