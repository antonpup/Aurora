using FastMember;
using System;
using System.Collections;

namespace Aurora.Utils {

    public static class FastMemberExtensions {

        /// <summary>
        /// Takes a path to a property (e.g. "Property/NestedProperty") and attempts to resolve it into a value within the context of this object.
        /// </summary>
        public static object ResolvePropertyPath(this object target, string path) {
            var pathParts = path.Split('/');
            var curObj = target;
            try {
                foreach (var part in pathParts) {
                    // If this is an enumerable and the part is a valid number, get the nth item of that enumerable
                    if (curObj is IEnumerable e && int.TryParse(part, out var index))
                        curObj = e.ElementAtIndex(index);

                    // Otherwise if this is any other object, use FastMember to access the relevant property/field.
                    else
                        curObj = ObjectAccessor.Create(curObj)[part];
                }

                return curObj; // If we got here, there is a valid object at this path, return it.

            } catch (ArgumentOutOfRangeException) { // Thrown if ObjectAccessor attepts to get a field/property that doesn't exist
            } catch (IndexOutOfRangeException) { } // Thrown if IEnumerable.ElementAtIndex tries to go out of bounds
            return null;
        }
    }
}
