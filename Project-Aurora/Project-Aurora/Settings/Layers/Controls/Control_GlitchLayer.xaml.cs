using System.Windows.Controls;

namespace Aurora.Settings.Layers.Controls {

    public partial class Control_GlitchLayer : UserControl {

        public Control_GlitchLayer() {
            InitializeComponent();
        }

        public Control_GlitchLayer(GlitchLayerHandler context) : this() {
            DataContext = context;
        }
    }
}
