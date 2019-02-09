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
    public class BooleanOr : IEvaluatableBoolean {

        [JsonProperty]
        private ObservableCollection<IEvaluatableBoolean> subconditions = new ObservableCollection<IEvaluatableBoolean>();
        
        public Visual GetControl(Application app) => new Control_SubconditionHolder(subconditions, app, "Require atleast one of the following is true...");
        public bool Evaluate(IGameState gameState) => subconditions.Any(subcondition => subcondition?.Evaluate(gameState) ?? false);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public void SetApplication(Application application) {
            foreach (var subcondition in subconditions)
                subcondition.SetApplication(application);
        }
    }


    /// <summary>
    /// Condition that checks a set of subconditions and requires them all to be true.
    /// </summary>
    [OverrideLogic("And", category: OverrideLogicCategory.Logic)]
    public class BooleanAnd : IEvaluatableBoolean {

        [JsonProperty]
        private ObservableCollection<IEvaluatableBoolean> subconditions = new ObservableCollection<IEvaluatableBoolean>();
        
        public Visual GetControl(Application app) => new Control_SubconditionHolder(subconditions, app, "Require all of the following are true...");
        public bool Evaluate(IGameState gameState) => subconditions.All(subcondition => subcondition?.Evaluate(gameState) ?? false);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public void SetApplication(Application application) {
            foreach (var subcondition in subconditions)
                subcondition.SetApplication(application);
        }
    }



    /// <summary>
    /// Condition that inverts another condition.AUR
    /// </summary>
    [OverrideLogic("Not", category: OverrideLogicCategory.Logic)]
    public class BooleanNot : IEvaluatableBoolean {

        [JsonProperty]
        public IEvaluatableBoolean SubCondition { get; set; } = new BooleanTrue();
        
        public Visual GetControl(Application app) => new Control_ConditionNot(this, app);
        public bool Evaluate(IGameState gameState) => !SubCondition.Evaluate(gameState);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        public void SetApplication(Application application) {
            SubCondition.SetApplication(application);
        }
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
    }
}
