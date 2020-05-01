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
    public class BooleanOr : IEvaluatable<bool>, IHasSubConditons {

        /// <summary>Creates a new Or evaluatable with no subconditions.</summary>
        public BooleanOr() { }
        /// <summary>Creates a new Or evaluatable with the given subconditions.</summary>
        public BooleanOr(IEnumerable<IEvaluatable<bool>> subconditions) {
            SubConditions = new ObservableCollection<IEvaluatable<bool>>(subconditions);
        }

        [JsonProperty]
        public ObservableCollection<IEvaluatable<bool>> SubConditions { get; set; } = new ObservableCollection<IEvaluatable<bool>>();

        public Visual GetControl() => new Control_SubconditionHolder(this, "Or");

        public bool Evaluate(IGameState gameState) => SubConditions.Any(subcondition => subcondition?.Evaluate(gameState) ?? false);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public IEvaluatable<bool> Clone() => new BooleanOr { SubConditions = new ObservableCollection<IEvaluatable<bool>>(SubConditions.Select(e => e.Clone())) };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// Condition that checks a set of subconditions and requires them all to be true.
    /// </summary>
    [Evaluatable("And", category: EvaluatableCategory.Logic)]
    public class BooleanAnd : IEvaluatable<bool>, IHasSubConditons {

        /// <summary>Creates a new And evaluatable with no subconditions.</summary>
        public BooleanAnd() { }
        /// <summary>Creates a new And evaluatable with the given subconditions.</summary>
        public BooleanAnd(IEnumerable<IEvaluatable<bool>> subconditions) {
            SubConditions = new ObservableCollection<IEvaluatable<bool>>(subconditions);
        }

        [JsonProperty]
        public ObservableCollection<IEvaluatable<bool>> SubConditions { get; set; } = new ObservableCollection<IEvaluatable<bool>>();

        public Visual GetControl() => new Control_SubconditionHolder(this, "And");

        public bool Evaluate(IGameState gameState) => SubConditions.All(subcondition => subcondition?.Evaluate(gameState) ?? false);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public IEvaluatable<bool> Clone() => new BooleanAnd { SubConditions = new ObservableCollection<IEvaluatable<bool>>(SubConditions.Select(e => { var x = e.Clone(); return x; })) };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }



    /// <summary>
    /// Condition that inverts another condition.
    /// </summary>
    [Evaluatable("Not", category: EvaluatableCategory.Logic)]
    public class BooleanNot : IEvaluatable<bool> {

        /// <summary>Creates a new NOT evaluatable with the default BooleanTrue subcondition.</summary>
        public BooleanNot() { }
        /// <summary>Creates a new NOT evaluatable which inverts the given subcondition.</summary>
        public BooleanNot(IEvaluatable<bool> subcondition) {
            SubCondition = subcondition;
        }

        [JsonProperty]
        public IEvaluatable<bool> SubCondition { get; set; } = new BooleanConstant();

        public Visual GetControl() => new StackPanel { Orientation = Orientation.Horizontal }
            .WithChild(new TextBlock { Text = "Not", FontWeight = FontWeights.Bold, Margin = new Thickness(2, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center })
            .WithChild(new Control_EvaluatablePresenter { EvalType = typeof(bool) }
                .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, new Binding(nameof(SubCondition)) { Source = this, Mode = BindingMode.TwoWay }));

        public bool Evaluate(IGameState gameState) => !SubCondition.Evaluate(gameState);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public IEvaluatable<bool> Clone() => new BooleanNot { SubCondition = SubCondition.Clone() };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// Condition that always returns true. Useful as a default condition as it means that
    /// the layer will always be visible.
    /// </summary>
    [Evaluatable("Boolean Constant", category: EvaluatableCategory.Logic)]
    public class BooleanConstant : IEvaluatable<bool> {

        /// <summary>Creates a new constant true boolean.</summary>
        public BooleanConstant() { }
        /// <summary>Creates a new constant boolean with the given state.</summary>
        public BooleanConstant(bool state) { }

        /// <summary>The value held by this constant value.</summary>
        public bool State { get; set; }

        // Create a checkbox to use to set the constant value
        public Visual GetControl() => new CheckBox { Content = "True/False" }
            .WithBinding(CheckBox.IsCheckedProperty, new Binding("State") { Source = this, Mode = BindingMode.TwoWay });

        // Simply return the current state
        public bool Evaluate(IGameState _) => State;
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        // Creates a new BooleanConstant
        public IEvaluatable<bool> Clone() => new BooleanConstant { State = State };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// Indicates that the implementing class has a SubCondition collection property.
    /// </summary>
    public interface IHasSubConditons {
        ObservableCollection<IEvaluatable<bool>> SubConditions { get; set; }
    }
}
