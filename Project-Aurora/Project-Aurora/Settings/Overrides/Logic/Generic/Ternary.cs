using Aurora.Profiles;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;


namespace Aurora.Settings.Overrides.Logic
{
    //[OverrideLogic("Numeric State Variable", category: OverrideLogicCategory.State)]
    public class TernaryGeneric<T> : IEvaluatable<T>
    {
        /// <summary>
        /// Evaluatable Condition that determines if CaseTrue or CaseFalse should be returned
        /// </summary>
        public IEvaluatable<bool> Condition { get; set; } = new BooleanConstant();
        public IEvaluatable<T> CaseTrue { get; set; } = (IEvaluatable<T>)EvaluatableTypeResolver.GetDefault(EvaluatableTypeResolver.GetEvaluatableType(typeof(IEvaluatable<T>)));
        public IEvaluatable<T> CaseFalse { get; set; } = (IEvaluatable<T>)EvaluatableTypeResolver.GetDefault(EvaluatableTypeResolver.GetEvaluatableType(typeof(IEvaluatable<T>)));

        /// <summary>Creates a new ternary evaluatable that doesn't target anything.</summary>
        public TernaryGeneric() {
        }
        /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
        public TernaryGeneric(IEvaluatable<bool> condition, IEvaluatable<T> caseTrue, IEvaluatable<T> caseFalse) : this() { Condition = condition; CaseTrue = caseTrue; CaseFalse = caseFalse; }

        // Path to the GSI variable
        public string VariablePath { get; set; }

        public Visual GetControl(Application application) => new Control_Ternary<T>(this, application);

        /// <summary>Evaluate Condition and return the appropriate evaluation</summary>
        public T Evaluate(IGameState gameState) => Condition.Evaluate(gameState) ? CaseTrue.Evaluate(gameState) : CaseFalse.Evaluate(gameState);
        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Update the applications of the children evaluatables.</summary>
        public void SetApplication(Application application)
        {
            Condition?.SetApplication(application);
            CaseTrue?.SetApplication(application);
            CaseFalse?.SetApplication(application);
        }

        public IEvaluatable<T> Clone() => new TernaryGeneric<T>(Condition.Clone(), CaseTrue.Clone(), CaseFalse.Clone());
        IEvaluatable IEvaluatable.Clone() => Clone();
    }

    [OverrideLogic("If - Else (Ternary)", category: OverrideLogicCategory.Logic)]
    public class TernaryBoolean : TernaryGeneric<bool>
    {
        /// <summary>Creates a new ternary evaluatable that doesn't target anything.</summary>
        public TernaryBoolean() : base() { }

        /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
        public TernaryBoolean(IEvaluatable<bool> condition, IEvaluatable<bool> caseTrue, IEvaluatable<bool> caseFalse) : base(condition, caseTrue, caseFalse) { }
    }

    [OverrideLogic("If - Else (Ternary)", category: OverrideLogicCategory.Logic)]
    public class TernaryNumeric : TernaryGeneric<double>
    {
        /// <summary>Creates a new ternary evaluatable that doesn't target anything.</summary>
        public TernaryNumeric() : base() { }

        /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
        public TernaryNumeric(IEvaluatable<bool> condition, IEvaluatable<double> caseTrue, IEvaluatable<double> caseFalse) : base(condition, caseTrue, caseFalse) { }
    }

    [OverrideLogic("If - Else (Ternary)", category: OverrideLogicCategory.Logic)]
    public class TernaryString : TernaryGeneric<string>
    {
        /// <summary>Creates a new ternary evaluatable that doesn't target anything.</summary>
        public TernaryString() : base() { }

        /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
        public TernaryString(IEvaluatable<bool> condition, IEvaluatable<string> caseTrue, IEvaluatable<string> caseFalse) : base(condition, caseTrue, caseFalse) { }
    }
}
