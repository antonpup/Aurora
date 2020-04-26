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
    /// Interaction logic for Control_CSGOBurningLayer.xaml
    /// </summary>
    public partial class Control_CSGOBurningLayer : UserControl
    {
        private bool settingsset = false;

        public Control_CSGOBurningLayer()
        {
            InitializeComponent();
        }

        public Control_CSGOBurningLayer(CSGOBurningLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is CSGOBurningLayerHandler && !settingsset)
            {
                this.ColorPicker_Burning.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as CSGOBurningLayerHandler).Properties._BurningColor ?? System.Drawing.Color.Empty);
                this.checkBox_Animated.IsChecked = (this.DataContext as CSGOBurningLayerHandler).Properties._Animated;

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

        private void ColorPicker_Burning_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOBurningLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as CSGOBurningLayerHandler).Properties._BurningColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void checkBox_Animated_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOBurningLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
                (this.DataContext as CSGOBurningLayerHandler).Properties._Animated = (sender as CheckBox).IsChecked.Value;
        }
    }
}
