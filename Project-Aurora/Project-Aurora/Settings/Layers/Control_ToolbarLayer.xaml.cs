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
    /// Interaction logic for Control_ToolbarLayer.xaml
    /// </summary>
    public partial class Control_ToolbarLayer : UserControl {

        private bool settingsset = false;

        public Control_ToolbarLayer(ToolbarLayerHandler context) {
            InitializeComponent();
            DataContext = context;
            Loaded += (obj, e) => SetSettings();
        }

        public void SetSettings() {
            if (DataContext is ToolbarLayerHandler && !settingsset) {
                ToolbarLayerHandlerProperties ctxProps = (DataContext as ToolbarLayerHandler).Properties;
                DefaultColor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(ctxProps._PrimaryColor ?? System.Drawing.Color.Empty);
                ActiveColor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(ctxProps._SecondaryColor ?? System.Drawing.Color.Empty);
                EnableScroll.IsChecked = ctxProps._EnableScroll;
                ScrollLoop.IsChecked = ctxProps._ScrollLoop;
                Keys.Sequence = ctxProps._Sequence;
                settingsset = true;
            }
        }

        private bool CanSave { get { return IsLoaded && settingsset && DataContext is ToolbarLayerHandler; } }
        private ToolbarLayerHandler Context { get { return (ToolbarLayerHandler)DataContext; } }

        private void DefaultColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (CanSave && (sender as ColorPicker).SelectedColor.HasValue)
                Context.Properties._PrimaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
        }

        private void ActiveColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (CanSave && (sender as ColorPicker).SelectedColor.HasValue)
                Context.Properties._SecondaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as ColorPicker).SelectedColor.Value);
        }

        private void EnableScroll_Checked(object sender, RoutedEventArgs e) {
            if (CanSave)
                Context.Properties._EnableScroll = (sender as CheckBox).IsChecked ?? false;
        }

        private void ScrollLoop_Checked(object sender, RoutedEventArgs e) {
            if (CanSave)
                Context.Properties._ScrollLoop = (sender as CheckBox).IsChecked ?? false;
        }

        private void Keys_SequenceUpdated(object sender, EventArgs e) {
            if (CanSave && sender is Controls.KeySequence)
                Context.Properties._Sequence = (sender as Controls.KeySequence).Sequence;
        }
    }
}
