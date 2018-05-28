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
    /// Interaction logic for Control_ConditionalLayer.xaml
    /// </summary>
    public partial class Control_ConditionalLayer : UserControl {

        private bool settingsset = false;
        private bool profileset = false;

        public Control_ConditionalLayer() {
            InitializeComponent();
        }

        public Control_ConditionalLayer(ConditionalLayerHandler datacontext) {
            InitializeComponent();
            this.DataContext = datacontext;
        }

        public void SetSettings() {
            if (this.DataContext is ConditionalLayerHandler && !settingsset) {
                ConditionalLayerHandler context = (ConditionalLayerHandler)this.DataContext;
                this.trueColor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(context.Properties._PrimaryColor ?? System.Drawing.Color.Empty);
                this.falseColor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(context.Properties._SecondaryColor ?? System.Drawing.Color.Empty);
                this.conditionPath.Text = context.Properties._ConditionPath;
                this.keySequence.Sequence = context.Properties._Sequence;

                settingsset = true;
            }
        }

        internal void SetProfile(Profiles.Application profile) {
            if (profile != null && !profileset) {
                var var_types_boolean = profile.ParameterLookup?.Where(kvp => Type.GetTypeCode(kvp.Value.Item1) == TypeCode.Boolean);
                conditionPath.Items.Clear();
                foreach (var item in var_types_boolean)
                    conditionPath.Items.Add(item.Key);

                profileset = true;
            }
            settingsset = false;
            this.SetSettings();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            SetSettings();
            this.Loaded -= UserControl_Loaded;
        }

        private void trueColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (IsLoaded && settingsset && this.DataContext is ConditionalLayerHandler && (sender as ColorPicker).SelectedColor.HasValue)
                (this.DataContext as ConditionalLayerHandler).Properties._PrimaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
        }

        private void falseColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (IsLoaded && settingsset && this.DataContext is ConditionalLayerHandler && (sender as ColorPicker).SelectedColor.HasValue)
                (this.DataContext as ConditionalLayerHandler).Properties._SecondaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
        }

        private void conditionPath_TextChanged(object sender, TextChangedEventArgs e) {
            if (IsLoaded && settingsset && this.DataContext is ConditionalLayerHandler)
                (this.DataContext as ConditionalLayerHandler).Properties._ConditionPath = (sender as ComboBox).Text;
        }

        private void keySequence_SequenceUpdated(object sender, EventArgs e) {
            if (IsLoaded && settingsset && this.DataContext is ConditionalLayerHandler)
                (this.DataContext as ConditionalLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
        }
    }
}
