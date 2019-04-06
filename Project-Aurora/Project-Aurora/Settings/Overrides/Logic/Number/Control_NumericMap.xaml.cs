using Aurora.Profiles;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {

    public partial class Control_NumericMap : UserControl {

        public NumberMap Map { get; set; }
        public Application Application { get; set; }

        public Control_NumericMap(NumberMap context, Application application) {
            InitializeComponent();
            Map = context;
            Application = application;
            DataContext = this;
        }
    }
}
