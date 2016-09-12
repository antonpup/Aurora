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

        public Control_LayerControlPresenter(Layer layer)
        {
            InitializeComponent();

            Layer = layer;
            cmbLayerType.SelectedItem = Layer.Handler.Type;
        }

        private void SetLayer(Layer layer)
        {
            isSettingNewLayer = true;

            DataContext = layer;

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
                    case LayerType.Percent:
                        _Layer.Handler = new PercentLayerHandler();
                        break;
                    case LayerType.Interactive:
                        _Layer.Handler = new InteractiveLayerHandler();
                        break;
                    default:
                        _Layer.Handler = new DefaultLayerHandler();
                        break;
                }

                ctrlLayerTypeConfig.Content = _Layer.Control;
            }
        }
    }
}
