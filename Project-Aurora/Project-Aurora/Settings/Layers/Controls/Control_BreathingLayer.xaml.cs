using System.Windows.Controls;

namespace Aurora.Settings.Layers.Controls {

    public partial class Control_BreathingLayer : UserControl {

        public Control_BreathingLayer() {
            InitializeComponent();
        }

        public Control_BreathingLayer(BreathingLayerHandler context) {
            InitializeComponent();
            DataContext = context;
        }
    }
}
