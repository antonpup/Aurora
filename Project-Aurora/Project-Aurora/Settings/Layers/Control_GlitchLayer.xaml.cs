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
    /// Interaction logic for Control_GlitchLayer.xaml
    /// </summary>
    public partial class Control_GlitchLayer : UserControl
    {
        private bool settingsset = false;

        public Control_GlitchLayer()
        {
            InitializeComponent();
        }

        public Control_GlitchLayer(GlitchLayerHandler datacontext) : this()
        {
            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            if (this.DataContext is GlitchLayerHandler && !settingsset)
            {
                this.updownUpdateInterval.Value = (this.DataContext as GlitchLayerHandler).Properties._UpdateInterval;
                this.KeySequence_keys.Sequence = (this.DataContext as GlitchLayerHandler).Properties._Sequence;
                this.ChkBoxAllowTransparency.IsChecked = (this.DataContext as GlitchLayerHandler).Properties._AllowTransparency ?? false;

                settingsset = true;
            }
        }

        internal void SetProfile(Profiles.Application profile)
        {
        }

        private void KeySequence_keys_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is GlitchLayerHandler && sender is Aurora.Controls.KeySequence)
                (this.DataContext as GlitchLayerHandler).Properties._Sequence = (sender as Aurora.Controls.KeySequence).Sequence;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();

            this.Loaded -= UserControl_Loaded;
        }

        private void updown_blink_value_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded && settingsset && this.DataContext is GlitchLayerHandler && sender is Xceed.Wpf.Toolkit.DoubleUpDown && (sender as Xceed.Wpf.Toolkit.DoubleUpDown).Value.HasValue)
                (this.DataContext as GlitchLayerHandler).Properties._UpdateInterval = (sender as Xceed.Wpf.Toolkit.DoubleUpDown).Value.Value;
        }

        private void ChkBoxAllowTransparency_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && settingsset && this.DataContext is GlitchLayerHandler && sender is CheckBox && (sender as CheckBox).IsChecked.HasValue)
                (this.DataContext as GlitchLayerHandler).Properties._AllowTransparency = (sender as CheckBox).IsChecked.Value;
        }
    }
}
