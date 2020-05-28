using System;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides.Logic.Generic {

    public partial class Control_Delay : UserControl {
        public Control_Delay() {
            InitializeComponent();
        }
    }

    public class Control_Delay<T> : Control_Delay {
        public Control_Delay(DelayGeneric<T> context) : base() {
            DataContext = context;
        }

        public Type EvalType => typeof(T);
    }
}
