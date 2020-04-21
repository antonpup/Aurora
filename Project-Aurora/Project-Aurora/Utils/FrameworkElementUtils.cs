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
    }
}
