using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Aurora.Settings.Overrides.Logic {

    public partial class Control_EvaluatableToolbox : UserControl {

        public Control_EvaluatableToolbox() {
            InitializeComponent();
        }

        /// <summary>Returns a list of all detected evaluatables grouped by their category.</summary>
        public IEnumerable<IGrouping<OverrideLogicCategory, EvaluatableRegistry.EvaluatableTypeContainer>> GroupedEvalutables =>
            EvaluatableRegistry.Get().GroupBy(x => x.Metadata.Category);

        private void EvaluatableSpawnerItem_StartDrag(object sender, MouseEventArgs e, Point initial) {
            var dc = (EvaluatableRegistry.EvaluatableTypeContainer)((FrameworkElement)sender).DataContext;
            DragDrop.DoDragDrop(this, Activator.CreateInstance(dc.Evaluatable), DragDropEffects.Move);
        }
    }
}
