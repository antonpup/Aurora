using System.Windows.Controls;

namespace Aurora.Settings.Layers {

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
