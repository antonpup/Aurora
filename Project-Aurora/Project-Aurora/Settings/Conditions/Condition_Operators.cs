using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Aurora.Profiles;
using Newtonsoft.Json;

namespace Aurora.Settings.Conditions {

    /// <summary>
    /// Condition that checks a set of subconditions for atleast one of them being true.
    /// </summary>
    [Condition("Logical Or")]
    public class ConditionOr : ICondition {

        [JsonProperty]
        private ObservableCollection<ICondition> subconditions = new ObservableCollection<ICondition>();
        
        public UserControl GetControl(Application app) => new Control_SubconditionHolder(subconditions, app, "Require atleast one of the following is true...");
        public bool Evaluate(IGameState gameState) => subconditions.Any(subcondition => subcondition?.Evaluate(gameState) ?? false);

        public void SetApplication(Application application) {
            foreach (var subcondition in subconditions)
                subcondition.SetApplication(application);
        }
    }


    /// <summary>
    /// Condition that checks a set of subconditions and requires them all to be true.
    /// </summary>
    [Condition("Logical And")]
    public class ConditionAnd : ICondition {

        [JsonProperty]
        private ObservableCollection<ICondition> subconditions = new ObservableCollection<ICondition>();
        
        public UserControl GetControl(Application app) => new Control_SubconditionHolder(subconditions, app, "Require all of the following are true...");
        public bool Evaluate(IGameState gameState) => subconditions.All(subcondition => subcondition?.Evaluate(gameState) ?? false);

        public void SetApplication(Application application) {
            foreach (var subcondition in subconditions)
                subcondition.SetApplication(application);
        }
    }


    /// <summary>
    /// Condition that inverts another condition.AUR
    /// </summary>
    [Condition("Logical Not")]
    public class ConditionNot : ICondition {

        [JsonProperty]
        public ICondition SubCondition { get; set; } = new ConditionTrue();
        
        public UserControl GetControl(Application app) => new Control_ConditionNot(this, app);
        public bool Evaluate(IGameState gameState) => !SubCondition.Evaluate(gameState);

        public void SetApplication(Application application) {
            SubCondition.SetApplication(application);
        }
    }


    /// <summary>
    /// Condition that always returns true. Useful as a default condition as it means that
    /// the layer will always be visible.
    /// </summary>
    [Condition("True Constant")]
    public class ConditionTrue : ICondition {
        
        public UserControl GetControl(Application application) => null;
        public bool Evaluate(IGameState _) => true;

        public void SetApplication(Application application) { }
    }


    [Condition("Debug")]
    public class ConditionDebug : ICondition {
        
        public bool State { get; set; }
        
        public UserControl GetControl(Application application) => new Control_ConditionDebug(this);
        public bool Evaluate(IGameState _) => State;

        public void SetApplication(Application application) { }
    }
}
