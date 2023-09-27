using Aurora.Settings;
using System.Windows.Controls;

namespace Aurora.Controls {

    public partial class Control_GradientEditor : UserControl {

        public Control_GradientEditor(LayerEffectConfig gradient) {
            InitializeComponent();
            animTypeCb.ItemsSource = Utils.EnumUtils.GetEnumItemsSource<AnimationType>();
            DataContext = gradient;
        }
    }
}
