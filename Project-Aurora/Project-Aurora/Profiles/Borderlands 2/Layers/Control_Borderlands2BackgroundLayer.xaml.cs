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

namespace Aurora.Profiles.Borderlands2.Layers
{
    /// <summary>
    /// Interaction logic for Control_Borderlands2BackgroundLayer.xaml
    /// </summary>
    public partial class Control_Borderlands2BackgroundLayer : UserControl
    {
        private bool settingsset = false;
        private bool profileset = false;

        public Control_Borderlands2BackgroundLayer()
        {
            InitializeComponent();
        }

        public Control_Borderlands2BackgroundLayer(Borderlands2BackgroundLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is Borderlands2BackgroundLayerHandler && !settingsset)
            {
                this.ColorPicker_Background.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ColorBackground ?? System.Drawing.Color.Empty);
                
                settingsset = true;
            }
        }

        internal void SetProfile(Application profile)
        {
            if (profile != null && !profileset)
            {
                var var_types_numerical = profile.ParameterLookup?.Where(kvp => Utils.TypeUtils.IsNumericType(kvp.Value.Item1));

                profileset = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }
        
        private void ColorPicker_Background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is Borderlands2BackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as Borderlands2BackgroundLayerHandler).Properties._ColorBackground = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }
    }
}
