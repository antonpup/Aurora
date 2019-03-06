using Aurora.Profiles.Dota_2.Layers;
using Aurora.Profiles.CSGO.Layers;
using Aurora.Profiles.GTA5.Layers;
using Aurora.Profiles.RocketLeague.Layers;
using Aurora.Settings.Layers;
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
using Aurora.Profiles.Payday_2.Layers;
using Aurora.Profiles;

namespace Aurora.Settings
{
    /// <summary>
    /// Interaction logic for Control_LayerControlPresenter.xaml
    /// </summary>
    public partial class Control_LayerControlPresenter : UserControl
    {
        private bool isSettingNewLayer = false;

        protected Layers.Layer _Layer;

        public Layers.Layer Layer { get { return _Layer; } set { _Layer = value; SetLayer(value); } }

        public Control_LayerControlPresenter()
        {
            InitializeComponent();
        }

        public Control_LayerControlPresenter(Layers.Layer layer) : this()
        {
            Layer = layer;
            cmbLayerType.SelectedItem = Layer.Handler.ID;
            grdLayerConfigs.Visibility = Visibility.Hidden;
            grd_LayerControl.IsHitTestVisible = true;
            grd_LayerControl.Effect = null;
        }

        private void SetLayer(Layers.Layer layer)
        {
            isSettingNewLayer = true;

            DataContext = layer;

            cmbLayerType.Items.Clear();

            foreach(var layertype in Global.LightingStateManager.DefaultLayerHandlers.Concat(layer.AssociatedApplication.Config.ExtraAvailableLayers))
                cmbLayerType.Items.Add(Global.LightingStateManager.LayerHandlers[layertype]);

            cmbLayerType.SelectedItem = Global.LightingStateManager.LayerHandlers[Layer.Handler.ID];
            ctrlLayerTypeConfig.Content = layer.Control;
            chkLayerSmoothing.IsChecked = Layer.Handler.EnableSmoothing;
            chk_ExcludeMask.IsChecked = Layer.Handler.EnableExclusionMask;
            keyseq_ExcludeMask.Sequence = Layer.Handler.ExclusionMask;
            sldr_Opacity.Value = (int)(Layer.Handler._Opacity ?? 1f * 100.0f);
            lbl_Opacity_Text.Text = $"{(int)sldr_Opacity.Value} %";

            grdLayerConfigs.Visibility = Visibility.Hidden;
            overridesEditor.Visibility = Visibility.Hidden;
            btnConfig.Visibility = Visibility.Visible;
            btnOverrides.Visibility = Visibility.Visible;
            grd_LayerControl.IsHitTestVisible = true;
            grd_LayerControl.Effect = null;
            isSettingNewLayer = false;

            overridesEditor.Layer = layer;
        }

        private void cmbLayerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && !isSettingNewLayer && sender is ComboBox)
            {
                LayerHandlerEntry enumVal = (LayerHandlerEntry)((sender as ComboBox).SelectedItem);

                ResetLayer(enumVal);
            }
        }

        private void ResetLayer(LayerHandlerEntry type)
        {
            if (IsLoaded && !isSettingNewLayer)
            {
                _Layer.Handler = Global.LightingStateManager.GetLayerHandlerInstance(type);

                ctrlLayerTypeConfig.Content = _Layer.Control;
                chkLayerSmoothing.IsChecked = _Layer.Handler.EnableSmoothing;
                chk_ExcludeMask.IsChecked = Layer.Handler.EnableExclusionMask;
                keyseq_ExcludeMask.Sequence = Layer.Handler.ExclusionMask;
                sldr_Opacity.Value = (int)(Layer.Handler.Opacity * 100.0f);
                lbl_Opacity_Text.Text = $"{(int)sldr_Opacity.Value} %";
                this._Layer.AssociatedApplication.SaveProfiles();

                overridesEditor.ForcePropertyListUpdate();
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && !isSettingNewLayer && sender is Button)
            {
                LayerHandlerEntry enumVal = (LayerHandlerEntry)(cmbLayerType.SelectedItem);

                ResetLayer(enumVal);
            }
        }

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && !isSettingNewLayer && sender is Button)
            {
                bool v = grdLayerConfigs.IsVisible;
                grdLayerConfigs.Visibility = v ? Visibility.Hidden : Visibility.Visible;
                grd_LayerControl.IsHitTestVisible = v;
                grd_LayerControl.Effect = v ? null : new System.Windows.Media.Effects.BlurEffect();
                btnOverrides.Visibility = v ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void chkLayerSmoothing_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && !isSettingNewLayer && sender is CheckBox)
                Layer.Handler.EnableSmoothing = (sender as CheckBox).IsChecked.Value;
        }

        private void chk_ExcludeMask_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && !isSettingNewLayer && sender is CheckBox)
                Layer.Handler.EnableExclusionMask = (sender as CheckBox).IsChecked.Value;

            keyseq_ExcludeMask.IsEnabled = Layer.Handler.EnableExclusionMask;
        }

        private void keyseq_ExcludeMask_SequenceUpdated(object sender, EventArgs e)
        {
            if (IsLoaded && !isSettingNewLayer && sender is Aurora.Controls.KeySequence)
                Layer.Handler.ExclusionMask = (sender as Aurora.Controls.KeySequence).Sequence;
        }

        private void sldr_Opacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded && !isSettingNewLayer && sender is Slider)
            {
                Layer.Handler._Opacity = (float)((sender as Slider).Value) / 100.0f;
                this.lbl_Opacity_Text.Text = $"{(int)((sender as Slider).Value)} %";
            }
        }

        private void btnOverrides_Click(object sender, RoutedEventArgs e) {
            if (IsLoaded && !isSettingNewLayer) {
                bool v = overridesEditor.IsVisible;
                overridesEditor.Visibility = v ? Visibility.Hidden : Visibility.Visible;
                grd_LayerControl.IsHitTestVisible = v;
                btnConfig.Visibility = v ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
