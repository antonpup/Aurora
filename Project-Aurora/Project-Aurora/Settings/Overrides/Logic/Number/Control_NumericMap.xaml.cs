using Aurora.Profiles;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {

    public partial class Control_NumericMap : UserControl {

        public Control_NumericMap(NumberMap context) {
            InitializeComponent();
            DataContext = context;
        }
    }
}
