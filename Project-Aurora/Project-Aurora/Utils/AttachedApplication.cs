using System.Windows;
using System.Windows.Data;
using Application = Aurora.Profiles.Application;

namespace Aurora.Utils {

    /// <summary>
    /// This class exposes an inherited attached property which allows for providing a cascading application context down to subcontrols.
    /// </summary>
    public static class AttachedApplication {

        /* This attached property allows for providing a cascading application context down to subcontrols.
         * Also provides a binding that will automatically provide this value to relevant dependency properties. */
        public static Application GetApplication(DependencyObject obj) => (Application)obj.GetValue(ApplicationProperty);
        public static void SetApplication(DependencyObject obj, Application value) => obj.SetValue(ApplicationProperty, value);

        // Using a DependencyProperty as the backing store for Application.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ApplicationProperty =
            DependencyProperty.RegisterAttached("Application", typeof(Application), typeof(AttachedApplication), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior | FrameworkPropertyMetadataOptions.AffectsRender));
    }

    /// <summary>
    /// A binding that automatically sets itself up to be bound to the <see cref="AttachedApplication.ApplicationProperty"/> attached property.
    /// </summary>
    public class AttachedApplicationBinding : Binding {
        public AttachedApplicationBinding() {
            Path = new PropertyPath("(0)", AttachedApplication.ApplicationProperty);
            RelativeSource = new RelativeSource(RelativeSourceMode.Self);
        }
    }
}
