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

namespace Aurora.Profiles.Payday_2.Layers
{
    /// <summary>
    /// Interaction logic for Control_PD2FlashbangLayer.xaml
    /// </summary>
    public partial class Control_PD2FlashbangLayer : UserControl
    {
        private bool settingsset = false;

        public Control_PD2FlashbangLayer()
        {
            InitializeComponent();
        }

        public Control_PD2FlashbangLayer(PD2FlashbangLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is PD2FlashbangLayerHandler && !settingsset)
            {
                this.ColorPicker_Flashbang.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as PD2FlashbangLayerHandler).Properties._FlashbangColor ?? System.Drawing.Color.Empty);

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

        private void ColorPicker_Flashbang_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is PD2FlashbangLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as PD2FlashbangLayerHandler).Properties._FlashbangColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }
    }
}
