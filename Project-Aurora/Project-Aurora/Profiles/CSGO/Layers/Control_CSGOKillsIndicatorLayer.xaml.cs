using Aurora.Settings;
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

namespace Aurora.Profiles.CSGO.Layers
{
    /// <summary>
    /// Interaction logic for Control_CSGOKillIndicatorLayer.xaml
    /// </summary>
    public partial class Control_CSGOKillIndicatorLayer : UserControl
    {
        private bool settingsset = false;

        public Control_CSGOKillIndicatorLayer()
        {
            InitializeComponent();
        }

        public Control_CSGOKillIndicatorLayer(CSGOKillIndicatorLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is CSGOKillIndicatorLayerHandler && !settingsset)
            {
                this.ColorPicker_RegularKill.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as CSGOKillIndicatorLayerHandler).Properties._RegularKillColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_HeadshotKill.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as CSGOKillIndicatorLayerHandler).Properties._HeadshotKillColor ?? System.Drawing.Color.Empty);
                this.KeySequence_keys.Sequence = (this.DataContext as CSGOKillIndicatorLayerHandler).Properties._Sequence;

                settingsset = true;
            }
        }

        internal void SetProfile(Application profile)
        {
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }

        private void ColorPicker_RegularKill_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOKillIndicatorLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as CSGOKillIndicatorLayerHandler).Properties._RegularKillColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_HeadshotKill_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOKillIndicatorLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as CSGOKillIndicatorLayerHandler).Properties._HeadshotKillColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void KeySequence_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOKillIndicatorLayerHandler && sender is Aurora.Controls.KeySequence)
            {
                (this.DataContext as CSGOKillIndicatorLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
            }
        }
    }
}
