using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Aurora.Utils {
    public static class FrameworkElementExtensions {

        /// <summary>
        /// Performs a breadth-first search from the given element and returns the first found visual child of the target type.
        /// Returns null if an element of the target type was not found.
        /// </summary>
        public static T FindChildOfType<T>(this DependencyObject self) where T : DependencyObject {
            var toSearch = new Queue<DependencyObject>();
            toSearch.Enqueue(self);
            while (toSearch.Count > 0) {
                var cur = toSearch.Dequeue();
                for (int i = 0, count = VisualTreeHelper.GetChildrenCount(cur); i < count; i++) {
                    var child = VisualTreeHelper.GetChild(cur, i);
                    if (child is T tChild) return tChild;
                    toSearch.Enqueue(child);
                }
            }
            return null;
        }

        /// <summary>Tests to see if the given 'parent' object is a parent of the given 'child' object (as according to
        /// <see cref="VisualTreeHelper.GetParent(DependencyObject)"/>).</summary>
        /// <param name="parent">The parent element. Will return true if 'child' is inside this.</param>
        /// <param name="child">The child element. Will return true if this is inside 'parent'.</param>
        public static bool IsParentOf(this DependencyObject parent, DependencyObject child) {
            var cur = child; // Starting at the child,
            while (cur != null) { // Continuing until we run out of elements
                if (cur == parent) // If the current item is the parent, return true
                    return true;
                cur = VisualTreeHelper.GetParent(cur); // Move on to the parent of the current element
            }
            return false; // If we ran out of elements, 'parent' is not a parent of 'child'.
        }


        #region Fluent Helper Methods
        /// <summary>
        /// Tiny extension for the FrameworkElement that allows to set a binding on an element and return that element (so it can be chained).
        /// Used in the TypeControlMap to shorten the code.
        /// </summary>
        public static T WithBinding<T>(this T self, DependencyProperty dp, Binding binding, BindingMode? bindingMode = null, IValueConverter converter = null) where T : FrameworkElement {
            if (bindingMode.HasValue)
                binding.Mode = bindingMode.Value;
            if (converter != null)
                binding.Converter = converter;
            self.SetBinding(dp, binding);
            return self;
        }

        /// <summary>
        /// Tiny extension for the FrameworkElement that allows to set a binding on an element and return that element (so it can be chained).
        /// Used in the TypeControlMap to shorten the code.
        /// </summary>
        public static T WithBinding<T>(this T self, DependencyProperty dp, object source, string path, BindingMode? bindingMode = null, IValueConverter converter = null) where T : FrameworkElement
            => self.WithBinding(dp, new Binding(path) { Source = source }, bindingMode, converter);

        /// <summary>
        /// Adds a child to the given target.
        /// </summary>
        public static T WithChild<T>(this T self, object child) where T : IAddChild {
            self.AddChild(child);
            return self;
        }

        /// <summary>
        /// Adds a child in with the given dock to the target DockPanel based element.
        /// </summary>
        public static T WithChild<T>(this T self, UIElement child, Dock dock) where T : DockPanel {
            DockPanel.SetDock(child, dock);
            self.Children.Add(child);
            return self;
        }
        #endregion
    }
}
