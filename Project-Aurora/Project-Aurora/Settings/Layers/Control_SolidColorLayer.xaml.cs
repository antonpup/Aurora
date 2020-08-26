using System.Windows.Controls;

namespace Aurora.Settings.Layers {

    public partial class Control_SolidColorLayer : UserControl {

        public Control_SolidColorLayer() {
            InitializeComponent();
        }

        public Control_SolidColorLayer(SolidColorLayerHandler context) {
            InitializeComponent();
            DataContext = context;
        }
    }
}
