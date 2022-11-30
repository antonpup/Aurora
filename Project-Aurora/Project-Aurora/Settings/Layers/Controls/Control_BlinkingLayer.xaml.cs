using System.Windows.Controls;

namespace Aurora.Settings.Layers.Controls {

    public partial class Control_BlinkingLayer : UserControl {

        public Control_BlinkingLayer() {
            InitializeComponent();
        }

        public Control_BlinkingLayer(BlinkingLayerHandler context) {
            InitializeComponent();
            DataContext = context;
        }
    }
}
