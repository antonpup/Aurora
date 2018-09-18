using Aurora.Controls;
using Aurora.Utils;
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
using Xceed.Wpf.Toolkit;

namespace Aurora.Profiles.Minecraft.Layers {
    /// <summary>
    /// Interaction logic for Control_MinecraftHealthBarLayer.xaml
    /// </summary>
    public partial class Control_MinecraftHealthBarLayer : UserControl {

        private bool settingSet = false;

        public Control_MinecraftHealthBarLayer(MinecraftHealthBarLayerHandler context) {
            InitializeComponent();
            DataContext = context;
        }

        private MinecraftHealthBarLayerHandler Context => (MinecraftHealthBarLayerHandler)DataContext;
        private bool CanSet => IsLoaded && settingSet && DataContext is MinecraftHealthBarLayerHandler;

        private void SetSettings() {
            if (DataContext is MinecraftHealthBarLayerHandler && !settingSet) {
                NormalHealth_ColorPicker.SelectedColor = ColorUtils.DrawingColorToMediaColor(Context.Properties._NormalHealthColor ?? System.Drawing.Color.Empty);

                AbsorptionHealth_Enabled.IsChecked = AbsorptionHealth_ColorPicker.IsEnabled = Context.Properties._EnableAbsorptionHealthColor ?? true;
                AbsorptionHealth_ColorPicker.SelectedColor = ColorUtils.DrawingColorToMediaColor(Context.Properties._AbsorptionHealthColor ?? System.Drawing.Color.Empty);

                RegenerationHealth_Enabled.IsChecked = RegenerationHealth_ColorPicker.IsEnabled = Context.Properties._EnableRegenerationHealthColor ?? true;
                RegenerationHealth_ColorPicker.SelectedColor = ColorUtils.DrawingColorToMediaColor(Context.Properties._RegenerationHealthColor ?? System.Drawing.Color.Empty);

                PoisonHealth_Enabled.IsChecked = PoisonHealth_ColorPicker.IsEnabled = Context.Properties._EnablePoisonHealthColor ?? true;
                PoisonHealth_ColorPicker.SelectedColor = ColorUtils.DrawingColorToMediaColor(Context.Properties._PoisonHealthColor ?? System.Drawing.Color.Empty);

                WitherHealth_Enabled.IsChecked = WitherHealth_ColorPicker.IsEnabled = Context.Properties._EnableWitherHealthColor ?? true;
                WitherHealth_ColorPicker.SelectedColor = ColorUtils.DrawingColorToMediaColor(Context.Properties._WitherHealthColor ?? System.Drawing.Color.Empty);

                Background_ColorPicker.SelectedColor = ColorUtils.DrawingColorToMediaColor(Context.Properties._BackgroundColor ?? System.Drawing.Color.Empty);
                ProgressGradualCh.IsChecked = Context.Properties._GradualProgress;
                KeySequence_Keys.Sequence = Context.Properties._Sequence;

                settingSet = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            SetSettings();
            Loaded -= UserControl_Loaded;
        }

        private void NormalHealth_ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (CanSet && e.NewValue.HasValue)
                Context.Properties._NormalHealthColor = ColorUtils.MediaColorToDrawingColor(e.NewValue.Value);
        }

        private void AbsorptionHealth_Enabled_Checked(object sender, RoutedEventArgs e) {
            if (CanSet && (sender as CheckBox).IsChecked.HasValue)
                Context.Properties._EnableAbsorptionHealthColor = AbsorptionHealth_ColorPicker.IsEnabled = (sender as CheckBox).IsChecked.Value;
        }
        private void AbsorptionHealth_ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (CanSet && e.NewValue.HasValue)
                Context.Properties._AbsorptionHealthColor = ColorUtils.MediaColorToDrawingColor(e.NewValue.Value);
        }

        private void RegenerationHealth_Enabled_Checked(object sender, RoutedEventArgs e) {
            if (CanSet && (sender as CheckBox).IsChecked.HasValue)
                Context.Properties._EnableRegenerationHealthColor = RegenerationHealth_ColorPicker.IsEnabled = (sender as CheckBox).IsChecked.Value;
        }
        private void RegenerationHealth_ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (CanSet && e.NewValue.HasValue)
                Context.Properties._RegenerationHealthColor = ColorUtils.MediaColorToDrawingColor(e.NewValue.Value);
        }

        private void PoisonHealth_Enabled_Checked(object sender, RoutedEventArgs e) {
            if (CanSet && (sender as CheckBox).IsChecked.HasValue)
                Context.Properties._EnablePoisonHealthColor = PoisonHealth_ColorPicker.IsEnabled = (sender as CheckBox).IsChecked.Value;
        }
        private void PoisonHealth_ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (CanSet && e.NewValue.HasValue)
                Context.Properties._PoisonHealthColor = ColorUtils.MediaColorToDrawingColor(e.NewValue.Value);
        }

        private void WitherHealth_Enabled_Checked(object sender, RoutedEventArgs e) {
            if (CanSet && (sender as CheckBox).IsChecked.HasValue)
                Context.Properties._EnableWitherHealthColor = WitherHealth_ColorPicker.IsEnabled = (sender as CheckBox).IsChecked.Value;
        }
        private void WitherHealth_ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (CanSet && e.NewValue.HasValue)
                Context.Properties._WitherHealthColor = ColorUtils.MediaColorToDrawingColor(e.NewValue.Value);
        }

        private void Background_ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (CanSet && e.NewValue.HasValue)
                Context.Properties._BackgroundColor = ColorUtils.MediaColorToDrawingColor(e.NewValue.Value);
        }

        private void ProgressGradualCh_Checked(object sender, RoutedEventArgs e) {
            if (CanSet && (sender as CheckBox).IsChecked.HasValue)
                Context.Properties._GradualProgress = (sender as CheckBox).IsChecked.Value;
        }

        private void KeySequence_Keys_SequenceUpdated(object sender, EventArgs e) {
            if (CanSet)
                Context.Properties._Sequence = (sender as KeySequence).Sequence;
        }
    }
}
