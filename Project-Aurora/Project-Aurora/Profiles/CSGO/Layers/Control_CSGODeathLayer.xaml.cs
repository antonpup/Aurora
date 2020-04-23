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
    /// Interaction logic for Control_CSGODeathLayer.xaml
    /// </summary>
    public partial class Control_CSGODeathLayer : UserControl
    {
        private bool settingsset = false;

        public Control_CSGODeathLayer()
        {
            InitializeComponent();
        }

        public Control_CSGODeathLayer(CSGODeathLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is CSGODeathLayerHandler && !settingsset)
            {
                CSGODeathLayerHandlerProperties properties = (this.DataContext as CSGODeathLayerHandler).Properties;

                this.ColorPicker_DeathColor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor(properties._DeathColor ?? System.Drawing.Color.Empty);
                this.IntegerUpDown_FadeOutAfter.Value = properties.FadeOutAfter;

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

        private void ColorPicker_DeathColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGODeathLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as CSGODeathLayerHandler).Properties._DeathColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void IntegerUpDown_FadeOutAfter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGODeathLayerHandler && sender is Xceed.Wpf.Toolkit.IntegerUpDown && (sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value.HasValue)
                (this.DataContext as CSGODeathLayerHandler).Properties._FadeOutAfter = (sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value;
        }
    }
}
