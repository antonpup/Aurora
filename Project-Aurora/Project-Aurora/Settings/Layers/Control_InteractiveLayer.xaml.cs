using System.Windows.Controls;

namespace Aurora.Settings.Layers {

    public partial class Control_InteractiveLayer : UserControl {

        public Control_InteractiveLayer(InteractiveLayerHandler context) {
            InitializeComponent();
            DataContext = context;
        }
    }
}
