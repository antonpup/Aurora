using Aurora.Settings;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace Aurora.Controls {

    public partial class Control_GradientEditor : UserControl {

        public Control_GradientEditor(LayerEffectConfig gradient) {
            InitializeComponent();
            animTypeCb.ItemsSource = Utils.EnumUtils.GetEnumItemsSource<AnimationType>();
            DataContext = gradient;
        }
    }
}
