using System.Windows.Controls;

namespace Aurora.Settings.Layers {

    public partial class Control_ToolbarLayer : UserControl {

        public Control_ToolbarLayer(ToolbarLayerHandler context) {
            InitializeComponent();
            DataContext = context;
        }
    }
}
