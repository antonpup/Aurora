using Aurora.Profiles;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Application = Aurora.Profiles.Application;

namespace Aurora.Controls {

    public partial class GameStateParameterPicker : UserControl, INotifyPropertyChanged {

        public event EventHandler<SelectedPathChangedEventArgs> SelectedPathChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private List<string> parameterList;

        public GameStateParameterPicker() {
            InitializeComponent();
        }

        #region UI Properties
        /// <summary>
        /// The current parts that make up the path. E.G. "LocalPCInfo/RAM" -> "LocalPCInfo", "RAM"
        /// </summary>
        private Stack<string> WorkingPath { get; set; } = new Stack<string>();

        /// <summary>
        /// Lazy-evaluated list of parameters for this application and property type.
        /// </summary>
        public List<string> ParameterList => parameterList ?? (parameterList = Application?.ParameterLookup?.GetParameters(PropertyType).ToList());

        /// <summary>
        /// Gets a list of items that should be displayed in the parameter list (based on the current "parent" variable).
        /// </summary>
        public IEnumerable<PathOption> CurrentParameterListItems {
            get {
                // If the application or param lookup is null, we don't know the parameters so do nothing
                if (Application?.ParameterLookup == null) return null;

                // If the given working path is a path to a variable (which it shouldn't be), pop the last item (the variable name) from the path to give just the "directory"
                if (Application.ParameterLookup.IsValidParameter(WorkingPathStr))
                    WorkingPath.Pop();

                // Generate the string version of this working path (and cache it)
                var _workingPath = WorkingPathStr;
                if (_workingPath != "") _workingPath += "/"; // If not at the root directory, add / to the end of the test path. This means it doesn't get confused with things such as `CPU` and `CPUUsage`.
                return from path in ParameterList // With all properties in the current param lookup that are of a valid type (e.g. numbers)
                       where path.StartsWith(_workingPath) // Pick only the ones that start with the same working path
                       let pathSplit = path.Substring(_workingPath.Length).Split('/') // Get a list of all remaining parts of the path (e.g. if this was A/B/C and current path was A, pathSplit would be 'B', 'C')
                       let isFolder = pathSplit.Length > 1 // If there is more than one part of the path remaining, this must be a directory
                       group isFolder by pathSplit[0] into g // Group by the path name so duplicates are removed
                       orderby !g.First(), g.Key // Order the remaining (distinct) items by folders first, then order by their name
                       select new PathOption(g.Key, g.First()); // Finally, put them in a POCO so we can bind the UI to these properties.
            }
        }

        /// <summary>
        /// Returns the string representation of the current working path.
        /// </summary>
        public string WorkingPathStr => string.Join("/", WorkingPath.Reverse());
        #endregion

        #region IsOpen Dependency Property
        /// <summary>
        /// Whether or not the dropdown for this picker is open.
        /// </summary>
        public bool IsOpen {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(GameStateParameterPicker), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region SelectedPath Dependency Property
        /// <summary>
        /// The path to a GameStateVariable the user has selected.
        /// </summary>
        public string SelectedPath {
            get => (string)GetValue(SelectedPathProperty);
            set => SetValue(SelectedPathProperty, value);
        }

        public static readonly DependencyProperty SelectedPathProperty =
            DependencyProperty.Register(nameof(SelectedPath), typeof(string), typeof(GameStateParameterPicker), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedPathDPChanged));

        private static void SelectedPathDPChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            // Do nothing if the value hasn't actually changed.
            if (e.OldValue == e.NewValue || e.NewValue == null) return;
            
            var picker = (GameStateParameterPicker)sender;
            
            // If the path isn't valid, set it to "".
            if (!picker.ValidatePath((string)e.NewValue)) {
                picker.SelectedPath = "";
                // We need to return out of this method as we don't want to change anything since NewValue is now no longer respective of the new new value.
                return;
            }

