using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Aurora.Profiles;
using Newtonsoft.Json;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Condition that checks a set of subconditions for atleast one of them being true.
    /// </summary>
    [OverrideLogic("Or", category: OverrideLogicCategory.Logic)]
    public class BooleanOr : IEvaluatable<bool>, IHasSubConditons {

        /// <summary>Creates a new Or evaluatable with no subconditions.</summary>
        public BooleanOr() { }
        /// <summary>Creates a new Or evaluatable with the given subconditions.</summary>
        public BooleanOr(IEnumerable<IEvaluatable<bool>> subconditions) {
            SubConditions = new ObservableCollection<IEvaluatable<bool>>(subconditions);
        }

        [JsonProperty]
        public ObservableCollection<IEvaluatable<bool>> SubConditions { get; set; } = new ObservableCollection<IEvaluatable<bool>>();

        [JsonIgnore]
        private Control_SubconditionHolder control;
        public Visual GetControl(Application app) => control ?? (control = new Control_SubconditionHolder(this, app, "Require atleast one of the following is true..."));

        public bool Evaluate(IGameState gameState) => SubConditions.Any(subcondition => subcondition?.Evaluate(gameState) ?? false);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public void SetApplication(Application application) {
            if (control != null)
                control.Application = application;
            foreach (var subcondition in SubConditions)
                subcondition.SetApplication(application);
        }

        public IEvaluatable<bool> Clone() => new BooleanOr { SubConditions = new ObservableCollection<IEvaluatable<bool>>(SubConditions.Select(e => e.Clone())) };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// Condition that checks a set of subconditions and requires them all to be true.
    /// </summary>
    [OverrideLogic("And", category: OverrideLogicCategory.Logic)]
    public class BooleanAnd : IEvaluatable<bool>, IHasSubConditons {

        /// <summary>Creates a new And evaluatable with no subconditions.</summary>
        public BooleanAnd() { }
        /// <summary>Creates a new And evaluatable with the given subconditions.</summary>
        public BooleanAnd(IEnumerable<IEvaluatable<bool>> subconditions) {
            SubConditions = new ObservableCollection<IEvaluatable<bool>>(subconditions);
        }

        [JsonProperty]
        public ObservableCollection<IEvaluatable<bool>> SubConditions { get; set; } = new ObservableCollection<IEvaluatable<bool>>();

        [JsonIgnore]
        private Control_SubconditionHolder control;
        public Visual GetControl(Application app) => control ?? (control = new Control_SubconditionHolder(this, app, "Require all of the following are true..."));

        public bool Evaluate(IGameState gameState) => SubConditions.All(subcondition => subcondition?.Evaluate(gameState) ?? false);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public void SetApplication(Application application) {
            if (control != null)
                control.Application = application;
            foreach (var subcondition in SubConditions)
                subcondition.SetApplication(application);
        }

        public IEvaluatable<bool> Clone() => new BooleanAnd { SubConditions = new ObservableCollection<IEvaluatable<bool>>(SubConditions.Select(e => { var x = e.Clone(); return x; })) };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }



    /// <summary>
    /// Condition that inverts another condition.
    /// </summary>
    [OverrideLogic("Not", category: OverrideLogicCategory.Logic)]
    public class BooleanNot : IEvaluatable<bool> {

        /// <summary>Creates a new NOT evaluatable with the default BooleanTrue subcondition.</summary>
        public BooleanNot() { }
        /// <summary>Creates a new NOT evaluatable which inverts the given subcondition.</summary>
        public BooleanNot(IEvaluatable<bool> subcondition) {
            SubCondition = subcondition;
        }

        [JsonProperty]
        public IEvaluatable<bool> SubCondition { get; set; } = new BooleanConstant();

        private Control_ConditionNot control;
        public Visual GetControl(Application app) => control ?? (control = new Control_ConditionNot(this, app));

        public bool Evaluate(IGameState gameState) => !SubCondition.Evaluate(gameState);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public void SetApplication(Application application) {
           // control.app
            SubCondition?.SetApplication(application);
        }

        public IEvaluatable<bool> Clone() => new BooleanNot { SubCondition = SubCondition.Clone() };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// Condition that always returns true. Useful as a default condition as it means that
    /// the layer will always be visible.
    /// </summary>
    [OverrideLogic("Boolean Constant", category: OverrideLogicCategory.Logic)]
    public class BooleanConstant : IEvaluatable<bool> {

        /// <summary>Creates a new constant true boolean.</summary>
        public BooleanConstant() { }
        /// <summary>Creates a new constant boolean with the given state.</summary>
        public BooleanConstant(bool state) { }

        /// <summary>The value held by this constant value.</summary>
        public bool State { get; set; }

        // Create a checkbox to use to set the constant value
        [JsonIgnore]
        private CheckBox control;
        public Visual GetControl(Application application) {
            if (control == null) {
                control = new CheckBox { Content = "True/False" };
                control.SetBinding(CheckBox.IsCheckedProperty, new Binding("State") { Source = this, Mode = BindingMode.TwoWay });
            }
            return control;
        }

        // Simply return the current state
        public bool Evaluate(IGameState _) => State;
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        // Application-independent evaluatable, do nothing.
        public void SetApplication(Application application) { }

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
