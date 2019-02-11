using System;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_ConditionNot.xaml
    /// </summary>
    public partial class Control_ConditionNot : UserControl {
        public Control_ConditionNot(BooleanNot context, Profiles.Application application) {
            InitializeComponent();
            DataContext = new Control_ConditionNot_Context { Application = application, ParentCondition = context };
        }
    }

    /// <summary>
    /// The datatype that is used as the DataContext for `Control_ConditionNot`.
    /// </summary>
    internal class Control_ConditionNot_Context {
        public BooleanNot ParentCondition { get; set; }
        public Profiles.Application Application { get; set; }
    }
}