            if (double.TryParse(e.NewValue.ToString(), out double val)) {
                // If a raw number has been entered, fill in the numeric stepper
                picker.numericEntry.Value = val;
            } else {
                // Else if an actual path has been given, split it up into it's ""directories""
                // For the path to be valid (and to be passed as a param to this method) it will be a path to a variable, not a "directory". We use this assumption.
                picker.WorkingPath = new Stack<string>(e.NewValue.ToString().Split('/'));
                picker.WorkingPath.Pop(); // Remove the last one, since the working path should not include the actual var name
                picker.NotifyChanged(nameof(WorkingPath), nameof(WorkingPathStr), nameof(ParameterList), nameof(CurrentParameterListItems)); // All these things will be different now, so trigger an update of anything requiring them
                picker.mainListBox.SelectedValue = e.NewValue.ToString().Split('/').Last(); // The selected item in the list will be the last part of the path
            }

            // Raise an event informing subscribers
            picker.SelectedPathChanged?.Invoke(picker, new SelectedPathChangedEventArgs(e.OldValue.ToString(), e.NewValue.ToString()));
        }
        #endregion

        #region Application Dependency Property
        /// <summary>
        /// The current application whose context this picker is for. Determines the available variables.
        /// </summary>
        public Application Application {
            get => (Application)GetValue(ApplicationProperty);
            set => SetValue(ApplicationProperty, value);
        }

        public static readonly DependencyProperty ApplicationProperty =
            DependencyProperty.Register(nameof(Application), typeof(Application), typeof(GameStateParameterPicker), new PropertyMetadata(null, ApplicationOrPropertyTypeChange));
        #endregion

        #region PropertyType Dependency Property
        /// <summary>
        /// The types of properties that will be shown to the user.
        /// </summary>
        public PropertyType PropertyType {
            get => (PropertyType)GetValue(PropertyTypeProperty);
            set => SetValue(PropertyTypeProperty, value);
        }

        public static readonly DependencyProperty PropertyTypeProperty =
            DependencyProperty.Register(nameof(PropertyType), typeof(PropertyType), typeof(GameStateParameterPicker), new PropertyMetadata(PropertyType.None, ApplicationOrPropertyTypeChange));

        public static void ApplicationOrPropertyTypeChange(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var picker = (GameStateParameterPicker)sender;
            picker.parameterList = null;
            picker.NotifyChanged(nameof(ParameterList), nameof(CurrentParameterListItems));

            if (!picker.ValidatePath(picker.SelectedPath))
                picker.SelectedPath = "";
        }
        #endregion

        /// <summary>
        /// Determines if the selected path is a valid one (i.e. it is a number or a valid variable in the current application context).
        /// If the current application context is null (i.e. not yet loaded), the path is assumed to be valid.
        /// </summary>
        private bool ValidatePath(string path) =>
            // If application parameter context doesn't exist or there is no set type, assume non loaded and allow the path
            Application?.ParameterLookup == null || PropertyType == PropertyType.None
            // An empty path is fine
            || string.IsNullOrEmpty(path)
            // If we're in number mode, allow the selected path to be a double
            || (PropertyType == PropertyType.Number && double.TryParse(path, out var _))
            // If not in number mode, must be a valid path and have the same type as the expected property type
            || Application.ParameterLookup.IsValidParameter(path, PropertyType);

        #region Animation
        /// <summary>Animates the list boxes.</summary>
        /// <param name="dx">Direction of animation. -1 for previous, 1 for next.</param>
        private void Animate(int dx) {
            var auxillaryScrollViewer = auxillaryListbox.FindChildOfType<ScrollViewer>();
            var mainScrollViewer = mainListBox.FindChildOfType<ScrollViewer>();
            auxillaryScrollViewer.ScrollToVerticalOffset(mainScrollViewer.VerticalOffset);

            // Move the aux to the centre and move the main to the side of it
            SetTransformRelativeOffset(mainListBox, dx);
            SetTransformRelativeOffset(mainListBox, 0);

            // Animate the aux moving away and the main moving in
            CreateStoryboard(dx, 0, mainListBox).Begin();
            CreateStoryboard(0, -dx, auxillaryListbox).Begin();
        }

