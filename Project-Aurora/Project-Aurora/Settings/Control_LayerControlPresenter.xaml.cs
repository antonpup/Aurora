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
            sldr_Opacity.Value = (int)(Layer.Handler.Opacity * 100.0f);
            lbl_Opacity_Text.Text = $"{(int)sldr_Opacity.Value} %";

            grdLayerConfigs.Visibility = Visibility.Hidden;
            grd_LayerControl.IsHitTestVisible = true;
            grd_LayerControl.Effect = null;
            isSettingNewLayer = false;
        }

        private void cmbLayerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && !isSettingNewLayer && sender is ComboBox)
            {
                LayerHandlerEntry enumVal = (LayerHandlerEntry)((sender as ComboBox).SelectedItem);

                ResetLayer(enumVal);
            }
        }

        private void btnLogic_Click(object sender, RoutedEventArgs e)
        {
            Window_LayerLogicEditor logic_edit = new Window_LayerLogicEditor(this._Layer);
            logic_edit.ShowDialog();
        }

        private void ResetLayer(LayerHandlerEntry type)
        {
            if (IsLoaded && !isSettingNewLayer)
            {
                _Layer.Handler = Global.LightingStateManager.GetLayerHandlerInstance(type);
                /*switch (type)
                {
                    case LayerType.Solid:
                        _Layer.Handler = new SolidColorLayerHandler();
                        break;
                    case LayerType.SolidFilled:
                        _Layer.Handler = new SolidFillLayerHandler();
                        break;
                    case LayerType.Gradient:
                        _Layer.Handler = new GradientLayerHandler();
                        break;
                    case LayerType.GradientFill:
                        _Layer.Handler = new GradientFillLayerHandler();
                        break;
                    case LayerType.Percent:
                        _Layer.Handler = new PercentLayerHandler();
                        break;
                    case LayerType.PercentGradient:
                        _Layer.Handler = new PercentGradientLayerHandler();
                        break;
                    case LayerType.Interactive:
                        _Layer.Handler = new InteractiveLayerHandler();
                        break;
                    case LayerType.ShortcutAssistant:
                        _Layer.Handler = new ShortcutAssistantLayerHandler();
                        break;
                    case LayerType.Equalizer:
                        _Layer.Handler = new EqualizerLayerHandler();
                        break;
                    case LayerType.Ambilight:
                        _Layer.Handler = new AmbilightLayerHandler();
                        break;
                    case LayerType.Breathing:
                        _Layer.Handler = new BreathingLayerHandler();
                        break;
                    case LayerType.Blinking:
                        _Layer.Handler = new BlinkingLayerHandler();
                        break;
                    case LayerType.Image:
                        _Layer.Handler = new ImageLayerHandler();
                        break;
                    case LayerType.Script:
                        _Layer.Handler = new ScriptLayerHandler();
                        break;
                    case LayerType.LockColor:
                        _Layer.Handler = new LockColourLayerHandler();
                        break;
                    case LayerType.Animation:
                        _Layer.Handler = new AnimationLayerHandler();
                        break;
                    case LayerType.Dota2Background:
                        _Layer.Handler = new Dota2BackgroundLayerHandler();
                        break;
                    case LayerType.Dota2Respawn:
                        _Layer.Handler = new Dota2RespawnLayerHandler();
                        break;
                    case LayerType.Dota2Abilities:
                        _Layer.Handler = new Dota2AbilityLayerHandler();
                        break;
                    case LayerType.Dota2Items:
                        _Layer.Handler = new Dota2ItemLayerHandler();
                        break;
                    case LayerType.Dota2HeroAbilityEffects:
                        _Layer.Handler = new Dota2HeroAbilityEffectsLayerHandler();
                        break;
                    case LayerType.Dota2Killstreak:
                        _Layer.Handler = new Dota2KillstreakLayerHandler();
                        break;
                    case LayerType.CSGOBackground:
                        _Layer.Handler = new CSGOBackgroundLayerHandler();
                        break;
                    case LayerType.CSGOBomb:
                        _Layer.Handler = new CSGOBombLayerHandler();
                        break;
                    case LayerType.CSGOKillsIndicator:
                        _Layer.Handler = new CSGOKillIndicatorLayerHandler();
                        break;
                    case LayerType.CSGOBurning:
                        _Layer.Handler = new CSGOBurningLayerHandler();
                        break;
                    case LayerType.CSGOFlashbang:
                        _Layer.Handler = new CSGOFlashbangLayerHandler();
                        break;
                    case LayerType.CSGOTyping:
                        _Layer.Handler = new CSGOTypingIndicatorLayerHandler();
                        break;
                    case LayerType.GTA5Background:
                        _Layer.Handler = new GTA5BackgroundLayerHandler();
                        break;
                    case LayerType.GTA5PoliceSiren:
                        _Layer.Handler = new GTA5PoliceSirenLayerHandler();
                        break;
                    case LayerType.RocketLeagueBackground:
                        _Layer.Handler = new RocketLeagueBackgroundLayerHandler();
                        break;
                    case LayerType.PD2Background:
                        _Layer.Handler = new PD2BackgroundLayerHandler();
                        break;
                    case LayerType.PD2Flashbang:
                        _Layer.Handler = new PD2FlashbangLayerHandler();
                        break;
                    case LayerType.PD2States:
                        _Layer.Handler = new PD2StatesLayerHandler();
                        break;
                    default:
                        _Layer.Handler = new DefaultLayerHandler();
                        break;
                }*/

                ctrlLayerTypeConfig.Content = _Layer.Control;
                chkLayerSmoothing.IsChecked = _Layer.Handler.EnableSmoothing;
                chk_ExcludeMask.IsChecked = Layer.Handler.EnableExclusionMask;
                keyseq_ExcludeMask.Sequence = Layer.Handler.ExclusionMask;
                sldr_Opacity.Value = (int)(Layer.Handler.Opacity * 100.0f);
                lbl_Opacity_Text.Text = $"{(int)sldr_Opacity.Value} %";
                this._Layer.AssociatedApplication.SaveProfiles();
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
                if(this.grdLayerConfigs.IsVisible)
                {
                    this.grdLayerConfigs.Visibility = Visibility.Hidden;
                    grd_LayerControl.IsHitTestVisible = true;
                    grd_LayerControl.Effect = null;
                }
                else
                {
                    this.grdLayerConfigs.Visibility = Visibility.Visible;
                    grd_LayerControl.IsHitTestVisible = false;
                    grd_LayerControl.Effect = new System.Windows.Media.Effects.BlurEffect();
                }
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
                Layer.Handler.Opacity = (float)((sender as Slider).Value) / 100.0f;
                this.lbl_Opacity_Text.Text = $"{(int)((sender as Slider).Value)} %";
            }
        }
    }
}
