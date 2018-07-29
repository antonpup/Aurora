using System;
using System.Collections.Generic;
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
using Xceed.Wpf.Toolkit;

namespace Aurora.Settings.Layers {
    /// <summary>
    /// Interaction logic for Control_TimerLayer.xaml
    /// </summary>
    public partial class Control_TimerLayer : UserControl {

        private bool settingsset = false;

        public Control_TimerLayer(TimerLayerHandler context) {
            InitializeComponent();
            DataContext = context;
            Loaded += (obj, e) => SetSettings();

            animationType.Items.Add(TimerLayerAnimationType.OnOff);
            animationType.Items.Add(TimerLayerAnimationType.Fade);

            repeatAction.Items.Add(TimerLayerRepeatPressAction.Reset);
            repeatAction.Items.Add(TimerLayerRepeatPressAction.Extend);
            repeatAction.Items.Add(TimerLayerRepeatPressAction.Ignore);
        }

        public void SetSettings() {
            if (DataContext is TimerLayerHandler && !settingsset) {
                TimerLayerHandlerProperties ctxProps = (DataContext as TimerLayerHandler).Properties;
                defaultColor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(ctxProps._PrimaryColor ?? System.Drawing.Color.Empty);
                activeColor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(ctxProps._SecondaryColor ?? System.Drawing.Color.Empty);
                timerDuration.Value = ctxProps._Duration;
                animationType.SelectedItem = ctxProps._AnimationType;
                repeatAction.SelectedItem = ctxProps._RepeatAction;
                triggerKeyList.Keybinds = ctxProps._TriggerKeys;
                KeySequence_Keys.Sequence = ctxProps._Sequence;
                settingsset = true;
            }
        }

        private bool CanSave { get { return IsLoaded && settingsset && DataContext is TimerLayerHandler; } }
        private TimerLayerHandler Context { get { return (TimerLayerHandler)DataContext; } }

        private void defaultColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (CanSave && (sender as ColorPicker).SelectedColor.HasValue)
                Context.Properties._PrimaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
        }

        private void activeColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (CanSave && (sender as ColorPicker).SelectedColor.HasValue)
                Context.Properties._SecondaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
        }

        private void timerDuration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (CanSave && (sender as IntegerUpDown).Value.HasValue)
                Context.Properties._Duration = (sender as IntegerUpDown).Value;
        }

        private void animationType_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (CanSave)
                Context.Properties._AnimationType = (TimerLayerAnimationType)(sender as ComboBox).SelectedItem;
        }

        private void repeatAction_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (CanSave)
                Context.Properties._RepeatAction = (TimerLayerRepeatPressAction)(sender as ComboBox).SelectedItem;
        }

        private void triggerKeyList_KeybindsChanged(object sender) {
            if (CanSave && sender is Controls.KeyBindList)
                Context.Properties._TriggerKeys = (sender as Controls.KeyBindList).Keybinds;
        }

        private void KeySequence_Keys_SequenceUpdated(object sender, EventArgs e) {
            if (CanSave && sender is Controls.KeySequence)
                Context.Properties._Sequence = (sender as Controls.KeySequence).Sequence;
        }
    }
}