        /// <summary>Creates a storyboard animation that changes the TransformRelativeOffsetProperty property from `fromX` to `toX` for the given target.</summary>
        private Storyboard CreateStoryboard(int from, int to, UIElement target) {
            var sb = new Storyboard {
                Children = new TimelineCollection(new[] {
                    new DoubleAnimation(from, to, new Duration(new TimeSpan(0, 0, 0, 0, 300))) {
                        EasingFunction = new PowerEase { EasingMode = EasingMode.EaseInOut }
                    }
                })
            };
            Storyboard.SetTarget(sb, target);
            Storyboard.SetTargetProperty(sb, new PropertyPath(TransformRelativeOffsetProperty));
            return sb;
        }

        #region TransformRelativeOffset Attached Property
        public static double GetTransformRelativeOffset(DependencyObject obj) => (double)obj.GetValue(TransformRelativeOffsetProperty);
        public static void SetTransformRelativeOffset(DependencyObject obj, double value) => obj.SetValue(TransformRelativeOffsetProperty, value);

        public static readonly DependencyProperty TransformRelativeOffsetProperty =
            DependencyProperty.RegisterAttached("TransformRelativeOffset", typeof(double), typeof(GameStateParameterPicker), new PropertyMetadata(0d));
        #endregion
        #endregion

        #region Event Handlers
        private void BackBtn_Click(object sender, RoutedEventArgs e) {
            if (WorkingPath.Count > 0) {
                // Make the aux list box take on the same items as the current one so that when animated (since the aux is moved to the middle first) it looks natural
                auxillaryListbox.ItemsSource = CurrentParameterListItems;

                Animate(-1);
                WorkingPath.Pop(); // Remove the last "directory" off the working path
                NotifyChanged(nameof(CurrentParameterListItems), nameof(WorkingPathStr)); // These properties will have changed so any UI stuff that relies on it should update
            }
        }

        private void MainListBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            /* The reason this is a PreviewMouseLeftButtonDown event rather than using a SelectionChanged event is because I was having issues where when the next items
             * were loaded it was immediately selecting the first item and then selecting a path the user didn't click. There is no click event available for the ListBox
             * so that was out of the question. Next I tried double click but it was counter-intuitive as items were being selected by not actually chosen, which could
             * easily confuse users. It also felt unresponsive as it would be a few hundred milliseconds after the double click that it actually called this method.
             * Finally preview left down was chosen as, after a bit of tweaking, it seemed to work properly - felt responsive and didn't repeatedly select new items. The
             * only real downside of this is that the user can't use the up and down arrows to change the selected item but I think that's a small price to pay.
             * Side note: THIS PICKER HAS TAKEN ME SO DAMN LONG TO MAKE. Probably longer than the actual GSI plugin system itself.... But hey, I'm proud of it. */

