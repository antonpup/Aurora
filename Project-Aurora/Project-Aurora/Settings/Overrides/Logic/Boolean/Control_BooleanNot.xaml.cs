using System;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_ConditionNot.xaml
    /// </summary>
    public partial class Control_ConditionNot : UserControl {
        public Control_ConditionNot(BooleanNot context, Profiles.Application application) {
            InitializeComponent();
            ParentExpr = context;
            Application = application;
            ((FrameworkElement)Content).DataContext = this;
        }

        #region Properties/Dependency Properties
        /// <summary>The parent <see cref="BooleanNot"/> evaluatable of this control.</summary>
        public BooleanNot ParentExpr { get; }
        
        /// <summary>The application context of this control. Is passed to the EvaluatablePresenter children.</summary>
        public Profiles.Application Application {
            get => (Profiles.Application)GetValue(ApplicationProperty);
            set => SetValue(ApplicationProperty, value);
        }

        /// <summary>The property used as a backing store for the application context.</summary>
        public static readonly DependencyProperty ApplicationProperty =
            DependencyProperty.Register("Application", typeof(Profiles.Application), typeof(Control_ConditionNot), new PropertyMetadata(null));
        #endregion
    }

    /// <summary>
    /// The datatype that is used as the DataContext for `Control_ConditionNot`.
    /// </summary>
    internal class Control_ConditionNot_Context {
        public BooleanNot ParentCondition { get; set; }
        public Profiles.Application Application { get; set; }
    }
}
