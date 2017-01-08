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

namespace Aurora.Settings.Layers
{
    /// <summary>
    /// Interaction logic for Control_SolidFillLayer.xaml
    /// </summary>
    public partial class Control_SolidFillLayer : UserControl
    {
        private bool settingsset = false;

        public Control_SolidFillLayer()
        {
            InitializeComponent();
        }

        public Control_SolidFillLayer(SolidFillLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if(this.DataContext is SolidFillLayerHandler && !settingsset)
            {
                this.ColorPicker_primaryColor.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as SolidFillLayerHandler).Properties._PrimaryColor ?? System.Drawing.Color.Empty);

                settingsset = true;
            }
        }

        private void ColorPicker_primaryColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is SolidFillLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
            {
                (this.DataContext as SolidFillLayerHandler).Properties._PrimaryColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }
    }
}
