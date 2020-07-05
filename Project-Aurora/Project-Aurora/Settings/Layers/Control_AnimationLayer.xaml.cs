using Aurora.Controls;
using Aurora.Settings.Overrides.Logic;
using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aurora.Settings.Layers
{
    /// <summary>
    /// Interaction logic for Control_AnimationLayer.xaml
    /// </summary>
    public partial class Control_AnimationLayer : UserControl
    {
        private Window windowAnimationEditor = null;
        private static bool windowAnimationEditorOpen;

        private Profiles.Application profile;
        private bool? triggerPathItemsAreBoolean = null;
        private bool settingsset = false;
        private bool profileset = false;

        public Control_AnimationLayer(AnimationLayerHandler datacontext) {
            InitializeComponent();
            DataContext = datacontext;

            // Populate comboboxs
            var triggerModeLCV = new ListCollectionView(Enum.GetValues(typeof(AnimationTriggerMode))
                .Cast<AnimationTriggerMode>()
                .Select(mode => new { Key = mode.GetDescription(), Value = mode, Description = mode.GetCategory() })
                .ToList()
            );
            triggerModeLCV.GroupDescriptions.Add(new PropertyGroupDescription("Description"));
            triggerModeCb.ItemsSource = triggerModeLCV;
            stackModeCb.ItemsSource = EnumUtils.GetEnumItemsSource<AnimationStackMode>();

            UpdateUI();
        }

        private bool CanSet => IsLoaded && settingsset && DataContext is AnimationLayerHandler;
        private AnimationLayerHandler Context => DataContext as AnimationLayerHandler;

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            SetSettings();
            Loaded -= UserControl_Loaded;
        }

        public void SetSettings() {
            if (DataContext is AnimationLayerHandler && !settingsset) {
                chkboxForceKeySequence.IsChecked = Context.Properties._forceKeySequence;
                chkboxScaleToKeySequence.IsChecked = Context.Properties._scaleToKeySequenceBounds;
                KeySequence_keys.Sequence = Context.Properties._Sequence;
                updownAnimationDuration.Value = (double)Context.Properties._AnimationDuration;
                updownAnimationRepeat.Value = Context.Properties._AnimationRepeat;
                triggerModeCb.SelectedValue = Context.Properties.TriggerMode;
                triggerAnyKey.IsChecked = Context.Properties._TriggerAnyKey;
                triggerPath.Text = Context.Properties._TriggerPath;
                triggerKeys.Sequence = Context.Properties._TriggerKeySequence;
                translateToKey.IsChecked = Context.Properties._KeyTriggerTranslate;
                stackModeCb.SelectedValue = Context.Properties.StackMode;
                whileKeyHeldTerminate.IsChecked = Context.Properties._WhileKeyHeldTerminateRunning;
                settingsset = true;
            }
        }

        internal void SetProfile(Profiles.Application profile) {
            if (profile != null && !profileset) {
                this.profile = profile;
                AttachedApplication.SetApplication(triggerEvaluatable, profile);
                UpdatePathCombobox();
                profileset = true;
            }
            settingsset = false;
            SetSettings();
        }

        private void UpdatePathCombobox() {
            bool isTriggerBoolean = AnimationLayerHandler.IsTriggerBooleanValueBased(Context.Properties.TriggerMode);
            // If the trigger items are currently all booleans, and the trigger is now boolean, don't re-populate the list (it will clear the user's selection).
            // Same goes for if it already contains numeric items and we are now in a numeric mode.
            //if (triggerPathItemsAreBoolean.HasValue && triggerPathItemsAreBoolean == isTriggerBoolean)
            //    return;
            triggerPathItemsAreBoolean = isTriggerBoolean;

            // Get a list of the parameters. If trigger is boolean mode, filters to only boolean values, else does numeric values
            /*triggerPath.ItemsSource = profile?.ParameterLookup?
                .Where(kvp => isTriggerBoolean
                    ? kvp.Value.Item1 == typeof(bool)
                    : TypeUtils.IsNumericType(kvp.Value.Item1)
                )
                .Select(kvp => kvp.Key)
                .ToList();*/
        }

        private void btnEditAnimation_Click(object sender, RoutedEventArgs e) {
            if (windowAnimationEditor == null) {
                if (windowAnimationEditorOpen == true) {
                    MessageBox.Show("Animation Editor already open for another layer.\r\nPlease close it.");
                    return;
                }

                windowAnimationEditor = new Window();
                windowAnimationEditor.Closed += WindowAnimationEditor_Closed;

                windowAnimationEditor.Title = "Animation Editor";

                Controls.Control_AnimationEditor animEditor = new Controls.Control_AnimationEditor() { AnimationMix = Context.Properties._AnimationMix };
                animEditor.AnimationMixUpdated += AnimEditor_AnimationMixUpdated;

                windowAnimationEditor.Content = animEditor;
                windowAnimationEditor.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                windowAnimationEditor.Show();
            } else {
                windowAnimationEditor.BringIntoView();
            }

            windowAnimationEditorOpen = true;
        }

        private void WindowAnimationEditor_Closed(object sender, EventArgs e) {
            windowAnimationEditor = null;
            windowAnimationEditorOpen = false;
        }

        private void AnimEditor_AnimationMixUpdated(object sender, EffectsEngine.Animations.AnimationMix mix) {
            if (CanSet && sender is Controls.Control_AnimationEditor)
                (this.DataContext as AnimationLayerHandler).Properties._AnimationMix = mix;
        }

        private void chkboxForceKeySequence_Checked(object sender, RoutedEventArgs e) {
            if (CanSet && sender is CheckBox)
                Context.Properties._forceKeySequence = ((sender as CheckBox).IsChecked.HasValue ? (sender as CheckBox).IsChecked.Value : false);
        }

        private void chkboxScaleToKeySequence_Checked(object sender, RoutedEventArgs e) {
            if (CanSet && sender is CheckBox)
                Context.Properties._scaleToKeySequenceBounds = ((sender as CheckBox).IsChecked.HasValue ? (sender as CheckBox).IsChecked.Value : false);
        }

        private void KeySequence_keys_SequenceUpdated(object sender, EventArgs e) {
            if (CanSet && sender is Controls.KeySequence)
                Context.Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
        }

        private void updownAnimationDuration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (CanSet && sender is Xceed.Wpf.Toolkit.DoubleUpDown)
                Context.Properties._AnimationDuration = (float)((sender as Xceed.Wpf.Toolkit.DoubleUpDown).Value.HasValue ? (sender as Xceed.Wpf.Toolkit.DoubleUpDown).Value.Value : 0.0f);
        }

        private void updownAnimationRepeat_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (CanSet && sender is Xceed.Wpf.Toolkit.IntegerUpDown)
                Context.Properties._AnimationRepeat = ((sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value.HasValue ? (sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value.Value : 0);
        }

        private void triggerMode_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selectedItem = (AnimationTriggerMode)(sender as ComboBox).SelectedValue;
            if (CanSet)
                Context.Properties._TriggerMode = selectedItem;

            UpdateUI();

            // If the evaluatable is not the correct type or it is null, then create the default Evaluatable for it
            if (AnimationLayerHandler.IsTriggerEvaluatableNumericValueBased(selectedItem) && !typeof(Evaluatable<double>).IsAssignableFrom(Context.Properties._EvaluatableTrigger?.GetType()))
                Context.Properties._EvaluatableTrigger = EvaluatableDefaults.Get<double>();
            else if (AnimationLayerHandler.IsTriggerEvaluatableBooleanValueBased(selectedItem) && !typeof(Evaluatable<bool>).IsAssignableFrom(Context.Properties._EvaluatableTrigger?.GetType()))
                Context.Properties._EvaluatableTrigger = EvaluatableDefaults.Get<bool>();

            // Update the evaluatable control
            triggerEvaluatable.EvalType = AnimationLayerHandler.IsTriggerEvaluatableNumericValueBased(selectedItem) ? typeof(double) : typeof(bool);
            triggerEvaluatable.Expression = Context.Properties._EvaluatableTrigger;
        }

        private void UpdateUI() {
            // Shortcut to the trigger mode
            var trigMode = Context.Properties.TriggerMode;

            // Only show trigger path when one of the path-like modes is set
            triggerGridLayout.RowDefinitions[1].Height = new GridLength(AnimationLayerHandler.IsTriggerNumericValueBased(trigMode) || AnimationLayerHandler.IsTriggerBooleanValueBased(trigMode) ? 28 : 0);
            // Only show IEvaluatable<object>when one of the evaluatable modes is set
            triggerGridLayout.RowDefinitions[2].Height = new GridLength(0, AnimationLayerHandler.IsTriggerEvaluatableNumericValueBased(trigMode) || AnimationLayerHandler.IsTriggerEvaluatableBooleanValueBased(trigMode) ? GridUnitType.Auto : GridUnitType.Pixel);
            // Only show tigger keys when one of the key-like modes is set
            triggerGridLayout.RowDefinitions[3].Height = new GridLength(AnimationLayerHandler.IsTriggerKeyBased(trigMode) ? 160 : 0);
            triggerGridLayout.RowDefinitions[4].Height = new GridLength(AnimationLayerHandler.IsTriggerKeyBased(trigMode) ? 28 : 0);
            // Only show the stack mode setting if the trigger mode is NOT "AlwaysOn"
            triggerGridLayout.RowDefinitions[5].Height = new GridLength(trigMode == AnimationTriggerMode.AlwaysOn ? 0 : 28);
            // Only show the force terminate setting if the trigger mode is key press or when key held (not released)
            triggerGridLayout.RowDefinitions[6].Height = new GridLength(trigMode == AnimationTriggerMode.OnKeyPress || trigMode == AnimationTriggerMode.WhileKeyHeld ? 28 : 0);

            // Use all available space when in an evaluatable mode (since these controls use a lot of space)
            triggerGroupbox.Width = AnimationLayerHandler.IsTriggerEvaluatableBooleanValueBased(trigMode) || AnimationLayerHandler.IsTriggerEvaluatableNumericValueBased(trigMode) ? double.NaN : 333;
            triggerGroupbox.HorizontalAlignment = AnimationLayerHandler.IsTriggerEvaluatableBooleanValueBased(trigMode) || AnimationLayerHandler.IsTriggerEvaluatableNumericValueBased(trigMode) ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;

            // Update the combobox
            UpdatePathCombobox();
        }

        private void triggerPath_TextChanged(object sender, TextChangedEventArgs e) {
            if (CanSet)
                Context.Properties._TriggerPath = (sender as ComboBox).Text;
        }

        private void triggerAnyKey_Checked(object sender, RoutedEventArgs e) {
            bool val = (sender as CheckBox).IsChecked ?? false;
            if (CanSet)
                Context.Properties._TriggerAnyKey = val;

            // Disable keybind box if allow on any keys
            triggerKeys.IsEnabled = !val;
        }

        private void triggerKeys_SequenceUpdated(object sender, EventArgs e) {
            if (CanSet)
                Context.Properties._TriggerKeySequence = (sender as Controls.KeySequence).Sequence;
        }

        private void translateToKey_Checked(object sender, RoutedEventArgs e) {
            if (CanSet)
                Context.Properties._KeyTriggerTranslate = (sender as CheckBox).IsChecked;
        }

        private void stackModeCb_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (CanSet)
                Context.Properties._StackMode = (AnimationStackMode)(sender as ComboBox).SelectedValue;
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e) {
            // Open the online documentation for the Animation Trigger properties
            Process.Start(new ProcessStartInfo(@"https://aurora-rgb.github.io/Docs/advanced-topics/animation-editor/"));
        }

        private void whileKeyHeldTerminate_Checked(object sender, RoutedEventArgs e) {
            if (CanSet)
                Context.Properties._WhileKeyHeldTerminateRunning = (sender as CheckBox).IsChecked;
        }

        private void triggerEvaluatable_ExpressionChanged(object sender, ExpressionChangeEventArgs e) {
            if (CanSet)
                Context.Properties._EvaluatableTrigger = e.NewExpression;
        }
    }
}
