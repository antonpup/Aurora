using System.Windows;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {

    public partial class Control_TimeAndUnit : UserControl {
        public Control_TimeAndUnit() {
            InitializeComponent();
            UnitCombo.ItemsSource = Utils.EnumUtils.GetEnumItemsSource<TimeUnit>();
            ((FrameworkElement)Content).DataContext = this;
        }

        #region Time Property
        public double Time {
            get => (double)GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(double), typeof(Control_TimeAndUnit), new PropertyMetadata(0d));
        #endregion

        #region Unit Property
        public TimeUnit Unit {
            get => (TimeUnit)GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(TimeUnit), typeof(Control_TimeAndUnit), new PropertyMetadata(TimeUnit.Seconds));
        #endregion
    }
}
