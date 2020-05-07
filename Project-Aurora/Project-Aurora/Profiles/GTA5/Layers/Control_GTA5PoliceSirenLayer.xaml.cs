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

namespace Aurora.Profiles.GTA5.Layers
{
    /// <summary>
    /// Interaction logic for Control_GTA5PoliceSirenLayer.xaml
    /// </summary>
    public partial class Control_GTA5PoliceSirenLayer : UserControl
    {
        private bool settingsset = false;

        public Control_GTA5PoliceSirenLayer()
        {
            InitializeComponent();
        }

        public Control_GTA5PoliceSirenLayer(GTA5PoliceSirenLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is GTA5PoliceSirenLayerHandler && !settingsset)
            {
                this.ColorPicker_LeftSiren.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as GTA5PoliceSirenLayerHandler).Properties._LeftSirenColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_RightSiren.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as GTA5PoliceSirenLayerHandler).Properties._RightSirenColor ?? System.Drawing.Color.Empty);
                this.ComboBox_SirenEffectType.SelectedItem = (this.DataContext as GTA5PoliceSirenLayerHandler).Properties._SirenType;
                this.Checkbox_DisplayOnPeripherals.IsChecked = (this.DataContext as GTA5PoliceSirenLayerHandler).Properties._PeripheralUse;
                this.KeySequence_LeftSiren.Sequence = (this.DataContext as GTA5PoliceSirenLayerHandler).Properties._LeftSirenSequence;
                this.KeySequence_RightSiren.Sequence = (this.DataContext as GTA5PoliceSirenLayerHandler).Properties._RightSirenSequence;

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

        private void ColorPicker_LeftSiren_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5PoliceSirenLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as GTA5PoliceSirenLayerHandler).Properties._LeftSirenColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_RightSiren_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5PoliceSirenLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as GTA5PoliceSirenLayerHandler).Properties._RightSirenColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void Checkbox_DisplayOnPeripherals_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5PoliceSirenLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
                (this.DataContext as GTA5PoliceSirenLayerHandler).Properties._PeripheralUse = (sender as CheckBox).IsChecked.Value;
        }

        private void KeySequence_LeftSiren_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5PoliceSirenLayerHandler && sender is Aurora.Controls.KeySequence)
            {
                (this.DataContext as GTA5PoliceSirenLayerHandler).Properties._LeftSirenSequence = (sender as Aurora.Controls.KeySequence).Sequence;
            }
        }

        private void KeySequence_RightSiren_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5PoliceSirenLayerHandler && sender is Aurora.Controls.KeySequence)
            {
                (this.DataContext as GTA5PoliceSirenLayerHandler).Properties._RightSirenSequence = (sender as Aurora.Controls.KeySequence).Sequence;
            }
        }

        private void ComboBox_SirenEffectType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is GTA5PoliceSirenLayerHandler && sender is ComboBox)
            {
                (this.DataContext as GTA5PoliceSirenLayerHandler).Properties._SirenType = (GTA5_PoliceEffects)Enum.Parse(typeof(GTA5_PoliceEffects), (sender as ComboBox).SelectedIndex.ToString());
            }
        }
    }
}
