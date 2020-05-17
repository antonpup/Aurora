using System.Linq;
using System.Windows.Controls;

namespace Aurora.Settings.Layers {

    public partial class Control_BinaryCounterLayer : UserControl {

        public Control_BinaryCounterLayer(BinaryCounterLayerHandler context) {
            InitializeComponent();
            SetApplication(context.Application);
            DataContext = context;
            sequence.Sequence = context.Properties._Sequence;
        }

        public void SetApplication(Profiles.Application app) {
            varPathPicker.Application = app;
        }

        private void KeySequence_SequenceUpdated(object sender, System.EventArgs e) {
            // Apparently the Controls.KeySequnce wasn't made with DependencyProperties properly as it doesn't work with bindings :(
            ((BinaryCounterLayerHandler)DataContext).Properties._Sequence = sequence.Sequence;
        }
    }
}
