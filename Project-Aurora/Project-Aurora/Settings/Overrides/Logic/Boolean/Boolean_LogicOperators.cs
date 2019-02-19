using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Aurora.Profiles;
using Newtonsoft.Json;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// Condition that checks a set of subconditions for atleast one of them being true.
    /// </summary>
    [OverrideLogic("Or", category: OverrideLogicCategory.Logic)]
    public class BooleanOr : IEvaluatableBoolean, IHasSubConditons {

        /// <summary>Creates a new Or evaluatable with no subconditions.</summary>
        public BooleanOr() { }
        /// <summary>Creates a new Or evaluatable with the given subconditions.</summary>
        public BooleanOr(IEnumerable<IEvaluatableBoolean> subconditions) {
            SubConditions = new ObservableCollection<IEvaluatableBoolean>(subconditions);
        }

        [JsonProperty]
        public ObservableCollection<IEvaluatableBoolean> SubConditions { get; set; } = new ObservableCollection<IEvaluatableBoolean>();

        public Visual GetControl(Application app) => new Control_SubconditionHolder(this, app, "Require atleast one of the following is true...");
        public bool Evaluate(IGameState gameState) => SubConditions.Any(subcondition => subcondition?.Evaluate(gameState) ?? false);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public void SetApplication(Application application) {
            foreach (var subcondition in SubConditions)
                subcondition.SetApplication(application);
        }

        public IEvaluatableBoolean Clone() => new BooleanOr { SubConditions = new ObservableCollection<IEvaluatableBoolean>(SubConditions.Select(e => e.Clone())) };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// Condition that checks a set of subconditions and requires them all to be true.
    /// </summary>
    [OverrideLogic("And", category: OverrideLogicCategory.Logic)]
    public class BooleanAnd : IEvaluatableBoolean, IHasSubConditons {

        /// <summary>Creates a new And evaluatable with no subconditions.</summary>
        public BooleanAnd() { }
        /// <summary>Creates a new And evaluatable with the given subconditions.</summary>
        public BooleanAnd(IEnumerable<IEvaluatableBoolean> subconditions) {
            SubConditions = new ObservableCollection<IEvaluatableBoolean>(subconditions);
        }

        [JsonProperty]
        public ObservableCollection<IEvaluatableBoolean> SubConditions { get; set; } = new ObservableCollection<IEvaluatableBoolean>();

        public Visual GetControl(Application app) => new Control_SubconditionHolder(this, app, "Require all of the following are true...");
        public bool Evaluate(IGameState gameState) => SubConditions.All(subcondition => subcondition?.Evaluate(gameState) ?? false);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public void SetApplication(Application application) {
            foreach (var subcondition in SubConditions)
                subcondition.SetApplication(application);
        }

        public IEvaluatableBoolean Clone() => new BooleanAnd { SubConditions = new ObservableCollection<IEvaluatableBoolean>(SubConditions.Select(e => { var x = e.Clone(); return x; })) };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }



    /// <summary>
    /// Condition that inverts another condition.
    /// </summary>
    [OverrideLogic("Not", category: OverrideLogicCategory.Logic)]
    public class BooleanNot : IEvaluatableBoolean {

        /// <summary>Creates a new NOT evaluatable with the default BooleanTrue subcondition.</summary>
        public BooleanNot() { }
        /// <summary>Creates a new NOT evaluatable which inverts the given subcondition.</summary>
        public BooleanNot(IEvaluatableBoolean subcondition) {
            SubCondition = subcondition;
        }

        [JsonProperty]
        public IEvaluatableBoolean SubCondition { get; set; } = new BooleanTrue();

        public Visual GetControl(Application app) => new Control_ConditionNot(this, app);
        public bool Evaluate(IGameState gameState) => !SubCondition.Evaluate(gameState);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public void SetApplication(Application application) {
            SubCondition?.SetApplication(application);
        }

        public IEvaluatableBoolean Clone() => new BooleanNot { SubCondition = SubCondition.Clone() };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// Condition that always returns true. Useful as a default condition as it means that
    /// the layer will always be visible.
    /// </summary>
    [OverrideLogic("True Constant", category: OverrideLogicCategory.Logic)]
    public class BooleanTrue : IEvaluatableBoolean {

        public Visual GetControl(Application application) => null;
        public bool Evaluate(IGameState _) => true;
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public void SetApplication(Application application) { }

        public IEvaluatableBoolean Clone() => new BooleanTrue();
        IEvaluatable IEvaluatable.Clone() => Clone();
    }


    /// <summary>
    /// Manually togglable condition. Useful for testing.
    /// </summary>
    [OverrideLogic("Manually Togglable", category: OverrideLogicCategory.Misc)]
    public class BooleanTogglable : IEvaluatableBoolean {

        public bool State { get; set; }

        public Visual GetControl(Application application) => new Control_ConditionDebug(this);
        public bool Evaluate(IGameState _) => State;
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public void SetApplication(Application application) { }

        public IEvaluatableBoolean Clone() => new BooleanTogglable { State = State };
        IEvaluatable IEvaluatable.Clone() => Clone();
    }

    /// <summary>
    /// Indicates that the implementing class has a SubCondition collection property.
    /// </summary>
    public interface IHasSubConditons {
        ObservableCollection<IEvaluatableBoolean> SubConditions { get; set; }
    }
}
