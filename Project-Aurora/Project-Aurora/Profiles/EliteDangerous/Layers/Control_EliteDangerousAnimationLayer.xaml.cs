using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aurora.Profiles.EliteDangerous.Layers
{
    /// <summary>
    /// Interaction logic for Control_EliteDangerousAnimationLayer.xaml
    /// </summary>
    public partial class Control_EliteDangerousAnimationLayer : UserControl
    {
        private bool settingsset = false;

        public Control_EliteDangerousAnimationLayer()
        {
            InitializeComponent();
        }

        public Control_EliteDangerousAnimationLayer(EliteDangerousAnimationLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

//        public void SetSettings()
//        {
//            if (this.DataContext is EliteDangerousAnimationLayerHandler && !settingsset)
//            {
//                this.ColorPicker_Default.SelectedColor = ColorUtils.DrawingColorToMediaColor((this.DataContext as EliteDangerousAnimationLayerHandler).Properties._DefaultColor ?? System.Drawing.Color.Empty);
//
//                settingsset = true;
//            }
//        }

        internal void SetProfile(Application profile)
        {
        }

        private void UserControl_Loaded(object? sender, RoutedEventArgs e)
        {
//            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }

        private void ColorPicker_Default_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
//            if (IsLoaded && settingsset && this.DataContext is EliteDangerousAnimationLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
//                (this.DataContext as EliteDangerousAnimationLayerHandler).Properties._DefaultColor = ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }
    }
}
