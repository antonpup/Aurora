using Aurora.Profiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;


namespace Aurora.Settings.Overrides.Logic
{
    //[OverrideLogic("Numeric State Variable", category: OverrideLogicCategory.State)]
    public class IfElseGeneric<T> : IEvaluatable<T> {
        /// <summary>
        /// A list of all branches of the conditional.
        /// </summary>
        public ObservableCollection<Branch> Cases { get; set; } = CreateDefaultCases(
            new BooleanConstant(), // Condition
            (IEvaluatable<T>)EvaluatableTypeResolver.GetDefault(EvaluatableTypeResolver.GetEvaluatableType(typeof(IEvaluatable<T>))), // True
            (IEvaluatable<T>)EvaluatableTypeResolver.GetDefault(EvaluatableTypeResolver.GetEvaluatableType(typeof(IEvaluatable<T>))) // False
        );

        /// <summary>Creates a new If-Else evaluatable with default evaluatables.</summary>
        public IfElseGeneric() { }
        /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
        public IfElseGeneric(IEvaluatable<bool> condition, IEvaluatable<T> caseTrue, IEvaluatable<T> caseFalse) : this() { Cases = CreateDefaultCases(condition, caseTrue, caseFalse); }
        /// <summary>Creates a new evaluatable using the given case tree.</summary>
        public IfElseGeneric(ObservableCollection<Branch> cases) : this() { Cases = cases; }

        public Visual GetControl(Application application) => new Control_Ternary<T>(this, application);

        /// <summary>Evaluate conditions and return the appropriate evaluation.</summary>
        public T Evaluate(IGameState gameState) {
            foreach (var branch in Cases)
                if (branch.Condition == null || branch.Condition.Evaluate(gameState)) // Find the first with a true condition, or where the condition is null (which indicates 'else')
                    return branch.Value.Evaluate(gameState);
            return default(T);
        }

        object IEvaluatable.Evaluate(IGameState gameState) => Evaluate(gameState);

        /// <summary>Update the applications of the children evaluatables.</summary>
        public void SetApplication(Application application) {
            foreach (var kvp in Cases) {
                kvp.Condition?.SetApplication(application);
                kvp.Value?.SetApplication(application);
            }
        }

        public IEvaluatable<T> Clone() => new IfElseGeneric<T>(new ObservableCollection<Branch>(Cases));
        IEvaluatable IEvaluatable.Clone() => Clone();

        private static ObservableCollection<Branch> CreateDefaultCases(IEvaluatable<bool> condition, IEvaluatable<T> caseTrue, IEvaluatable<T> caseFalse) =>
            new ObservableCollection<Branch> {
                new Branch(condition, caseTrue),
                new Branch(null, caseFalse)
            };

        public class Branch {
            public IEvaluatable<bool> Condition { get; set; }
            public IEvaluatable<T> Value { get; set; }

            public Branch() { }
            public Branch(IEvaluatable<bool> condition, IEvaluatable<T> value) { Condition = condition; Value = value; }
        }
    }


    [OverrideLogic("If - Else If - Else", category: OverrideLogicCategory.Logic)]
    public class IfElseBoolean : IfElseGeneric<bool> {
        /// <summary>Creates a new If-Else evaluatable with default evaluatables.</summary>
        public IfElseBoolean() : base() { }

        /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
        public IfElseBoolean(IEvaluatable<bool> condition, IEvaluatable<bool> caseTrue, IEvaluatable<bool> caseFalse) : base(condition, caseTrue, caseFalse) { }

        /// <summary>Creates a new evaluatable using the given case tree.</summary>
        public IfElseBoolean(ObservableCollection<Branch> cases) : base(cases) { }
    }


    [OverrideLogic("If - Else If - Else", category: OverrideLogicCategory.Logic)]
    public class IfElseNumeric : IfElseGeneric<double> {
        /// <summary>Creates a new If-Else evaluatable with default evaluatables.</summary>
        public IfElseNumeric() : base() { }

        /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
        public IfElseNumeric(IEvaluatable<bool> condition, IEvaluatable<double> caseTrue, IEvaluatable<double> caseFalse) : base(condition, caseTrue, caseFalse) { }

        /// <summary>Creates a new evaluatable using the given case tree.</summary>
        public IfElseNumeric(ObservableCollection<Branch> cases) : base(cases) { }
    }


    [OverrideLogic("If - Else If - Else", category: OverrideLogicCategory.Logic)]
    public class IfElseString : IfElseGeneric<string> {
        /// <summary>Creates a new If-Else evaluatable with default evaluatables.</summary>
        public IfElseString() : base() { }

        /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
        public IfElseString(IEvaluatable<bool> condition, IEvaluatable<string> caseTrue, IEvaluatable<string> caseFalse) : base(condition, caseTrue, caseFalse) { }

        /// <summary>Creates a new evaluatable using the given case tree.</summary>
        public IfElseString(ObservableCollection<Branch> cases) : base(cases) { }
    }
}