            // Element selection code is adapted from http://kevin-berridge.blogspot.com/2008/06/wpf-listboxitem-double-click.html
            var el = (UIElement)mainListBox.InputHitTest(e.GetPosition(mainListBox));
            while (el != null && el != mainListBox) {
                if (el is ListBoxItem item) {

                    // Since the user has picked an item on the list, we want to clear the numeric box so it is obvious to the user that the number is having no effect.
                    numericEntry.Value = null;

                    // Copy the current list items to the aux list box incase the list box is animated later. This must be done BEFORE the workingpath.push call.
                    auxillaryListbox.ItemsSource = CurrentParameterListItems;

                    // Add the clicked item to the working path (even if it is an end variable, not a "directory")
                    WorkingPath.Push(((PathOption)item.DataContext).Path);

                    var path = string.Join("/", WorkingPath.Reverse());
                    if (Application?.ParameterLookup?.IsValidParameter(path) ?? false) {
                        // If it turns out the user has selected an end variable, we want to update the DependencyObject for the selected path
                        SelectedPath = path;
                        NotifyChanged(nameof(SelectedPath));
                    } else {
                        // If the user has selected a directory instead (i.e. isn't not a valid parameter) then perform the animation since there will now be new properties to choose from
                        Animate(1);
                    }

                    // Regardless of whether it was a variable or a directory, the list and path will have changed
                    NotifyChanged(nameof(CurrentParameterListItems), nameof(WorkingPathStr));
                }
                el = (UIElement)VisualTreeHelper.GetParent(el);
            }
        }

        private void NumericEntry_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            var v = (sender as Xceed.Wpf.Toolkit.DoubleUpDown).Value;

            // If there is no value, then this will have been set programmatically, so do nothing since we don't want to end up in a change event handler loop
            if (!v.HasValue) return;

            mainListBox.SelectedItem = null; // Clear the selection on the list box (to emphasise to the user it is now irrelevant)
            SelectedPath = v.ToString(); // Set the selectedpath to be the value of this numeric stepper
            NotifyChanged(nameof(SelectedPath));
        }
        #endregion

        private void NotifyChanged(params string[] propNames) {
            foreach (var prop in propNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }


        /// <summary>
        /// Basic POCO for holding a bit of metadata about a path option.
        /// </summary>
        public class PathOption {
            public PathOption(string path, bool isFolder) {
                Path = path;
                IsFolder = isFolder;
            }

            public string DisplayPath => Path.CamelCaseToSpaceCase();
            public string Path { get; }
            public bool IsFolder { get; }
        }
    }


    /// <summary>
    /// EventArgs for an event that is thrown when the selected path of a <see cref="GameStateParameterPicker"/> changes.
    /// </summary>
    public class SelectedPathChangedEventArgs : EventArgs {

        public SelectedPathChangedEventArgs(string oldPath, string newPath) {
            OldPath = oldPath;
            NewPath = newPath;
        }

        public string OldPath { get; }
        public string NewPath { get; }
    }


    /// <summary>
    /// Converter that checks to see if a string is null or whitespace. Returns true if it HAS A VALUE, or false if it IS NULL/WHITESPACE.
    /// </summary>
    public class IsStringNotNullOrWhitespaceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !string.IsNullOrWhiteSpace(value?.ToString());
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => new NotImplementedException();
    }


    /// <summary>
    /// Converter that converts a PropertyType enum value to a GridLength. Used for binding onto one of the row definition properties to hide a row when
    /// the property type is anything other than <see cref="PropertyType.Number" />.
    /// </summary>
    public class PropertyTypeToGridLengthConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new GridLength(0, (PropertyType)value == PropertyType.Number ? GridUnitType.Auto : GridUnitType.Pixel);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => new NotImplementedException();
    }


    /// <summary>
    /// A binding that will create a <see cref="TranslateTransform"/> based on the target's TransformRelativeOffset. The value will be relative to the
    /// size of the element (e.g. if the element's width is 150 and the TransformRelativeOffset is 0.5, then the TranslateTransform's X will be 75).
    /// </summary>
    public class DoubleToRelativeTransformOffset : MultiBinding {

        public DoubleToRelativeTransformOffset() {
            Bindings.Add(new Binding("ActualWidth") { RelativeSource = new RelativeSource(RelativeSourceMode.Self) });
            Bindings.Add(new Binding() { Path = new PropertyPath(GameStateParameterPicker.TransformRelativeOffsetProperty), RelativeSource = new RelativeSource(RelativeSourceMode.Self) });
            Converter = new Conv();
        }

        class Conv : IMultiValueConverter {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => new TranslateTransform((double)values[0] * (double)values[1], 0);
            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }
    }
}
