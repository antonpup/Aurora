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

namespace Aurora.Profiles.RocketLeague.Layers
{
    /// <summary>
    /// Interaction logic for Control_RocketLeagueBackgroundLayer.xaml
    /// </summary>
    public partial class Control_RocketLeagueBackgroundLayer : UserControl
    {
        private bool settingsset = false;
        private bool profileset = false;

        public Control_RocketLeagueBackgroundLayer()
        {
            InitializeComponent();
        }

        public Control_RocketLeagueBackgroundLayer(RocketLeagueBackgroundLayerHandler datacontext)
        {
            InitializeComponent();

            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is RocketLeagueBackgroundLayerHandler && !settingsset)
            {
                this.ColorPicker_Blue.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as RocketLeagueBackgroundLayerHandler).Properties._BlueColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Orange.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as RocketLeagueBackgroundLayerHandler).Properties._OrangeColor ?? System.Drawing.Color.Empty);
                this.ColorPicker_Default.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((this.DataContext as RocketLeagueBackgroundLayerHandler).Properties._DefaultColor ?? System.Drawing.Color.Empty);
                this.Checkbox_ShowTeamScoreSplit.IsChecked = (this.DataContext as RocketLeagueBackgroundLayerHandler).Properties._ShowTeamScoreSplit ?? false;
                this.Checkbox_ShowGoalExplosion.IsChecked = ( this.DataContext as RocketLeagueBackgroundLayerHandler ).Properties._ShowGoalExplosion ?? false;
                this.Checkbox_ShowEnemyExplosion.IsChecked = (this.DataContext as RocketLeagueBackgroundLayerHandler).Properties._ShowGoalExplosion ?? false;

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

        private void ColorPicker_Blue_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is RocketLeagueBackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as RocketLeagueBackgroundLayerHandler).Properties._BlueColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Orange_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is RocketLeagueBackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as RocketLeagueBackgroundLayerHandler).Properties._OrangeColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void ColorPicker_Default_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (IsLoaded && settingsset && this.DataContext is RocketLeagueBackgroundLayerHandler && sender is Xceed.Wpf.Toolkit.ColorPicker && (sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.HasValue)
                (this.DataContext as RocketLeagueBackgroundLayerHandler).Properties._DefaultColor = Utils.ColorUtils.MediaColorToDrawingColor((sender as Xceed.Wpf.Toolkit.ColorPicker).SelectedColor.Value);
        }

        private void Checkbox_ShowTeamScoreSplit_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is RocketLeagueBackgroundLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
                (this.DataContext as RocketLeagueBackgroundLayerHandler).Properties._ShowTeamScoreSplit  = (sender as CheckBox).IsChecked.Value;
        }

        private void Checkbox_ShowGoalExplosion_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if ( IsLoaded && settingsset && this.DataContext is RocketLeagueBackgroundLayerHandler && sender is CheckBox && ( sender as CheckBox ).IsChecked.HasValue )
                ( this.DataContext as RocketLeagueBackgroundLayerHandler ).Properties._ShowGoalExplosion = ( sender as CheckBox ).IsChecked.Value;
        }

        private void Checkbox_ShowEnemyExplosion_enabled_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is RocketLeagueBackgroundLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
                (this.DataContext as RocketLeagueBackgroundLayerHandler).Properties._ShowEnemyExplosion = (sender as CheckBox).IsChecked.Value;
        }
    }
}
