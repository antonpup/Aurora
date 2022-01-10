using System.Linq;
using System.Windows.Controls;

namespace Aurora.Settings.Layers {

    public partial class Control_BinaryCounterLayer : UserControl {

        public Control_BinaryCounterLayer(BinaryCounterLayerHandler context) {
            InitializeComponent();
            SetApplication(context.Application);
            DataContext = context;
        }

        public void SetApplication(Profiles.Application app) {
            varPathPicker.Application = app;
        }
    }
}
