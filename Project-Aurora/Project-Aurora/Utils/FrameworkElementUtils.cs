using System.Windows;
using System.Windows.Data;

namespace Aurora.Utils {
    public static class FrameworkElementExtensions {

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
    }
}
