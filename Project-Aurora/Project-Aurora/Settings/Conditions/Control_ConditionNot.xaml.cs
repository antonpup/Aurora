using System.Windows.Controls;

namespace Aurora.Settings.Conditions {
    /// <summary>
    /// Interaction logic for Control_ConditionNot.xaml
    /// </summary>
    public partial class Control_ConditionNot : UserControl {
        public Control_ConditionNot(ConditionNot context, Profiles.Application application) {
            InitializeComponent();
            DataContext = new Control_ConditionNot_Context { Application = application, ParentCondition = context };
        }

        private void ConditionPresenter_ConditionChanged(object sender, ConditionChangeEventArgs e) {
            ((Control_ConditionNot_Context)DataContext).ParentCondition.SubCondition = e.NewCondition;
        }
    }

    /// <summary>
    /// The datatype that is used as the DataContext for `Control_ConditionNot`.
    /// </summary>
    internal class Control_ConditionNot_Context {
        public ConditionNot ParentCondition { get; set; }
        public Profiles.Application Application { get; set; }
    }
}
