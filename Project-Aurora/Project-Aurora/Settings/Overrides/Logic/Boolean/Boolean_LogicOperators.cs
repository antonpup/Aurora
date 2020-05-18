using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Aurora.Profiles;
using Aurora.Utils;
using Newtonsoft.Json;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Condition that checks a set of subconditions for atleast one of them being true.
    /// </summary>
    [Evaluatable("Or", category: EvaluatableCategory.Logic)]
    public class BooleanOr : Evaluatable<bool>, IHasSubConditons {

        /// <summary>Creates a new Or evaluatable with no subconditions.</summary>
        public BooleanOr() { }
        /// <summary>Creates a new Or evaluatable with the given subconditions.</summary>
        public BooleanOr(IEnumerable<Evaluatable<bool>> subconditions) {
            SubConditions = new ObservableCollection<Evaluatable<bool>>(subconditions);
        }

        [JsonProperty]
        public ObservableCollection<Evaluatable<bool>> SubConditions { get; set; } = new ObservableCollection<Evaluatable<bool>>();

        public override Visual GetControl() => new Control_SubconditionHolder(this, "Or");

        protected override bool Execute(IGameState gameState) => SubConditions.Any(subcondition => subcondition?.Evaluate(gameState) ?? false);
        
        public override Evaluatable<bool> Clone() => new BooleanOr { SubConditions = new ObservableCollection<Evaluatable<bool>>(SubConditions.Select(e => e.Clone())) };
    }


    /// <summary>
    /// Condition that checks a set of subconditions and requires them all to be true.
    /// </summary>
    [Evaluatable("And", category: EvaluatableCategory.Logic)]
    public class BooleanAnd : Evaluatable<bool>, IHasSubConditons {

        /// <summary>Creates a new And evaluatable with no subconditions.</summary>
        public BooleanAnd() { }
        /// <summary>Creates a new And evaluatable with the given subconditions.</summary>
        public BooleanAnd(IEnumerable<Evaluatable<bool>> subconditions) {
            SubConditions = new ObservableCollection<Evaluatable<bool>>(subconditions);
        }

        [JsonProperty]
        public ObservableCollection<Evaluatable<bool>> SubConditions { get; set; } = new ObservableCollection<Evaluatable<bool>>();

        public override Visual GetControl() => new Control_SubconditionHolder(this, "And");

        protected override bool Execute(IGameState gameState) => SubConditions.All(subcondition => subcondition?.Evaluate(gameState) ?? false);
        public override Evaluatable<bool> Clone() => new BooleanAnd { SubConditions = new ObservableCollection<Evaluatable<bool>>(SubConditions.Select(e => { var x = e.Clone(); return x; })) };
    }


    /// <summary>
    /// Condition that inverts another condition.
    /// </summary>
    [Evaluatable("Not", category: EvaluatableCategory.Logic)]
    public class BooleanNot : Evaluatable<bool> {

        /// <summary>Creates a new NOT evaluatable with the default BooleanTrue subcondition.</summary>
        public BooleanNot() { }
        /// <summary>Creates a new NOT evaluatable which inverts the given subcondition.</summary>
        public BooleanNot(Evaluatable<bool> subcondition) {
            SubCondition = subcondition;
        }

        [JsonProperty]
        public Evaluatable<bool> SubCondition { get; set; } = new BooleanConstant();

        public override Visual GetControl() => new StackPanel { Orientation = Orientation.Horizontal }
            .WithChild(new TextBlock { Text = "Not", FontWeight = FontWeights.Bold, Margin = new Thickness(2, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center })
            .WithChild(new Control_EvaluatablePresenter { EvalType = typeof(bool) }
                .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, new Binding(nameof(SubCondition)) { Source = this, Mode = BindingMode.TwoWay }));

        protected override bool Execute(IGameState gameState) => !SubCondition.Evaluate(gameState);

        public override Evaluatable<bool> Clone() => new BooleanNot { SubCondition = SubCondition.Clone() };
    }


    /// <summary>
    /// Condition that always returns true. Useful as a default condition as it means that
    /// the layer will always be visible.
    /// </summary>
    [Evaluatable("Boolean Constant", category: EvaluatableCategory.Logic)]
    public class BooleanConstant : Evaluatable<bool> {

        /// <summary>Creates a new constant true boolean.</summary>
        public BooleanConstant() { }
        /// <summary>Creates a new constant boolean with the given state.</summary>
        public BooleanConstant(bool state) { }

        /// <summary>The value held by this constant value.</summary>
        public bool State { get; set; }

        // Create a checkbox to use to set the constant value
        public override Visual GetControl() => new CheckBox { Content = "True/False", VerticalAlignment = System.Windows.VerticalAlignment.Center }
            .WithBinding(CheckBox.IsCheckedProperty, new Binding("State") { Source = this, Mode = BindingMode.TwoWay });

        // Simply return the current state
        protected override bool Execute(IGameState _) => State;
        // Creates a new BooleanConstant
        public override Evaluatable<bool> Clone() => new BooleanConstant { State = State };
    }


    /// <summary>
    /// Indicates that the implementing class has a SubCondition collection property.
    /// </summary>
    public interface IHasSubConditons {
        ObservableCollection<Evaluatable<bool>> SubConditions { get; set; }
    }
}
