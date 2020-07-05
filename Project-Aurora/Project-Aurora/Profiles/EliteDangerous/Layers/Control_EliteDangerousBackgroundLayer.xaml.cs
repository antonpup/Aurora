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

namespace Aurora.Profiles.EliteDangerous.Layers
{
    /// <summary>
    /// Interaction logic for Control_EliteDangerousBackgroundLayer.xaml
    /// </summary>
    public partial class Control_EliteDangerousBackgroundLayer : UserControl
    {
        private bool settingsset = false;

        public Control_EliteDangerousBackgroundLayer()
        {
            InitializeComponent();
        }

        public Control_EliteDangerousBackgroundLayer(EliteDangerousBackgroundLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is EliteDangerousBackgroundLayerHandler && !settingsset)
            {
                this.ColorPicker_CombatMode.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as EliteDangerousBackgroundLayerHandler).Properties._CombatModeColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_DiscoveryMode.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as EliteDangerousBackgroundLayerHandler).Properties._DiscoveryModeColor ?? System.Drawing.Color.Empty);

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

        private void ColorPicker_CombatMode_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousBackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousBackgroundLayerHandler).Properties._CombatModeColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_DiscoveryMode_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is EliteDangerousBackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as EliteDangerousBackgroundLayerHandler).Properties._DiscoveryModeColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }
    }
}
