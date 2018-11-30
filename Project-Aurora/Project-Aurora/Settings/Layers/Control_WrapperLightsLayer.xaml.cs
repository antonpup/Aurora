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
    public partial class Control_WrapperLightsLayer : UserControl
    {
        private bool settingsset = false;

        public Control_WrapperLightsLayer()
        {
            InitializeComponent();
        }

        public Control_WrapperLightsLayer(WrapperLightsLayerHandler datacontext) : this()
        {
            this.DataContext = datacontext;
        }

        private WrapperLightsLayerHandler Context => DataContext as WrapperLightsLayerHandler;

        public void SetSettings()
        {
            if(this.DataContext is WrapperLightsLayerHandler && !settingsset)
            {
                CloneSourceKS.Sequence = new KeySequence(Context.Properties._CloningMap.Keys.ToArray());
                settingsset = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSettings();
            this.Loaded -= UserControl_Loaded;
        }

        private void ce_color_factor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
                this.ce_color_factor_label.Text = ((float)this.ce_color_factor.Value).ToString();
        }

        private void ce_color_hsv_sine_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
                this.ce_color_hsv_sine_label.Text = ((float)this.ce_color_hsv_sine.Value).ToString();
        }

        private void ce_color_hsv_gamma_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded)
                this.ce_color_hsv_gamma_label.Text = ((float)this.ce_color_hsv_gamma.Value).ToString();
        }

        private void CloneSourceKS_SequenceKeysChange(object sender, EventArgs e) {
            // If any items ARE in the clone map but NOT in the sequence, remove them
            // We dont need to worry about adding items to the clone map since CloneDestKS_SequenceUpdated takes care of that
            var toRemove = Context.Properties.CloningMap.Keys.Where(key => !CloneSourceKS.Sequence.keys.Contains(key)).ToList();
            foreach (var key in toRemove)
                Context.Properties.CloningMap.Remove(key);
        }

        private void CloneDestKS_SequenceKeysChange(object sender, EventArgs e) {
            // Adds or updates the sequnce in the cloning map at the source key
            if (CloneSourceKS.SelectedItems.Count() == 1)
                Context.Properties.CloningMap[CloneSourceKS.SelectedItems.First()] = CloneDestKS.Sequence;
        }

        private void CloneSourceKS_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            int selectedCount = CloneSourceKS.SelectedItems.Count();
            // The destination can only be edited for one source at a time
            CloneDestKS.IsEnabled = selectedCount == 1;

            // Set the destination sequence to be the sequence from the cloning map if a sequence for the source key exists
            CloneDestKS.Sequence = selectedCount == 1 && Context.Properties.CloningMap.ContainsKey(CloneSourceKS.SelectedItems.First())
                ? Context.Properties.CloningMap[CloneSourceKS.SelectedItems.First()]
                : null;
        }
    }
}
