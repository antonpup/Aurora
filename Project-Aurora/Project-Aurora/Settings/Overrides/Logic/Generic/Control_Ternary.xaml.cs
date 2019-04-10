using System;
using System.Linq;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_ConditionNot.xaml
    /// </summary>
    public partial class Control_Ternary : UserControl {
        public Control_Ternary() {
            InitializeComponent();
        }
    }

    public partial class Control_Ternary<T> : Control_Ternary
    {
        public Control_Ternary(TernaryGeneric<T> context, Profiles.Application application) : base()
        {
            DataContext = new Control_Ternary_Context<T> { Application = application, ParentCondition = context, EvaluatableType = EvaluatableTypeResolver.GetEvaluatableType(typeof(IEvaluatable<T>)) };
        }
    }

    /// <summary>
    /// The datatype that is used as the DataContext for `Control_ConditionNot`.
    /// </summary>
    internal class Control_Ternary_Context<T> {
        public TernaryGeneric<T> ParentCondition { get; set; }
        public Profiles.Application Application { get; set; }
        public EvaluatableType EvaluatableType { get; set; }
    }
}
