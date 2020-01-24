using System.Windows.Controls;

namespace Aurora.Settings.Layers {

    public partial class Control_RadialLayer : UserControl {

        public Control_RadialLayer(RadialLayerHandler context) {
            DataContext = context;
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            if (DataContext is RadialLayerHandler context) {
                Sequence.Sequence = context.Properties._Sequence;
                Loaded -= UserControl_Loaded;
            }
        }

        private void Sequence_SequenceUpdated(object sender, System.EventArgs e) {
            ((RadialLayerHandler)DataContext).Properties._Sequence = Sequence.Sequence;
        }
    }
}
