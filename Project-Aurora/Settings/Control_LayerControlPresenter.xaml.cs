using Aurora.Profiles.Dota_2.Layers;
using Aurora.Profiles.CSGO.Layers;
using Aurora.Profiles.GTA5.Layers;
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

namespace Aurora.Settings
{
    /// <summary>
    /// Interaction logic for Control_LayerControlPresenter.xaml
    /// </summary>
    public partial class Control_LayerControlPresenter : UserControl
    {
        private bool isSettingNewLayer = false;

        protected Layer _Layer;

        public Layer Layer { get { return _Layer; } set { _Layer = value; SetLayer(value); } }

        public Control_LayerControlPresenter()
        {
            InitializeComponent();
        }

        public Control_LayerControlPresenter(Layer layer) : this()
        {
            Layer = layer;
            cmbLayerType.SelectedItem = Layer.Handler.Type;
        }

        private void SetLayer(Layer layer)
        {
            isSettingNewLayer = true;

            DataContext = layer;

            cmbLayerType.Items.Clear();

            foreach(var layertype in layer.AssociatedProfile.AvailableLayers)
                cmbLayerType.Items.Add(layertype);

            cmbLayerType.SelectedItem = Layer.Handler.Type;
            ctrlLayerTypeConfig.Content = layer.Control;
            isSettingNewLayer = false;
        }

        private void cmbLayerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && !isSettingNewLayer && sender is ComboBox)
            {
                LayerType enumVal = (LayerType)Enum.Parse(typeof(LayerType), ((sender as ComboBox).SelectedItem).ToString());

                switch (enumVal)
                {
                    case LayerType.Solid:
                        _Layer.Handler = new SolidColorLayerHandler();
                        break;
                    case LayerType.SolidFilled:
                        _Layer.Handler = new SolidFillLayerHandler();
                        break;
                    case LayerType.Percent:
                        _Layer.Handler = new PercentLayerHandler();
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
                    case LayerType.LockColor:
                        _Layer.Handler = new LockColourLayerHandler();
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
                    case LayerType.Dota2HeroAbiltiyEffects:
                        _Layer.Handler = new Dota2HeroAbiltiyEffectsLayerHandler();
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
                    default:
                        _Layer.Handler = new DefaultLayerHandler();
                        break;
                }

                ctrlLayerTypeConfig.Content = _Layer.Control;
                this._Layer.AssociatedProfile.SaveProfiles();
            }
        }
    }
}
