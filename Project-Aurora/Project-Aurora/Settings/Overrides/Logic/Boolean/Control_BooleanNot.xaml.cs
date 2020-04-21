using System;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic {
    /// <summary>
    /// Interaction logic for Control_ConditionNot.xaml
    /// </summary>
    public partial class Control_ConditionNot : UserControl {
        public Control_ConditionNot(BooleanNot context) {
            InitializeComponent();
            DataContext = context;
        }
    }
}
