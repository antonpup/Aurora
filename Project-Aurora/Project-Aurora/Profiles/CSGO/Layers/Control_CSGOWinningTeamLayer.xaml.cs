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
    /// Interaction logic for Control_WinningTeamLayer.xaml
    /// </summary>
    public partial class Control_CSGOWinningTeamLayer : UserControl
    {
        private bool settingsset = false;

        public Control_CSGOWinningTeamLayer()
        {
            InitializeComponent();
        }

        public Control_CSGOWinningTeamLayer(CSGOWinningTeamLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is CSGOWinningTeamLayerHandler && !settingsset)
            {
                this.ColorPicker_CT.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as CSGOWinningTeamLayerHandler).Properties._CTColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_T.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as CSGOWinningTeamLayerHandler).Properties._TColor ?? System.Drawing.Color.Empty);

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

        private void ColorPicker_CT_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOWinningTeamLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as CSGOWinningTeamLayerHandler).Properties._CTColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_T_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is CSGOWinningTeamLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as CSGOWinningTeamLayerHandler).Properties._TColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }
    }
}
